using lpsolve55;
using RawConverter.Common;
using RawConverter.Converter;
using RawConverter.MassSpec;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RawConverter.DDADataProcess
{
    class PrecursorCorrector
    {
        private double MAX_CHECK_FORWARD_MZ = 1.1;
        private double MAX_CHECK_BACKWARD_MZ = 1.1;
        private const int MAX_MASS_IN_AVERAGINE_TABLE = 7850;
        private const int MAX_ISOTOPE_NUM = 11;
        private double[,] AveragineTable = null;
        private int[] IsotopeNumArr = null;


        public PrecursorCorrector()
        {
            InitializeAveragineTable();
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

        //via vote to predict monoistopic by three MS1 scans
        public void CorrectPrecursor(ref MassSpectrum curSpec, double precMz, int precZ, int CurrentMS1ScanNum, List<MassSpectrum> MS1spec)
        {
            Dictionary<double, double> CorrectMZ = new Dictionary<double, double>();
            double precMzFromFilter = curSpec.PrecMzFromFilter;
            int flag = 0;
            double PredictedMZ = 0.0;
            double precErr = curSpec.PrecMzFromFilter * RawFileConverter.FTMS_ERR_TOL / 1E6; 

            bool debug = false;
            int debugNum = 2087;

            // get the peaks dropped in a designated window by current MS1;
            List<Ion> peaksInWindows = new List<Ion>();

            if (precZ > 6)
            {
                return;
            }

            //*yychu Debug
            if (debug)
            {
                if (curSpec.ScanNumber == debugNum)
                {
                    Console.WriteLine("MZ:" + precMz);
                    foreach (var yy in MS1spec)
                    {
                        Console.WriteLine("MS1specNo: " + yy.ScanNumber + "\t");
                    }
                }
            }
            //*/

            if (curSpec.ScanNumber == 26643)
            {
                Console.Write("");
            }
            //caculate each MS2 with another MS1 
            for (int i = 0; i < MS1spec.Count; i++)
            {
                if (MS1spec[i] == null)
                {
                    continue;
                }

                double tmpPrecMZ = 0.0;
                double tmpScore = 0.0;
                // get the precursor MS1 spectrum;
                List<Ion> peaks = MS1spec[i].Peaks;

                // get the peaks dropped in a designated window;
                List<Ion> peaksInWindow = new List<Ion>();
                double err = RawFileConverter.FTMS_ERR_TOL * precMz / 1E6;
                double startMz = precMzFromFilter - MAX_CHECK_BACKWARD_MZ;
                startMz = Math.Min(startMz, precMz) - err;
                double endMz = precMzFromFilter + MAX_CHECK_FORWARD_MZ;
                foreach (Ion peak in peaks)
                {
                    if (peak.MZ >= startMz && peak.MZ <= endMz)
                    {
                        peaksInWindow.Add(peak);
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

            
                
                // get all the possible envelopes;
                List<Envelope> envList = FindEnvelopesByLP(peaksInWindow, curSpec.PrecMzFromFilter, precZ);
                if (envList == null)
                {
                    continue;
                }

                // rescore and re-sort the envList according to the scores decendingly;
                RescoreEnvelopes(envList);
                envList.Sort(delegate(Envelope e1, Envelope e2)
                {
                    if (e1.Score < e2.Score)
                        return 1;
                    else if (e1.Score > e2.Score)
                        return -1;
                    else
                        return 0;
                });

                Envelope env = (envList != null && envList.Count > 0) ? envList[0] : null;
                if (env != null && env.Score < double.MaxValue)
                {
                    curSpec.PrecursorRefined = true;
                    tmpPrecMZ = env.MonoisotPeak.MZ;
                    tmpScore = env.Score;
                    if (MS1spec[i].ScanNumber == CurrentMS1ScanNum)
                    {
                        peaksInWindows = peaksInWindow;
                    }
                }
                else
                {
                    continue;
                }


                //* yychu Debug
                if (debug)
                {
                    if (curSpec.ScanNumber == debugNum)
                    {
                        Console.WriteLine("no:" + curSpec.ScanNumber);
                        Console.WriteLine("oldMZ: " + tmpPrecMZ);
                    }
                }
                //*/

                // Vote monoisotopic
                foreach (var mz in CorrectMZ)
                {
                    if (Math.Abs(mz.Key - tmpPrecMZ) <= precErr)
                    {
                        CorrectMZ[mz.Key] += tmpScore;
                        flag = 1;
                        break;
                    }
                    flag = 0;
                }
                if (flag == 0)
                {
                    CorrectMZ.Add(tmpPrecMZ, tmpScore);
                }
            }
           //*yychu Debug
            if (debug)
            {
                if (curSpec.ScanNumber == debugNum)
                {
                    foreach (var mz in CorrectMZ)
                    {
                        Console.WriteLine("CorrectMZ: " + mz);
                    }
                }
            }
            //*/

            PredictedMZ = CorrectMZ.FirstOrDefault(x => x.Value == CorrectMZ.Values.Max()).Key;
            if (PredictedMZ == 0)
            {
                PredictedMZ = curSpec.Precursors[0].Item1;
            }

            curSpec.Precursors.Clear();
            curSpec.Precursors.Add(new Tuple<double, int>(PredictedMZ, precZ));


            //To find prediected monoisotopic intensity in current MS1
            foreach (Ion peak in peaksInWindows)
            {
                if(Math.Round(peak.MZ,2)==Math.Round(PredictedMZ,2))
                {
                    curSpec.PrecursorIntensity = peak.Intensity;
                    break;
                }
                else
                {
                    curSpec.PrecursorIntensity = 0;
                }
            }
        }


        public List<Envelope> FindEnvelopes(List<Ion> peaks, double precMz, int precZ)
        {
            // define the precursor list returned after the search;
            List<Envelope> envList = new List<Envelope>();
            
            // skip if the current spectrum is MS1;
            if (peaks == null || peaks.Count == 0)
            {
                return envList;
            }

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
            double err = RawFileConverter.FTMS_ERR_TOL * precMz / 1E6;
            for (int i = 0; i < peaks.Count; i++)
            {
                if (peaks[i].MZ - precMz > err)
                {
                    break;
                }
                Ion startPeak = peaks[i];
                List<Envelope> localEnvList = new List<Envelope>();
                List<Ion> env = new List<Ion>();
                env.Add(peaks[i]);

                // get the theoretical isotopic distribution;
                double precMH = (startPeak.MZ - Utils.PROTON_MASS) * precZ + Utils.PROTON_MASS;
                // simply skip the correction for spectrum with a large mass;
                if (precMH > MAX_MASS_IN_AVERAGINE_TABLE)
                {
                    break;
                }
                int avgTblIdx = (int) (precMH + 25) / 50;
                int isotNum = IsotopeNumArr[avgTblIdx];
                double[] theoIsotDist = new double[isotNum];
                for (int idx = 0; idx < isotNum; idx++)
                {
                    theoIsotDist[idx] = AveragineTable[avgTblIdx, idx];
                }

                // propagate the envelope;
                Ion prevPeak = startPeak;
                for (int j = i + 1; j < peaks.Count; j++)
                {
                    // only check limited forward m/z values;
                    if (peaks[j].MZ - startPeak.MZ > MAX_CHECK_FORWARD_MZ)
                    {
                        break;
                    }

                    // add into the envelop if the current peak has a specific m/z gap with the previous one in the env;
                    if (Math.Abs(peaks[j].MZ - prevPeak.MZ - Utils.MASS_DIFF_C12_C13 / (double) precZ) <= err)
                    {
                        env.Add(peaks[j]);
                        prevPeak = peaks[j];
                    }
                }
                if (env.Count > 1)
                {
                    // check whether to keep this envelope;
                    int neutronNum = (int) Math.Round(Math.Abs(startPeak.MZ - precMz) * precZ / Utils.MASS_DIFF_C12_C13);
                    double tmpErr = Math.Abs(precMz - startPeak.MZ) - neutronNum * Utils.MASS_DIFF_C12_C13 / precZ;
                    if (Math.Abs(tmpErr) < err)
                    {
                        envList.Add(new Envelope(startPeak, precZ, env, theoIsotDist, highestPeak));
                    }
                }
            }

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
            

            return envList;
        }

        public List<Envelope> FindEnvelopesByLP(List<Ion> peaks, double precMz, int precZ)
        {
            if (peaks == null || peaks.Count == 0)
            {
                return null;
            }

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
            List<Envelope> envList = new List<Envelope>();
            for (int i = 0; i < peaks.Count; i++)
            {
                Ion startPeak = peaks[i];

                double err = 5 * startPeak.MZ / 1e6;
                double stepMz = Utils.MASS_DIFF_C12_C13 / (double) precZ;
                double previousMz = startPeak.MZ;
                List<Ion> env = new List<Ion>();
                env.Add(peaks[i]);

                // get the theoretical isotopic distribution;
                double precMH = (previousMz - Utils.PROTON_MASS) * precZ + Utils.PROTON_MASS;
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
                    envList.Add(new Envelope(startPeak.Clone(), precZ, env, theoIsotDist, highestPeak));
                }
            }

            if (envList.Count == 0)
            {
                return envList;
            }

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
                        return envList;
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

            // select the envelope with the largest weight;
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
            int topCount = 1;
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

        private void FindBestEnvelope(List<Ion> peaks, double precMz, int precZ)
        {
            double precMH = (precMz - Utils.PROTON_MASS) * precZ + Utils.PROTON_MASS;
            // simply skip the correction for spectrum with a large mass;
            if (precMH > MAX_MASS_IN_AVERAGINE_TABLE)
            {
                return;
            }
            int avgTblIdx = (int) (precMH + 25) / 50;
            int isotNum = IsotopeNumArr[avgTblIdx];
            double[] theoIsotDist = new double[isotNum];
            for (int i = 0; i < isotNum; i++)
            {
                theoIsotDist[i] = AveragineTable[avgTblIdx, i];
            }

            // find all the possible envelopes;
            double ppm = 10;
            double err = precMz * ppm / 1e6;
            double mzStep = 1.0 / precZ;
            List<double[]> obsvIsotDistList = new List<double[]>();
            List<double> startMzList = new List<double>();
            for (int i = 0; i < peaks.Count; i++)
            {
                Ion peak = peaks[i];
                double stepNum = Math.Round((precMz - peak.MZ) / mzStep);
                if (Math.Abs(precMz - stepNum * mzStep - peak.MZ) > 2 * err)
                {
                    continue;
                }

                // precursor m/z value correction only looks for previous peaks;
                if (peak.MZ > precMz + err)
                {
                    continue;
                }

                double[] obsvIsotDist = new double[isotNum];
                obsvIsotDist[0] = peak.Intensity;
                int distIdx = 1;
                double maxIntensity = peak.Intensity;
                Ion lastPeak = peak;
                for (int j = i + 1; j < peaks.Count; j++)
                {
                    if (distIdx >= isotNum)
                    {
                        break;
                    }
                    Ion p = peaks[j];
                    if (Math.Abs(p.MZ - lastPeak.MZ - mzStep) <= err)
                    {
                        obsvIsotDist[distIdx] = p.Intensity;
                        if (maxIntensity < p.Intensity)
                        {
                            maxIntensity = p.Intensity;
                        }
                        distIdx++;
                        lastPeak = p;
                    }
                }
                if (maxIntensity > 0)
                {
                    for (int j = 0; j < isotNum; j++)
                    {
                        obsvIsotDist[j] = obsvIsotDist[j] * 100 / maxIntensity;
                    }
                    obsvIsotDistList.Add(obsvIsotDist);
                    startMzList.Add(peak.MZ);
                }
            }

            // compare the observed envelopes to the theoretical distribution;
            double[] bestDist = null;
            double minScore = double.MaxValue;
            int bestMzIdx = -1;
            for (int distIdx = 0; distIdx < obsvIsotDistList.Count; distIdx++)
            {
                double[] obsvIsotDist = obsvIsotDistList[distIdx];
                double score = 0;
                for (int i = 0; i < isotNum; i++)
                {
                    score += Math.Pow(theoIsotDist[i] - obsvIsotDist[i], 2);
                }
                if (minScore > score)
                {
                    minScore = score;
                    bestDist = obsvIsotDist;
                    bestMzIdx = distIdx;
                }
            }
            //if (minScore < double.MaxValue)
            //{
            //    precMz = startMzList[bestMzIdx];
            //    curSpec.Precursors.Clear();
            //    curSpec.Precursors.Add(new Tuple<double, int>(precMz, precZ));
            //}
            //else // if still cannot predict it, use the precursor m/z in the scanFilter;
            //{
            //    if (precMzInRaw < 0.01)
            //    {
            //        curSpec.Precursors.Clear();
            //        curSpec.Precursors.Add(new Tuple<double, int>(precMz, precZ));
            //    }
            //}
        }
    }
    //class PrecursorInfo
    //{
    //    public double PrecursorMZ{get; set;}
    //    public double PrecursorInstensity { get; set; }
    //    public PrecursorInfo
    //        (
    //       double precursorMZ,
    //       double precursorInstensity
    //        )
    //    {
    //        PrecursorMZ = precursorMZ;
    //        precursorInstensity = precursorMZ;
    //    }
    //}
}
                                                                                                                                                                                                                                                                                                       