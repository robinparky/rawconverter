using RawConverter.Common;
using RawConverter.MassSpec;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RawConverter.Converter
{
    class MgfConverter
    {
        /// <summary>
        /// Local variables
        /// </summary>
        private Regex _startsWithNumber = new Regex("^[0-9]", RegexOptions.Compiled);
        private StreamReader _mgfReader = null;
        private StreamWriter _ms2Writer = null;
        private StreamWriter _ms3Writer = null;
        
        private const bool GET_PRECURSOR_MZ_AND_INTENSITY_FROM_MS1 = false;
        private const bool ALWAYS_USE_PRECURSOR_CHARGE_STATE_RANGE = false;
        private const int MS2_MINIMUM_PRECURSOR_CHARGE = 2;
        private const int MS2_MAXIMUM_PRECURSOR_CHARGE = 3;
        private const int MS3_MINIMUM_PRECURSOR_CHARGE = 1;

        private int _mzDecimalPlace = 0;
        private int _intensityDecimalPlace = 0;
        private HashSet<int> _ddaDataChargeStates = null;

        private double _lastProgress = 0;
        private int _totalSpecNum = 0;
        private int _processedSpecNum = 0;

        private int _lastScanNum = 0;

        private string _outFileName = null;

        public MgfConverter(string mgfFile, string outFolder, string outFileType)
        {
            _mgfReader = new StreamReader(mgfFile);
            
            // get the number of spectra;
            string line = null;
            while ((line = _mgfReader.ReadLine()) != null)
            {
                if (line.StartsWith("BEGIN IONS"))
                {
                    _totalSpecNum++;
                }
            }
            _mgfReader.Close();
            _mgfReader = new StreamReader(mgfFile);

            InitWriters(Path.GetFileName(mgfFile), outFolder, outFileType);
        }

        private void InitWriters(string inFileName, string outFolder, string outFileType)
        {
            string outFileWithoutExtentionName = outFolder + Path.DirectorySeparatorChar +
                Regex.Replace(inFileName, ".mgf", ".", RegexOptions.IgnoreCase);
            Console.WriteLine(" Output file: " + outFileWithoutExtentionName + outFileType);
            if (outFileType.Equals("ms2", StringComparison.InvariantCultureIgnoreCase))
            {
                _outFileName = outFileWithoutExtentionName + outFileType;
                _ms2Writer = new StreamWriter(_outFileName);
            }
            if (outFileType.Equals("ms3", StringComparison.InvariantCultureIgnoreCase))
            {
                _outFileName = outFileWithoutExtentionName + outFileType;
                _ms3Writer = new StreamWriter(_outFileName);
            }
        }

        public void Convert(TaskProgress progress)
        {
            string line = null;
            int scan = 0;
            bool started = false;
            
            List<String> mgfData = new List<String>();
            while ((line = _mgfReader.ReadLine()) != null)
            {
                if (!started)
                {
                    do
                    {
                        if (line.Contains("BEGIN IONS"))
                        {
                            started = true;
                            break;
                        }
                    } while ((line = _mgfReader.ReadLine()) != null);
                }

                if (line.Length > 0)
                {
                    //process each precursor
                    if (line.Contains("BEGIN IONS"))
                    {
                        if (mgfData.Count > 0)
                        {
                            MassSpectrum spec = Process(mgfData);
                            if (spec != null)
                            {
                                if (spec.ScanNumber == 0)
                                {
                                    spec.ScanNumber = ++scan;
                                }
                                _lastScanNum = spec.ScanNumber;
                                WriteToOutFile(spec);
                                _processedSpecNum++;
                                progress.CurrentProgress = (int)((double) _processedSpecNum / (_totalSpecNum) * 100);
                                if (progress.CurrentProgress > _lastProgress)
                                {
                                    _lastProgress = progress.CurrentProgress;
                                    //int currentLineCursor = Console.CursorTop;
                                    //Console.SetCursorPosition(0, Console.CursorTop);
                                    //Console.Write(" Reading MGF File: " + _lastProgress + "%");
                                    //Console.SetCursorPosition(0, currentLineCursor);
                                }
                            }
                            mgfData.Clear();
                        }
                    }
                    else if (!line.Contains("END IONS"))
                    {
                        mgfData.Add(line);
                    }
                }
            }

            // process the last scan;
            MassSpectrum lastSpec = Process(mgfData);
            if (lastSpec != null)
            {
                if (lastSpec.ScanNumber == 0)
                {
                    lastSpec.ScanNumber = ++scan;
                }
                _lastScanNum = lastSpec.ScanNumber;
                WriteToOutFile(lastSpec);
                _processedSpecNum++;
                progress.CurrentProgress = (int)((double)_processedSpecNum / (_totalSpecNum) * 100);
                if (progress.CurrentProgress > _lastProgress)
                {
                    _lastProgress = progress.CurrentProgress;
                    int currentLineCursor = Console.CursorTop;
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write(" Reading MGF File: " + _lastProgress + "%");
                    Console.SetCursorPosition(0, currentLineCursor);
                }
            }
            mgfData.Clear();

            // write the MSn header into the MSn file;
            FinishConversion();
        }

        private void FinishConversion()
        {
            _mgfReader.Close();

            // close the writer first;
            if (_ms2Writer != null)
            {
                _ms2Writer.Close();
            }
            if (_ms3Writer != null)
            {
                _ms3Writer.Close();
            }

            // get the first scan number;
            File.Copy(_outFileName, _outFileName + ".tmp");
            StreamReader sr = new StreamReader(_outFileName + ".tmp");
            string line = null;
            while (!(line = sr.ReadLine()).StartsWith("S\t")) {
                line = sr.ReadLine();
            }
            string[] elems = line.Split(new char[] {'\t'});
            int firstScanNum = int.Parse(elems[1]);

            // MSn header format;
            string msnHeader = "H\tCreation Date\t" + DateTime.Now.ToString() + "\n"
                + "H\tExtractor\tRAWXtract+\n"
                + "H\tExtractorVersion\t0.1\n"
                + "H\tComments\tRawXtract+ written by Lin He, 2014\n"
                + "H\tExtractorOptions\tMSn\n"
                + "H\tAcquisitionMethod\tData-Dependent\n"
                + "H\tInstrumentType\tFTMS\n"
                + "H\tScanType\tMS2\n"
                + "H\tDataType\tCentroid\n"
                + "H\tResolution\n"
                + "H\tIsolationWindow\n"
                + "H\tFirstScan\t" + firstScanNum + "\n"
                + "H\tLastScan\t" + _lastScanNum + "\n";

            StreamWriter sw = new StreamWriter(_outFileName);
            sw.Write(msnHeader);
            sw.Write(line + "\n");
            while ((line = sr.ReadLine()) != null)
            {
                sw.Write(line + "\n");
            }
            sr.Close();
            sw.Close();

            // delete the temporary file;
            File.Delete(_outFileName + ".tmp");
        }

        /// <summary>
        /// Parse the lines between a pair of "BEGIN IONS" and "END IONS" into an instance of MassSpectrum.
        /// </summary>
        /// <param name="mgfData"></param>
        /// <returns></returns>
        private MassSpectrum Process(List<String> mgfData)
        {
            int scanNumber = 0;
            double retTime = 0;
            Activation activationType;
            double precMz = 0;
            List<int> chargeList = new List<int>();
            List<Tuple<double, int>> precursors = new List<Tuple<double, int>>();
            List<Ion> peakList = new List<Ion>();

            foreach (String row in mgfData)
            {
                if (_startsWithNumber.IsMatch(row)) //This test needs to be the first as it is the most frequent;
                {
                    // set the precursors;
                    foreach (int charge in chargeList)
                    {
                        precursors.Add(new Tuple<double, int>(precMz, charge));
                    }

                    chargeList.Clear();


                    // get ion information;
                    string[] cols = Regex.Split(row, " ");
                    if (cols.Length == 1)
                    {
                        cols = Regex.Split(row, "\t");
                    }
                    Ion ion = new Ion(double.Parse(cols[0]), double.Parse(cols[1]));
                    peakList.Add(ion);
                }
                else
                {
                    string[] cols = Regex.Split(row, "=");
                    if (cols[0].Equals("SCANS"))
                    {
                        if (!cols[1].Contains("-"))
                        {
                            scanNumber = int.Parse(cols[1]);
                        }
                        else
                        {
                            string[] colsTemp = Regex.Split(cols[1], "-");
                            scanNumber = int.Parse(colsTemp[0]);
                        }
                    }
                    else if (cols[0].Equals("RTINSECONDS"))
                    {
                        if (!cols[1].Contains("-"))
                        {
                            retTime = double.Parse(cols[1]);
                        }
                        else
                        {
                            string[] retTimeTemp = Regex.Split(cols[1], "-");
                            retTime = double.Parse(retTimeTemp[0]);
                        }
                    }
                    else if (cols[0].Equals("TITLE"))
                    {
                        string[] titleValue = Regex.Split(cols[1], ":");
                        try
                        {
                            int fragmentationIndex = titleValue.Select((item, indx) => new { Item = item, Index = indx }).Where(x => x.Item.ToLower().Contains("fragmentation")).Select(x => x.Index).Single();
                            activationType = (Activation)Enum.Parse(typeof(Activation), titleValue[fragmentationIndex + 1].ToUpper());
                        }
                        catch (Exception)
                        {
                            //If does not exists fragmentation field, CID is selected as ActivationType
                            Console.WriteLine("WARNING: Not found fragmentation type information.\n");
                            activationType = (Activation)Enum.Parse(typeof(Activation), "CID");
                        }

                    }
                    else if (cols[0].Equals("CHARGE"))
                    {
                        string[] charges = Regex.Split(cols[1], "and");
                        foreach (string charge in charges)
                        {
                            string[] elems = Regex.Split(charge, "\\+");
                            if (elems[0].Equals(""))
                            {
                                chargeList.Add(int.Parse(elems[1]));
                            }
                            else
                            {
                                chargeList.Add(int.Parse(elems[0]));
                            }
                            
                        }
                    }
                    else if (cols[0].Equals("PEPMASS"))
                    {
                        //Identify precursor intensity
                        string massWithoutIntensity;
                        try
                        {
                            string[] pepmassWithTAB = Regex.Split(cols[1], "\t");
                            if (pepmassWithTAB.Length == 1)
                            {
                                massWithoutIntensity = Regex.Split(cols[1], " ")[0];
                            }
                            else
                            {
                                massWithoutIntensity = pepmassWithTAB[0];
                            }

                        }
                        catch (Exception)
                        {
                            Console.WriteLine("WARNING: Not found Mass field in PEPMASS. Default mass = 0.0");
                            massWithoutIntensity = "0";
                        }

                        precMz = double.Parse(massWithoutIntensity);

                    }
                }

            }

            peakList.Sort((a, b) => a.MZ.CompareTo(b.MZ));
            MassSpectrum spec = new MassSpectrum(scanNumber, "", retTime, peakList, 0, InstrumentType.ELSE, "", 0);
            spec.Precursors = precursors;

            return spec;
        }

        private void WriteToOutFile(MassSpectrum spec)
        {
            // MS2 file;
            if (_ms2Writer != null)
            {
                WriteToMSn(_ms2Writer, spec);
            }

            // MS3 file;
            if (_ms3Writer != null)
            {
                WriteToMSn(_ms3Writer, spec);
            }
        }

        private void WriteToMSn(StreamWriter writer, MassSpectrum spec)
        {

            writer.Write("S\t" + String.Format("{0:000000}", spec.ScanNumber) + "\t"
                + String.Format("{0:000000}", spec.ScanNumber) + "\t"
                + Math.Round(spec.Precursors[0].Item1, _mzDecimalPlace + 1) + "\n"
                + "I\tRetTime\t" + Math.Round(spec.RetentionTime, 2) + "\n"
                + "I\tIonInjectionTime\t" + spec.IonInjectionTime + "\n"
                + "I\tActivationType\t" + spec.ActivationMethod + "\n"
                + "I\tInstrumentType\t" + spec.InstrumentType + "\n"
                + "I\tTemperatureFTAnalyzer\t" + spec.TemperatureFTAnalyzer + "\n"
                + "I\tFilter\t" + spec.Filter + "\n"
                + "I\tPrecursorScan\t" + spec.PrecursorScanNumber + "\n"
                + "I\tPrecursorInt\t"
                );
            if (!double.IsNaN(spec.PrecursorIntensity))
            {
                writer.Write(Math.Round(spec.PrecursorIntensity, _intensityDecimalPlace) + "\n");
            }
            else
            {
                writer.Write("\n");
            }

            foreach (Tuple<double, int> prec in spec.Precursors)
            {
                if (prec.Item2 != 0)
                {
                    double precMH = (prec.Item1 - Utils.PROTON_MASS) * prec.Item2 + Utils.PROTON_MASS;
                    writer.Write("Z\t" + prec.Item2 + "\t" + Math.Round(precMH, _mzDecimalPlace + 1) + "\n");
                }
                else
                {
                    foreach (int charge in _ddaDataChargeStates)
                    {
                        double precMH = (prec.Item1 - Utils.PROTON_MASS) * charge + Utils.PROTON_MASS;
                        writer.Write("Z\t" + charge + "\t" + Math.Round(precMH, _mzDecimalPlace + 1) + "\n");
                    }
                }
            }

            foreach (Ion peak in spec.Peaks)
            {
                writer.Write(Math.Round(peak.MZ, _mzDecimalPlace) + " "
                    + Math.Round(peak.Intensity, _intensityDecimalPlace) + "\n");
            }
        }

        internal void SetOptions(int mzDecimalPlace, int intensityDecimalPlace, HashSet<int> DDADataChargeStates)
        {
            this._mzDecimalPlace = mzDecimalPlace;
            this._intensityDecimalPlace = intensityDecimalPlace;
            this._ddaDataChargeStates = DDADataChargeStates;
        }
    }
}
