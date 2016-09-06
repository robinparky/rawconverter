using RawConverter.Common;
using RawConverter.MassSpec;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Factorization;
using MathNet.Numerics;
using lpsolve55;
using System.Diagnostics;
using System.Windows.Forms;

namespace RawConverter.DIADataProcess
{
    class PrecursorPredictor
    {
        private const double MAX_CHECK_FORWARD_MZ = 2.1;
        private const int MAX_MASS_IN_AVERAGINE_TABLE = 7850;
        private const int MAX_ISOTOPE_NUM = 11;
        private double[,] AveragineTable = null;
        private int[] IsotopeNumArr = null;
        public double Threshold { set; get; }

        private int MinCharge = 1;
        private int MaxCharge = 6;

        private StreamWriter dbgWr = null;

        public double IsolationWindowSize { set; get; }

        public PrecursorPredictor(double isolationWindowSize, int minCharge, int maxCharge, double threshold)
        {
            IsolationWindowSize = isolationWindowSize;
            MinCharge = minCharge;
            MaxCharge = maxCharge;
            Threshold = threshold;
            InitializeAveragineTable();
            //if (dbgWr == null)
            //{
            //    dbgWr = new StreamWriter("D:/dia_dump.txt");
            //}
        }

        private void InitializeAveragineTable()
        {
            int len = MAX_MASS_IN_AVERAGINE_TABLE / 50 + 1;
            IsotopeNumArr = new int[len];
            AveragineTable = new double[len, MAX_ISOTOPE_NUM];
            try
            {
                using (StreamReader sr = new StreamReader("AveragineTable.txt"))
                {
                    while (sr.Peek() >= 0)
                    {
                        string line = sr.ReadLine();
                        string[] elems = line.Split('\t');
                        int intMass = int.Parse(elems[0]);
                        int idx = intMass / 50;
                        if (idx >= len)
                        {
                            sr.Close();
                            break;
                        }
                        IsotopeNumArr[idx] = int.Parse(elems[3]);
                        int max = IsotopeNumArr[idx] + 4;
                        for (int i = 4; i < max; i++)
                        {
                            AveragineTable[idx, i - 4] = double.Parse(elems[i]);
                        }
                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error occured when reading averagine_table.txt");
                Console.WriteLine(e.Message);
            }
        }

        public void PredictPrecursors(ref MassSpectrum curSpec, ref MassSpectrum precSpec)
        {
            // set the precursor scan number for current spectrum;
            curSpec.PrecursorScanNumber = precSpec.ScanNumber;

            // set the correct isolation window size, becuase in the initiliazation of PrecursorPredictor 
            // a default isolation window size (10 mu, ie. 5 mu in each direction) was set. This default set 
            // is for the case that no isolation window size can be extract from the scan header;
            IsolationWindowSize = curSpec.IsolationWindowSize > 0 ? curSpec.IsolationWindowSize : IsolationWindowSize;

            // get the precursor m/z from the scan filter;
            double precMz = curSpec.Precursors.Count == 0 ? 0 : curSpec.Precursors[0].Item1;
            int precZ = curSpec.Precursors.Count == 0 ? 0 : curSpec.Precursors[0].Item2;
            if (precZ > 6)
            {
                return;
            }

            // get the precursor MS1 spectrum;
            List<Ion> peaks = precSpec.Peaks;
            // get the highest peak intensity in peaks;
            double globalMaxIntensity = double.MinValue;
            foreach (Ion p in peaks)
            {
                if (p.Intensity > globalMaxIntensity)
                {
                    globalMaxIntensity = p.Intensity;
                }
            }
            // get the peaks dropped in a designated window;
            List<Ion> peaksInWindow = new List<Ion>();

            foreach (Ion peak in peaks)
            {
                if (peak.MZ >= precMz - IsolationWindowSize / 2 - 1 && peak.MZ <= precMz + IsolationWindowSize / 2 + 1)
                {
                    // filter with the relative intensity;
                    if (peak.Intensity / globalMaxIntensity >= 0.0005)
                    {
                        peaksInWindow.Add(peak);
                    }
                }
            }

            // sort the peaks;
            peaksInWindow.Sort(delegate(Ion p1, Ion p2)
            {
                if (p1.MZ < p2.MZ)
                    return -1;
                else if (p1.MZ > p2.MZ)
                    return 1;
                else
                    return 0;
            });

            /* Lin He added for debugging;
            dbgWr.WriteLine("Scan " + curSpec.ScanNumber);
            foreach (Ion peak in peaksInWindow)
            {
                dbgWr.WriteLine(peak.MZ + " " + peak.Intensity);
            }
            dbgWr.WriteLine();
            dbgWr.Flush();
            //*/

            // get all the possible envelopes;
            List<Envelope> envList = PredictPrecursors(peaksInWindow);
            if (envList.Count > 0)
            {
                
                curSpec.Precursors.Clear();
                curSpec.PrecursorRefined = true;
                foreach (Envelope env in envList) 
                {
                    if (env.Score >= Threshold)
                    {
                        curSpec.Precursors.Add(new Tuple<double, int>(env.MonoisotPeak.MZ, env.Charge));
                    }
                }
            }
        }

        public List<Envelope> PredictPrecursors(List<Ion> peaks)
        {
            return PredictPrecursors(peaks, MinCharge, MaxCharge);
        }

        /// <summary>
        /// Predict the precursor m/z's and charges of all possible peptides.
        /// </summary>
        /// <param name="peaks">All peaks in an m/z window.</param>
        /// <param name="minCharge">The minimum charge considered in the prediction.</param>
        /// <param name="maxCharge">The minimum charge considered in the prediction.</param>
        /// <returns>Prediected precursor list.</returns>
        public List<Envelope> PredictPrecursors(List<Ion> peaks, int minCharge, int maxCharge) 
        {
            // confirm the minimum charge state is valid;
            if (minCharge <= 0)
            {
                minCharge = 1;
            }

            // define the precursor list returned after the search;
            List<Envelope> envList = new List<Envelope>();
            
            // skip if the current spectrum is MS1;
            if (peaks == null || peaks.Count == 0)
            {
                return envList;
            }
            
            // maybe not require to centroid, we only get the centroided 
            // centrod the peaks;
            //List<Ion> centroidedPeaks = new List<Ion>();
            //double lastMz = 0, lastIntensity = 0;
            //bool reachedPeak = false;
            //double mzSum = 0, intensitySum = 0;
            //int countedPeakNum = 0;
            //for (int i = 0; i < peaksInWindow.Count; i++)
            //{
            //    Ion peak = peaksInWindow[i];
            //    if (!reachedPeak && peak.Intensity >= lastIntensity)
            //    {
            //        mzSum += peak.MZ;
            //        intensitySum += peak.Intensity;
            //        lastIntensity = peak.Intensity;
            //        countedPeakNum++;
            //    }
            //    else if (!reachedPeak && peak.Intensity < lastIntensity)
            //    {
            //        reachedPeak = true;
            //        mzSum += peak.MZ;
            //        intensitySum += peak.Intensity;
            //        lastIntensity = peak.Intensity;
            //        countedPeakNum++;
            //    }
            //    else if (reachedPeak && peak.Intensity > lastIntensity)
            //    {
            //        lastMz = peak.MZ;
            //        lastIntensity = peak.Intensity;
            //        reachedPeak = false;
            //        double mzMean = mzSum / countedPeakNum;
            //        centroidedPeaks.Add(new Ion(mzMean, intensitySum));
            //        mzSum = peak.MZ;
            //        intensitySum = peak.Intensity;
            //        countedPeakNum = 1;
            //    }
            //}

            // get the highest peak;
            Ion highestPeak = peaks[0];
            foreach (Ion peak in peaks)
            {
                if (peak.Intensity > highestPeak.Intensity)
                {
                    highestPeak = peak;
                }
            }
            
            // generate all the possible envelopes;
            for (int i = 0; i < peaks.Count; i++)
            {
                Ion startPeak = peaks[i];

                double err = 10 * startPeak.MZ / 1e6;
                List<Envelope> localEnvList = new List<Envelope>();
                for (int z = minCharge; z <= maxCharge; z++)
                {
                    double stepMz = Utils.MASS_DIFF_C12_C13 / (double)z;
                    double previousMz = startPeak.MZ;
                    List<Ion> env = new List<Ion>();
                    env.Add(peaks[i]);

                    // get the theoretical isotopic distribution;
                    double precMH = (previousMz - Utils.PROTON_MASS) * z + Utils.PROTON_MASS;
                    // simply skip the correction for spectrum with a large mass;
                    if (precMH > MAX_MASS_IN_AVERAGINE_TABLE)
                    {
                        break;
                    }
                    int avgTblIdx = (int)(precMH + 25) / 50;
                    int isotNum = IsotopeNumArr[avgTblIdx];
                    double[] theoIsotDist = new double[isotNum];
                    for (int idx = 0; idx < isotNum; idx++)
                    {
                        theoIsotDist[idx] = AveragineTable[avgTblIdx, idx];
                    }

                    // propagate the envelope;
                    for (int j = i + 1; j < peaks.Count; j++)
                    {
                        // only check limited forward m/z values;
                        if (env.Count >= theoIsotDist.Length || peaks[j].MZ - startPeak.MZ > MAX_CHECK_FORWARD_MZ)
                        {
                            break;
                        } 

                        // add into the envelop if the current peak has a specific m/z gap with the previous one in the env;
                        if (Math.Abs(peaks[j].MZ - previousMz - stepMz) <= err)
                        {
                            env.Add(peaks[j].Clone());
                            previousMz = peaks[j].MZ;
                        }
                    }
                    
                    // if the observed envelope has a smaller length, add fake peaks;
                    //while (env.Count < isotNum)
                    //{
                    //    Ion lastPeak = env[env.Count - 1];
                    //    env.Add(new Ion(lastPeak.MZ + stepMz, 0));
                    //}

                    if (env.Count > 2)
                    {
                        localEnvList.Add(new Envelope(startPeak.Clone(), z, env, theoIsotDist, highestPeak));
                    }
                }
                envList.AddRange(localEnvList.AsEnumerable());
                //if (localEnvList.Count > 0) 
                //{
                //    // sort the localEnvList according to the scores decendingly, and remove the bad ones;
                //    localEnvList.Sort(delegate(Envelope e1, Envelope e2)
                //    {
                //        if (e1.Score < e2.Score)
                //            return -1;
                //        else if (e1.Score > e2.Score)
                //            return 1;
                //        else
                //            return 0;
                //    });

                //    // add top two envelopes to envList;
                //    int topNum = localEnvList.Count > 2 ? 2 : localEnvList.Count;
                //    for (int topIdx = 0; topIdx < topNum; topIdx++) 
                //    {
                //        envList.Add(localEnvList[topIdx]);
                //    }
                //}
            }

            if (envList.Count == 0)
            {
                return envList;
            }

            // calculate the best combination to filter the envelope list;
            FilterEnvelopesByLP(envList);

            // remove all the impossible subsets;
            RemoveFalseSubsets(envList);
            RescoreEnvelopes(envList);

            // sort the envList according to the scores decendingly;
            envList.Sort(delegate(Envelope e1, Envelope e2)
            {
                if (e1.Score < e2.Score)
                    return 1;
                else if (e1.Score > e2.Score)
                    return -1;
                else
                    return 0;
            });

            // sort the envList according to the abundence decendingly;
            envList.Sort(delegate(Envelope e1, Envelope e2)
            {
                if (e1.MonoisotPeak.Intensity < e2.MonoisotPeak.Intensity)
                    return 1;
                else if (e1.MonoisotPeak.Intensity > e2.MonoisotPeak.Intensity)
                    return -1;
                else
                    return 0;
            });

            // This part is used for only select one precursor for each isolation window;
            //if (envList.Count > 0)
            //{
            //    envList.RemoveAll(elem => elem.MonoisotPeak.Intensity < envList[0].MonoisotPeak.Intensity);
            //}
            
            return envList;
        }

        private void RescoreEnvelopes(List<Envelope> envList)
        {
            // get all the peaks involved in these envlopes and sort according to the m/z values ascendingly;
            List<Ion> peakList = new List<Ion>();
            foreach (Envelope env in envList)
            {
                foreach (Ion peak in env.PeaksInEnvelope)
                {
                    int idx = peakList.FindIndex(
                        delegate(Ion p)
                        {
                            return (p.MZ == peak.MZ && p.Intensity == peak.Intensity);
                        }
                        );
                    if (idx < 0)
                    {
                        peakList.Add(peak.Clone());
                    }
                }
            }
            peakList.Sort(delegate(Ion p1, Ion p2)
            {
                if (p1.MZ < p2.MZ)
                    return -1;
                else if (p1.MZ > p2.MZ)
                    return 1;
                else
                    return 0;
            });

            foreach (Ion peak in peakList)
            {
                // for each peak, get its involved envelopes;
                Dictionary<Envelope, int> involvedEnvs = new Dictionary<Envelope, int>();
                foreach (Envelope env in envList)
                {
                    int idx = env.PeaksInEnvelope.FindIndex(
                        delegate(Ion p) 
                        {
                            return (p.MZ == peak.MZ && p.Intensity == peak.Intensity);
                        });
                    if (idx >= 0)
                    {
                        involvedEnvs.Add(env, idx);
                    }
                }

                // calculate the actual intensities of peaks in each envelope;
                double sum = 0;
                foreach (Envelope env in involvedEnvs.Keys)
                {
                    int idx = involvedEnvs[env];
                    sum += env.FractionQuantity * env.TheoIsotDist[idx];
                }
                foreach (Envelope env in involvedEnvs.Keys)
                {
                    int idx = involvedEnvs[env];
                    env.PeaksInEnvelope[idx].Intensity = (env.FractionQuantity * env.TheoIsotDist[idx]) / sum * peak.Intensity;
                }
            }

            // trigger the fitting score recalculation for each envelope;
            foreach (Envelope env in envList)
            {
                env.UpdateRequired = true;
            }
        }

        /* unsafe is needed to make sure that these function are not relocated in memory by the CLR. If that would happen, a crash occurs */
        /* go to the project property page and in onfiguration properties>build?set Allow Unsafe Code Blocks to True. */
        /* see http://msdn2.microsoft.com/en-US/library/chfa2zb8.aspx and http://msdn2.microsoft.com/en-US/library/t2yzs44b.aspx */
        private /* unsafe */ static void logfunc(IntPtr lp, int userhandle, string Buf)
        {
            System.Diagnostics.Debug.Write(Buf);
        }

        private /* unsafe */ static bool ctrlcfunc(IntPtr lp, int userhandle)
        {
            /* 'If set to true, then solve is aborted and returncode will indicate this. */
            return (false);
        }

        private /* unsafe */ static void msgfunc(IntPtr lp, int userhandle, lpsolve.lpsolve_msgmask message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }

        private static void ThreadProc(object filename)
        {
            IntPtr lp;
            lpsolve.lpsolve_return ret;
            double o;

            lp = lpsolve.read_LP((string)filename, 0, "");
            ret = lpsolve.solve(lp);
            o = lpsolve.get_objective(lp);
            Debug.Assert(ret == lpsolve.lpsolve_return.OPTIMAL && Math.Round(o, 13) == 1779.4810350637485);
            lpsolve.delete_lp(lp);
        }
        private static void Test()
        {
            const string NewLine = "\n";

            IntPtr lp;
            int release = 0, Major = 0, Minor = 0, build = 0;
            double[] Row;
            double[] Lower;
            double[] Upper;
            double[] Col;
            double[] Arry;

            lp = lpsolve.make_lp(0, 4);

            lpsolve.lp_solve_version(ref Major, ref Minor, ref release, ref build);

            /* let's first demonstrate the logfunc callback feature */
            lpsolve.put_logfunc(lp, new lpsolve.logfunc(logfunc), 0);
            lpsolve.print_str(lp, "lp_solve " + Major + "." + Minor + "." + release + "." + build + " demo" + NewLine + NewLine);
            lpsolve.solve(lp); /* just to see that a message is send via the logfunc routine ... */
            /* ok, that is enough, no more callback */
            lpsolve.put_logfunc(lp, null, 0);

            /* Now redirect all output to a file */
            lpsolve.set_outputfile(lp, "result.txt");

            /* set an abort function. Again optional */
            lpsolve.put_abortfunc(lp, new lpsolve.ctrlcfunc(ctrlcfunc), 0);

            /* set a message function. Again optional */
            lpsolve.put_msgfunc(lp, new lpsolve.msgfunc(msgfunc), 0, (int)(lpsolve.lpsolve_msgmask.MSG_PRESOLVE | lpsolve.lpsolve_msgmask.MSG_LPFEASIBLE | lpsolve.lpsolve_msgmask.MSG_LPOPTIMAL | lpsolve.lpsolve_msgmask.MSG_MILPEQUAL | lpsolve.lpsolve_msgmask.MSG_MILPFEASIBLE | lpsolve.lpsolve_msgmask.MSG_MILPBETTER));

            lpsolve.print_str(lp, "lp_solve " + Major + "." + Minor + "." + release + "." + build + " demo" + NewLine + NewLine);
            lpsolve.print_str(lp, "This demo will show most of the features of lp_solve " + Major + "." + Minor + "." + release + "." + build + NewLine);

            lpsolve.print_str(lp, NewLine + "We start by creating a new problem with 4 variables and 0 constraints" + NewLine);
            lpsolve.print_str(lp, "We use: lp = lpsolve.make_lp(0, 4);" + NewLine);

            lpsolve.set_timeout(lp, 0);

            lpsolve.print_str(lp, "We can show the current problem with lpsolve.print_lp(lp);" + NewLine);
            lpsolve.print_lp(lp);

            lpsolve.print_str(lp, "Now we add some constraints" + NewLine);
            lpsolve.print_str(lp, "lpsolve.add_constraint(lp, Row, lpsolve.lpsolve_constr_types.LE, 4);" + NewLine);
            // pay attention to the 1 base and ignored 0 column for constraints
            lpsolve.add_constraint(lp, new double[] { 0, 3, 2, 2, 1 }, lpsolve.lpsolve_constr_types.LE, 4);
            lpsolve.print_lp(lp);

            // check ROW array works
            Row = new double[] { 0, 0, 4, 3, 1 };
            lpsolve.print_str(lp, "lpsolve.add_constraint(lp, Row, lpsolve.lpsolve_constr_types.GE, 3);" + NewLine);
            lpsolve.add_constraint(lp, Row, lpsolve.lpsolve_constr_types.GE, 3);
            lpsolve.print_lp(lp);

            lpsolve.print_str(lp, "Set the objective function" + NewLine);
            lpsolve.print_str(lp, "lpsolve.set_obj_fn(lp, Row);" + NewLine);
            lpsolve.set_obj_fn(lp, new double[] { 0, 2, 3, -2, 3 });
            lpsolve.print_lp(lp);

            lpsolve.print_str(lp, "Now solve the problem with lpsolve.solve(lp);" + NewLine);
            lpsolve.print_str(lp, lpsolve.solve(lp) + ": " + lpsolve.get_objective(lp) + NewLine);

            Col = new double[lpsolve.get_Ncolumns(lp)];
            lpsolve.get_variables(lp, Col);

            Row = new double[lpsolve.get_Nrows(lp)];
            lpsolve.get_constraints(lp, Row);

            Arry = new double[lpsolve.get_Ncolumns(lp) + lpsolve.get_Nrows(lp) + 1];
            lpsolve.get_dual_solution(lp, Arry);

            Arry = new double[lpsolve.get_Ncolumns(lp) + lpsolve.get_Nrows(lp)];
            Lower = new double[lpsolve.get_Ncolumns(lp) + lpsolve.get_Nrows(lp)];
            Upper = new double[lpsolve.get_Ncolumns(lp) + lpsolve.get_Nrows(lp)];
            lpsolve.get_sensitivity_rhs(lp, Arry, Lower, Upper);

            Lower = new double[lpsolve.get_Ncolumns(lp) + 1];
            Upper = new double[lpsolve.get_Ncolumns(lp) + 1];
            lpsolve.get_sensitivity_obj(lp, Lower, Upper);

            lpsolve.print_str(lp, "The value is 0, this means we found an optimal solution" + NewLine);
            lpsolve.print_str(lp, "We can display this solution with lpsolve.print_solution(lp);" + NewLine);
            lpsolve.print_objective(lp);
            lpsolve.print_solution(lp, 1);
            lpsolve.print_constraints(lp, 1);

            lpsolve.print_str(lp, "The dual variables of the solution are printed with" + NewLine);
            lpsolve.print_str(lp, "lpsolve.print_duals(lp);" + NewLine);
            lpsolve.print_duals(lp);

            lpsolve.print_str(lp, "We can change a single element in the matrix with" + NewLine);
            lpsolve.print_str(lp, "lpsolve.set_mat(lp, 2, 1, 0.5);" + NewLine);
            lpsolve.set_mat(lp, 2, 1, 0.5);
            lpsolve.print_lp(lp);

            lpsolve.print_str(lp, "If we want to maximize the objective function use lpsolve.set_maxim(lp);" + NewLine);
            lpsolve.set_maxim(lp);
            lpsolve.print_lp(lp);

            lpsolve.print_str(lp, "after solving this gives us:" + NewLine);
            lpsolve.solve(lp);
            lpsolve.print_objective(lp);
            lpsolve.print_solution(lp, 1);
            lpsolve.print_constraints(lp, 1);
            lpsolve.print_duals(lp);

            lpsolve.print_str(lp, "Change the value of a rhs element with lpsolve.set_rh(lp, 1, 7.45);" + NewLine);
            lpsolve.set_rh(lp, 1, 7.45);
            lpsolve.print_lp(lp);
            lpsolve.solve(lp);
            lpsolve.print_objective(lp);
            lpsolve.print_solution(lp, 1);
            lpsolve.print_constraints(lp, 1);

            lpsolve.print_str(lp, "We change C4 to the integer type with" + NewLine);
            lpsolve.print_str(lp, "lpsolve.set_int(lp, 4, true);" + NewLine);
            lpsolve.set_int(lp, 4, true);
            lpsolve.print_lp(lp);

            lpsolve.print_str(lp, "We set branch & bound debugging on with lpsolve.set_debug(lp, true);" + NewLine);

            lpsolve.set_debug(lp, true);
            lpsolve.print_str(lp, "and solve..." + NewLine);

            lpsolve.solve(lp);
            lpsolve.print_objective(lp);
            lpsolve.print_solution(lp, 1);
            lpsolve.print_constraints(lp, 1);

            lpsolve.print_str(lp, "We can set bounds on the variables with" + NewLine);
            lpsolve.print_str(lp, "lpsolve.set_lowbo(lp, 2, 2); & lpsolve.set_upbo(lp, 4, 5.3);" + NewLine);
            lpsolve.set_lowbo(lp, 2, 2);
            lpsolve.set_upbo(lp, 4, 5.3);
            lpsolve.print_lp(lp);

            lpsolve.solve(lp);
            lpsolve.print_objective(lp);
            lpsolve.print_solution(lp, 1);
            lpsolve.print_constraints(lp, 1);

            lpsolve.print_str(lp, "Now remove a constraint with lpsolve.del_constraint(lp, 1);" + NewLine);
            lpsolve.del_constraint(lp, 1);
            lpsolve.print_lp(lp);
            lpsolve.print_str(lp, "Add an equality constraint" + NewLine);
            Row = new double[] { 0, 1, 2, 1, 4 };
            lpsolve.add_constraint(lp, Row, lpsolve.lpsolve_constr_types.EQ, 8);
            lpsolve.print_lp(lp);

            lpsolve.print_str(lp, "A column can be added with:" + NewLine);
            lpsolve.print_str(lp, "lpsolve.add_column(lp, Col);" + NewLine);
            lpsolve.add_column(lp, new double[] { 3, 2, 2 });
            lpsolve.print_lp(lp);

            lpsolve.print_str(lp, "A column can be removed with:" + NewLine);
            lpsolve.print_str(lp, "lpsolve.del_column(lp, 3);" + NewLine);
            lpsolve.del_column(lp, 3);
            lpsolve.print_lp(lp);

            lpsolve.print_str(lp, "We can use automatic scaling with:" + NewLine);
            lpsolve.print_str(lp, "lpsolve.set_scaling(lp, lpsolve.lpsolve_scales.SCALE_MEAN);" + NewLine);
            lpsolve.set_scaling(lp, lpsolve.lpsolve_scales.SCALE_MEAN);
            lpsolve.print_lp(lp);

            lpsolve.print_str(lp, "The function lpsolve.get_mat(lp, row, column); returns a single" + NewLine);
            lpsolve.print_str(lp, "matrix element" + NewLine);
            lpsolve.print_str(lp, "lpsolve.get_mat(lp, 2, 3); lpsolve.get_mat(lp, 1, 1); gives " + lpsolve.get_mat(lp, 2, 3) + ", " + lpsolve.get_mat(lp, 1, 1) + NewLine);
            lpsolve.print_str(lp, "Notice that get_mat returns the value of the original unscaled problem" + NewLine);

            lpsolve.print_str(lp, "If there are any integer type variables, then only the rows are scaled" + NewLine);
            lpsolve.print_str(lp, "lpsolve.set_int(lp, 3, false);" + NewLine);
            lpsolve.set_int(lp, 3, false);
            lpsolve.print_lp(lp);

            lpsolve.solve(lp);
            lpsolve.print_str(lp, "print_solution gives the solution to the original problem" + NewLine);
            lpsolve.print_objective(lp);
            lpsolve.print_solution(lp, 1);
            lpsolve.print_constraints(lp, 1);

            lpsolve.print_str(lp, "Scaling is turned off with lpsolve.unscale(lp);" + NewLine);
            lpsolve.unscale(lp);
            lpsolve.print_lp(lp);

            lpsolve.print_str(lp, "Now turn B&B debugging off and simplex tracing on with" + NewLine);
            lpsolve.print_str(lp, "lpsolve.set_debug(lp, false); lpsolve.set_trace(lp, true); and lpsolve.solve(lp);" + NewLine);
            lpsolve.set_debug(lp, false);
            lpsolve.set_trace(lp, true);

            lpsolve.solve(lp);
            lpsolve.print_str(lp, "Where possible, lp_solve will start at the last found basis" + NewLine);
            lpsolve.print_str(lp, "We can reset the problem to the initial basis with" + NewLine);
            lpsolve.print_str(lp, "default_basis lp. Now solve it again..." + NewLine);

            lpsolve.default_basis(lp);
            lpsolve.solve(lp);

            lpsolve.print_str(lp, "It is possible to give variables and constraints names" + NewLine);
            lpsolve.print_str(lp, "lpsolve.set_row_name(lp, 1, \"speed\"); lpsolve.set_col_name(lp, 2, \"money\");" + NewLine);
            lpsolve.set_row_name(lp, 1, "speed");
            lpsolve.set_col_name(lp, 2, "money");
            lpsolve.print_lp(lp);
            lpsolve.print_str(lp, "As you can see, all column and rows are assigned default names" + NewLine);
            lpsolve.print_str(lp, "If a column or constraint is deleted, the names shift place also:" + NewLine);

            lpsolve.print_str(lp, "lpsolve.del_column(lp, 1);" + NewLine);
            lpsolve.del_column(lp, 1);
            lpsolve.print_lp(lp);

            lpsolve.write_lp(lp, "lp.lp");
            lpsolve.write_mps(lp, "lp.mps");

            lpsolve.set_outputfile(lp, null);

            lpsolve.delete_lp(lp);

            lp = lpsolve.read_LP("lp.lp", 0, "test");
            if (lp == (IntPtr)0)
            {
                MessageBox.Show("Can't find lp.lp, stopping");
                return;
            }

            lpsolve.set_outputfile(lp, "result2.txt");

            lpsolve.print_str(lp, "An lp structure can be created and read from a .lp file" + NewLine);
            lpsolve.print_str(lp, "lp = lpsolve.read_LP(\"lp.lp\", 0, \"test\");" + NewLine);
            lpsolve.print_str(lp, "The verbose option is disabled" + NewLine);

            lpsolve.print_str(lp, "lp is now:" + NewLine);
            lpsolve.print_lp(lp);

            lpsolve.print_str(lp, "solution:" + NewLine);
            lpsolve.set_debug(lp, true);
            lpsolve.lpsolve_return statuscode = lpsolve.solve(lp);
            string status = lpsolve.get_statustext(lp, (int)statuscode);
            Debug.WriteLine(status);

            lpsolve.set_debug(lp, false);
            lpsolve.print_objective(lp);
            lpsolve.print_solution(lp, 1);
            lpsolve.print_constraints(lp, 1);

            lpsolve.write_lp(lp, "lp.lp");
            lpsolve.write_mps(lp, "lp.mps");

            lpsolve.set_outputfile(lp, null);

            lpsolve.delete_lp(lp);
        }  

        private void FilterEnvelopes(List<Envelope> envList)
        {
            // get all the peaks involved in these envlopes and sort according to the m/z values ascendingly;
            List<Ion> peakList = new List<Ion>();
            foreach (Envelope env in envList)
            {
                foreach (Ion peak in env.PeaksInEnvelope)
                {
                    int idx = peakList.FindIndex(
                        delegate(Ion p)
                        {
                            return (p.MZ == peak.MZ && p.Intensity == peak.Intensity);
                        }
                        );
                    if (idx < 0)
                    {
                        peakList.Add(peak);
                    }
                }
            }
            peakList.Sort(delegate(Ion p1, Ion p2)
            {
                if (p1.MZ < p2.MZ)
                    return -1;
                else if (p1.MZ > p2.MZ)
                    return 1;
                else
                    return 0;
            });

            double[] peakMzArrForSearch = new double[peakList.Count];
            for (int i = 0; i < peakList.Count; i++)
            {
                peakMzArrForSearch[i] = peakList[i].MZ;
            }
            // construct a matrix of theoretical relative intensities;
            int rowNum = peakList.Count;
            int colNum = envList.Count;
            double[,] mat = new double[rowNum, colNum];
            Array.Clear(mat, 0, mat.Length); // set all elements to zero;
            for (int envIdx = 0; envIdx < envList.Count; envIdx++)
            {
                Envelope env = envList[envIdx];
                for (int peakNum = 0; peakNum < env.PeaksInEnvelope.Count; peakNum++)
                {
                    Ion peak = env.PeaksInEnvelope[peakNum];
                    int peakIdx = Array.BinarySearch(peakMzArrForSearch, peak.MZ);
                    if (peakIdx < 0)
                    {
                        Console.WriteLine("Something is wrong here, cannot find " + peak.MZ + " in the peak list!");
                        return;
                    }

                    mat[peakIdx, envIdx] = env.TheoIsotDist[peakNum];
                }
            }

            Matrix<double> coefMat = SparseMatrix.OfArray(mat);
            // pseudo inverse the coefficient matrix;
            Matrix<double> piCoefMat = PseudoInverse(coefMat);

            // construct the peak intensity array;
            double[,] peakIntArr = new double[peakList.Count, 1];
            for (int i = 0; i < peakIntArr.Length; i++)
            {
                peakIntArr[i, 0] = peakList[i].Intensity;
            }
            Matrix<double> b = SparseMatrix.OfArray(peakIntArr);

            Matrix<double> x = piCoefMat * b;

            // check whether exist non-positive x values;
            bool recalcRequired = false;
            double[,] xArr = x.ToArray();
            for (int xIdx = 0; xIdx < xArr.Length; xIdx++)
            {
                if (xArr[xIdx, 0] <= 0)
                {
                    envList[xIdx].Score = double.MaxValue;
                    recalcRequired = true;
                }
                else
                {
                    List<Ion> peaksInEnv = envList[xIdx].PeaksInEnvelope;
                    int len = peaksInEnv.Count();
                    for (int j = xIdx + 1; j < xArr.Length; j++)
                    {
                        double monoPrecMz = envList[j].PeaksInEnvelope[0].MZ;
                        for (int pIdx = 1; pIdx < len; pIdx++)
                        {
                            if (Math.Abs(monoPrecMz - envList[xIdx].PeaksInEnvelope[pIdx].MZ) < 0.01)
                            {
                                if (xArr[j, 0] / xArr[xIdx, 0] <= 0.2)
                                {
                                    envList[j].Score = double.MaxValue;
                                }
                            }
                        }
                        
                    }
                    envList[xIdx].FractionQuantity = xArr[xIdx, 0];
                }
            }

            // remove all the envelopes with bad scores and repeat the filtration;
            if (recalcRequired)
            { 
                envList.RemoveAll(elem => elem.Score >= double.MaxValue);
                FilterEnvelopes(envList);
            }
        }

        private void FilterEnvelopesByLP(List<Envelope> envList)
        {
            // get all the peaks involved in these envlopes and sort according to the m/z values ascendingly;
            // i.e. construct the vector of peaks in the isolation window;
            List<Ion> peakList = new List<Ion>();
            foreach (Envelope env in envList)
            {
                foreach (Ion peak in env.PeaksInEnvelope)
                {
                    int idx = peakList.FindIndex(
                        delegate(Ion p)
                        {
                            return (p.MZ == peak.MZ && p.Intensity == peak.Intensity);
                        }
                        );
                    if (idx < 0)
                    {
                        peakList.Add(peak);
                    }
                }
            }
            peakList.Sort(delegate(Ion p1, Ion p2)
            {
                if (p1.MZ < p2.MZ)
                    return -1;
                else if (p1.MZ > p2.MZ)
                    return 1;
                else
                    return 0;
            });
            double[] peakMzArrForSearch = new double[peakList.Count];
            for (int i = 0; i < peakList.Count; i++)
            {
                peakMzArrForSearch[i] = peakList[i].MZ;
            }

            // construct a matrix of theoretical relative intensities;
            int rowNum = peakList.Count;
            int colNum = envList.Count;
            double[,] mat = new double[rowNum, colNum];
            Array.Clear(mat, 0, mat.Length); // set all elements to zero;
            for (int envIdx = 0; envIdx < envList.Count; envIdx++)
            {
                Envelope env = envList[envIdx];
                for (int peakNum = 0; peakNum < env.PeaksInEnvelope.Count; peakNum++)
                {
                    Ion peak = env.PeaksInEnvelope[peakNum];
                    int peakIdx = Array.BinarySearch(peakMzArrForSearch, peak.MZ);
                    if (peakIdx < 0)
                    {
                        Console.WriteLine("Something is wrong here, cannot find " + peak.MZ + " in the peak list!");
                        return;
                    }

                    mat[peakIdx, envIdx] = env.TheoIsotDist[peakNum];
                }
            }

            int totalLen = colNum + rowNum;
            IntPtr lp = lpsolve.make_lp(0, totalLen);
            lpsolve.set_verbose(lp, 0);
            //lpsolve.set_outputfile(lp, "result.txt");
            totalLen += 1;
            for (int i = 0; i < rowNum; i++)
            {
                double[] row1 = new double[totalLen];
                double[] row2 = new double[totalLen];
                for (int j = 1; j <= colNum; j++)
                {
                    row1[j] = mat[i, j - 1];
                    row2[j] = mat[i, j - 1];
                }
                for (int j = colNum + 1; j < totalLen; j++)
                {
                    if (j - colNum - i == 1)
                    {
                        row1[j] = 1;
                        row2[j] = -1;
                    }
                    else
                    {
                        row1[j] = 0;
                        row2[j] = 0;
                    }
                }
                lpsolve.add_constraint(lp, row1, lpsolve.lpsolve_constr_types.GE, peakList[i].Intensity);
                lpsolve.add_constraint(lp, row2, lpsolve.lpsolve_constr_types.LE, peakList[i].Intensity); 
            }

            for (int i = 1; i < totalLen; i++)
            {
                double[] row = new double[totalLen];
                Array.Clear(row, 0, totalLen);
                row[i] = 1;
                lpsolve.add_constraint(lp, row, lpsolve.lpsolve_constr_types.GE, 0);
                //if (i > colNum)
                //{
                //    lpsolve.add_constraint(lp, row, lpsolve.lpsolve_constr_types.LE, peakList[i - colNum - 1].Intensity * 0.2);
                //}
            }

            double[] objRow = new double[totalLen];
            Array.Clear(objRow, 0, totalLen);
            for (int i = colNum + 1; i < totalLen; i++)
            {
                objRow[i] = 1;
            }
            lpsolve.set_obj_fn(lp, objRow);

            lpsolve.solve(lp); 
            double[] vars = new double[lpsolve.get_Ncolumns(lp)];
            //lpsolve.print_lp(lp);
            lpsolve.get_variables(lp, vars);
            lpsolve.delete_lp(lp);
            
            // select the top five envelopes;
            List<Tuple<int, double>> wList = new List<Tuple<int, double>>();
            for (int i = 0; i < colNum; i++)
            {
                wList.Add(new Tuple<int, double>(i, vars[i]));
            }
            wList.Sort(delegate(Tuple<int, double> t1, Tuple<int, double> t2)
            {
                if (t1.Item2 < t2.Item2)
                    return 1;
                else if (t1.Item2 > t2.Item2)
                    return -1;
                else
                    return 0;
            });
            int topCount = 5;
            for (int i = 0; i < colNum; i++)
            {
                if (topCount <= 0 || wList[i].Item2 <= 0)
                {
                    envList[wList[i].Item1].Score = double.MaxValue;
                }
                else
                {
                    topCount--;
                }
                envList[wList[i].Item1].FractionQuantity = wList[i].Item2;
            }
            envList.RemoveAll(elem => elem.Score >= double.MaxValue);

            // check whether exist non-positive x values;
            //bool recalcRequired = false;
           // double[,] xArr = x.ToArray();
            //for (int xIdx = 0; xIdx < xArr.Length; xIdx++)
            //{
            //    if (xArr[xIdx, 0] <= 0)
            //    {
            //        envList[xIdx].Score = double.MaxValue;
            //        recalcRequired = true;
            //    }
            //    else
            //    {
            //        List<Ion> peaksInEnv = envList[xIdx].PeaksInEnvelope;
            //        int len = peaksInEnv.Count();
            //        for (int j = xIdx + 1; j < xArr.Length; j++)
            //        {
            //            double monoPrecMz = envList[j].PeaksInEnvelope[0].MZ;
            //            for (int pIdx = 1; pIdx < len; pIdx++)
            //            {
            //                if (Math.Abs(monoPrecMz - envList[xIdx].PeaksInEnvelope[pIdx].MZ) < 0.01)
            //                {
            //                    if (xArr[j, 0] / xArr[xIdx, 0] <= 0.2)
            //                    {
            //                        envList[j].Score = double.MaxValue;
            //                    }
            //                }
            //            }

            //        }
            //        envList[xIdx].FractionQuantity = xArr[xIdx, 0];
            //    }
            //}

            // remove all the envelopes with bad scores and repeat the filtration;
            //if (recalcRequired)
            //{
            //    envList.RemoveAll(elem => elem.Score >= double.MaxValue);
            //    FilterEnvelopes(envList);
            //}
        }

        private void RemoveFalseSubsets(List<Envelope> envList) 
        {
            // find all the subsets and determine whether they should be removed;
            for (int i = 0; i < envList.Count; i++)
            {
                for (int j = i + 1; j < envList.Count; j++)
                {
                    int subsetType = IsSubset(envList[i], envList[j]);
                    if (subsetType == 1)
                    {
                        // envList[i] is the subset of envList[j];
                        if (envList[i].FractionQuantity / envList[j].FractionQuantity <= 0.5)
                        {
                            envList[i].Score = double.MaxValue;
                        }
                    } 
                    else if (subsetType == -1) 
                    {
                        // envList[j] is the subset of envList[i];
                        if (envList[j].FractionQuantity / envList[i].FractionQuantity <= 0.5)
                        {
                            envList[j].Score = double.MaxValue;
                        }
                    }

                    // else, they have no relationship, do nothing;
                }
            }

            // remove all the envelopes with bad scores;
            envList.RemoveAll(elem => elem.Score >= double.MaxValue);
            FilterEnvelopes(envList);
        }

        private int IsSubset(Envelope e1, Envelope e2) 
        {
            // check if e1 is the subset of e2;
            bool flag = true;
            foreach (Ion peak in e1.PeaksInEnvelope)
            {
                int idx = e2.PeaksInEnvelope.FindIndex(
                        delegate(Ion p)
                        {
                            return (p.MZ == peak.MZ && p.Intensity == peak.Intensity);
                        }
                        );
                if (idx < 0)
                {
                    flag = false;
                    break;
                }
            }
            if (flag)
            {
                return 1;
            }

            // check if el2 is the subset of el1;
            flag = true;
            foreach (Ion peak in e2.PeaksInEnvelope)
            {
                int idx = e1.PeaksInEnvelope.FindIndex(
                        delegate(Ion p)
                        {
                            return (p.MZ == peak.MZ && p.Intensity == peak.Intensity);
                        }
                        );
                if (idx < 0)
                {
                    flag = false;
                    break;
                }
            }
            if (flag)
            {
                return -1;
            }

            // not a subset for the other;
            return 0;
        }

        private List<T> Clone<T>(IEnumerable<T> oldList)
        {
            return new List<T>(oldList);
        }

        private Matrix<double> PseudoInverse(Matrix<double> M)
        {
            var svd = M.Svd(true);
            var W = svd.W;
            var s = svd.S;

            // The first element of W has the maximum value. 
            double tolerance = Precision.EpsilonOf(2) * Math.Max(M.RowCount, M.ColumnCount) * W[0, 0];

            for (int i = 0; i < s.Count; i++)
            {
                if (s[i] < tolerance)
                    s[i] = 0;
                else
                    s[i] = 1 / s[i];
            }
            W.SetDiagonal(s);

            // (U * W * VT)T is equivalent with V * WT * UT 
            return (svd.U * W * svd.VT).Transpose();
        }

        private void FilterEnvelopes_Old(ref List<Envelope> envList)
        {
            // remove all the envelopes with bad scores, 0.5 is used as the threshold;
            int envListLen = envList.Count;
            while (envListLen >= 0 && envList[envListLen - 1].Score >= 0.5)
            {
                envList.RemoveAt(envListLen - 1);
                envListLen--;
            }

            bool isChanged = false;
            // filter the envelopes sharing the same monoisotope but different charges;
            // and also filter the envelopes sharing the same charge but shifting the monoisotopes;
            for (int i = 0; i < envList.Count; i++)
            {
                for (int j = i + 1; j < envList.Count; j++)
                {
                    if (envList[i].Score > 0.5 || envList[j].Score > 0.5)
                    {
                        continue;
                    }
                    if (Math.Abs(envList[i].MonoisotPeak.MZ - envList[j].MonoisotPeak.MZ) < 0.001)
                    {
                        isChanged = AdjustEnvelopeScoresWithDifferentCharges(ref envList, i, j);
                    }
                    else
                    {
                        double massDiff = Math.Abs(envList[j].MonoisotPeak.MZ - envList[i].MonoisotPeak.MZ);
                        //if (envList[j].Charge == envList[i].Charge)
                        {
                            int stepNum = (int) Math.Round(massDiff * envList[i].Charge);
                            if (Math.Abs(massDiff - (double) stepNum / envList[i].Charge) <= 0.01)
                            {
                                isChanged |= AdjustEnvelopeScoresWithDifferentMonoIstoMz(ref envList, i, j);
                            }
                        }
                    }
                }
            }

            // sort the envList according to the scores decendingly;
            envList.Sort(delegate(Envelope e1, Envelope e2)
            {
                if (e1.Score < e2.Score)
                    return -1;
                else if (e1.Score > e2.Score)
                    return 1;
                else
                    return 0;
            });

            if (isChanged)
            {
                //FilterEnvelopes(ref envList);
            }
        }

        private bool AdjustEnvelopeScoresWithDifferentCharges(ref List<Envelope> envList, int i, int j)
        {
            bool changed = true;

            // define coefficients, a11 and a21 are for monoisotopic peaks, a22 is for the first isotopic peak with the larger charge;
            // b1 is the intensity of the monoisotopic peak, and b2 is the intensity of the first isotopic peak with the larger charge;
            double a11 = 0, a21 = 0, a22 = 0, b1 = 0, b2 = 0;

            // make sure that a11 is for the smaller charge, a21 and a22 are for the larger charge;
            b1 = envList[i].PeaksInEnvelope[0].Intensity;
            if (envList[i].Charge > envList[j].Charge)
            {
                a11 = envList[j].TheoIsotDist[0];
                a21 = envList[i].TheoIsotDist[0];
                a22 = envList[i].TheoIsotDist[1];
                b2 = envList[i].PeaksInEnvelope[1].Intensity;
            }
            else
            {
                a11 = envList[i].TheoIsotDist[0];
                a21 = envList[j].TheoIsotDist[0];
                a22 = envList[j].TheoIsotDist[1];
                b2 = envList[j].PeaksInEnvelope[1].Intensity;
            }

            double x2 = b2 / a22;   // portion of larger charge;
            double x1 = (b1 - a21 * b2 / a22) / a11;    // portion of smaller charge;
            double ratio = Math.Abs(x1 / x2);

            // ith envlope has the smaller charge;
            if (envList[j].Charge > envList[i].Charge)
            {
                // case 1: x1 is much smaller than x2, means the smaller charge trends to be impossible;
                if (ratio < 0.75)
                {
                    envList[i].Score = 1;
                }
                // case 2: x1 is much larger than x2, means the smaller charge trends to be dominant;
                else if (ratio > 1.33)
                {
                    envList[j].Score = 1;
                }
                // case 3: x1 and x2 are similar, means there is a big chance that they form a mixture, so keep both.
                else
                {
                    changed = false;
                }
            }
            
            // jth envlope has the smaller charge;
            else 
            {
                // case 1: x1 is much smaller than x2, means the smaller charge trends to be impossible;
                if (ratio < 0.75)
                {
                    envList[j].Score = 1;
                }
                // case 2: x1 is much larger than x2, means the smaller charge trends to be dominant;
                else if (ratio > 1.33)
                {
                    envList[i].Score = 1;
                }
                // case 3: x1 and x2 are similar, means there is a big chance that they form a mixture, so keep both.
                else 
                {
                    changed = false;
                }
            }

            return changed;
        }

        private bool AdjustEnvelopeScoresWithDifferentMonoIstoMz(ref List<Envelope> envList, int i, int j)
        {
            bool changed = true;

            // define coefficients, a11 and a21 are for the envelope with smaller monoisotopic peaks, a12 is for the the envelope with the larger one;
            // b1 is the intensity of the monoisotopic peak with the smaller m/z, and b2 is the intensity of the monoisotopic peak with the larger m/z;
            double a11 = 0, a12 = 0, a21 = 0, b1 = 0, b2 = 0;

            int idx = (int) Math.Round(Math.Abs(envList[i].MonoisotPeak.MZ - envList[j].MonoisotPeak.MZ) * envList[i].Charge);
            if (idx >= envList[i].TheoIsotDist.Length || idx >= envList[j].TheoIsotDist.Length)
            {
                return false;
            }

            // make sure that a11 is for the smaller charge, a21 and a22 are for the larger charge;
            if (envList[i].MonoisotPeak.MZ > envList[j].MonoisotPeak.MZ)
            {
                a11 = envList[j].TheoIsotDist[0];
                a12 = envList[j].TheoIsotDist[idx];
                a21 = envList[i].TheoIsotDist[0];
                b1 = envList[j].PeaksInEnvelope[0].Intensity;
                b2 = envList[i].PeaksInEnvelope[0].Intensity;
            }
            else
            {
                a11 = envList[i].TheoIsotDist[0];
                a12 = envList[i].TheoIsotDist[idx];
                a21 = envList[j].TheoIsotDist[0];
                b1 = envList[i].PeaksInEnvelope[0].Intensity;
                b2 = envList[j].PeaksInEnvelope[0].Intensity;
            }

            double x1 = b1 / a11;   // portion of the smaller m/z;
            double x2 = (b2 - a12 * b1 / a11) / a21;    // portion of larger m/z;
            double ratio = Math.Abs(x2 / x1);

            // ith envlope has the smaller monoisotopic m/z;
            if (envList[j].MonoisotPeak.MZ > envList[i].MonoisotPeak.MZ)
            {
                // case 1: x2 is much smaller than x1, means the larger monoisotopic m/z trends to be impossible;
                if (ratio < 0.8)
                {
                    envList[j].Score = 1;
                }
                // case 2: x2 is much larger than x1, means the larger monoisotopic m/z trends to be dominant;
                else if (ratio > 1.2)
                {
                    envList[i].Score = 1;
                }
                // case 3: x1 and x2 are similar, means there is a big chance that they form a mixture, so keep both.
                else
                {
                    changed = false;
                }
            }

            // jth envlope has the smaller monoisotopic m/z;
            else
            {
                // case 1: x2 is much smaller than x1, means the larger monoisotopic m/z trends to be impossible;
                if (ratio < 0.8)
                {
                    envList[i].Score = 1;
                }
                // case 2: x2 is much larger than x1, means the larger monoisotopic m/z trends to be dominant;
                else if (ratio > 1.2)
                {
                    envList[j].Score = 1;
                }
                // case 3: x1 and x2 are similar, means there is a big chance that they form a mixture, so keep both.
                else
                {
                    changed = false;
                }
            }

            return changed;
        }
    }

}
