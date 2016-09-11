using MSFileReaderLib;
using RawConverter.Common;
using RawConverter.DDADataProcess;
using RawConverter.DIADataProcess;
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
    class RawFileConverter
    {
        private IXRawfile5 _rawReader = null;
        private StreamWriter _mgfWriter = null;
        private StreamWriter _ms1Writer = null;
        private bool _isFirstMS1Scan = true;
        private StreamWriter _ms2Writer = null;
        private bool _isFirstMS2Scan = true;
        private StreamWriter _ms3Writer = null;
        private bool _isFirstMS3Scan = true;
        private MzXMLFileWriter _mzXMLWriter = null;
        private StreamWriter _chargeNumWriter = null;
        private string outFileWithoutExtentionName = null;

        private int FirstScanNum = 0;
        private int LastScanNum = 0;

        // variables used for peak filteration;
        private int maximumNumberOfPeaks = 400;
        private double relativeThresholdPercent = 0.01;
        private int absoluteThreshold = 10;

        private const PrecursorMassType PRECURSOR_MASS_TYPE = PrecursorMassType.Monoisotopic;
        private const bool ALWAYS_USE_PRECURSOR_CHARGE_STATE_RANGE = true;
        private int[] MS2_PRECURSOR_CHARGES = new int[] { 2, 3 };

        private bool isCentroided = true;
        private bool correctPrecMz = false;
        private bool correctPrecZ = false;
        private bool predictPrecursors = false;

        private int mzDecimalPlace = 0;
        private int intensityDecimalPlace = 0;

        private bool extractPrecByMz = false;
        private bool bypassThermoAlgorithm = false;

        private ExperimentType expType;
        private HashSet<int> DDADataChargeStates = null;

        private double lastProgress = 0;

        // the unit of the following error toleraneces is PPM;
        public const double FTMS_ERR_TOL = 20;
        public const double ITMS_ERR_TOL = 250;

        private string rawFileName;

        // whether to show the peak charge states and the peak resolution;
        private bool showPeakChargeState;
        private bool showPeakResolution;

        //whether to export charge states Statistics
        private bool exportChargeState;

        //List of MS1 scan number
        private List<int> MS1ScanNum = new List<int>();
        private int CurrentIndexInMS1 = 0;

        private double DEFAULT_DIA_ISO_WIN_SIZE = 10;

        //Collection 
        private Dictionary<int, int> chargeNum = new Dictionary<int, int>();

        public RawFileConverter(string rawFile, string outFolder, string[] outFileTypes, ExperimentType expType, bool exportChargeState)
        {
           
            _rawReader = (IXRawfile5)new MSFileReader_XRawfile();
            Console.WriteLine("_rawReader created successfully.");
            int pnMajorVersion = -1, pnMinorVersion = -1, pnSubMinorVersion = -1, nBuilderNumber = -1;
            _rawReader.Version(ref pnMajorVersion, ref pnMinorVersion, ref pnSubMinorVersion, ref nBuilderNumber);
            Console.WriteLine("pnMajorVersion = " + pnMajorVersion);
            Console.WriteLine("pnMinorVersion = " + pnMinorVersion);
            Console.WriteLine("pnSubMinorVersion = " + pnSubMinorVersion);
            Console.WriteLine("nBuilderNumber = " + nBuilderNumber);

            _rawReader.Open(rawFile);
            Console.WriteLine("_rawReader open file successfully.");
            _rawReader.SetCurrentController(0, 1);

            rawFileName = rawFile;

            // get the numbers of the first and the last scans used in MSn file headers;
            _rawReader.GetFirstSpectrumNumber(ref FirstScanNum);
            _rawReader.GetLastSpectrumNumber(ref LastScanNum);
            outFolder = Path.GetDirectoryName(rawFile);
            InitWriters(Path.GetFileName(rawFile), outFolder, outFileTypes, expType, exportChargeState);

            this.expType = expType;
        }

        public void Close()
        {
            _rawReader.Close();

            if (_mgfWriter != null)
            {
                _mgfWriter.Close();
            }
            if (_ms1Writer != null)
            {
                _ms1Writer.Close();
            }
            if (_ms2Writer != null)
            {
                _ms2Writer.Close();
            }
            if (_ms3Writer != null)
            {
                _ms3Writer.Close();
            }
            if (_mzXMLWriter != null)
            {
                _mzXMLWriter.Close();
            }
            if (_chargeNumWriter != null)
            {
                _chargeNumWriter.Close();
            }
        }

        private void InitWriters(string inFileName, string outFolder, string[] outFileTypes, ExperimentType expType, bool exportChargeState)
        {
            foreach (string outFileType in outFileTypes)
            {
                outFileWithoutExtentionName = outFolder + Path.DirectorySeparatorChar +
                    Regex.Replace(inFileName, ".raw", ".", RegexOptions.IgnoreCase);
                Console.WriteLine(" Output file: " + outFileWithoutExtentionName + outFileType);
                if (outFileType.Equals("mgf", StringComparison.InvariantCultureIgnoreCase))
                {
                    _mgfWriter = new StreamWriter(outFileWithoutExtentionName + outFileType);
                }
                if (outFileType.Equals("ms1", StringComparison.InvariantCultureIgnoreCase))
                {
                    _ms1Writer = new StreamWriter(outFileWithoutExtentionName + outFileType);
                }
                if (outFileType.Equals("ms2", StringComparison.InvariantCultureIgnoreCase))
                {
                    _ms2Writer = new StreamWriter(outFileWithoutExtentionName + outFileType);
                }
                if (outFileType.Equals("ms3", StringComparison.InvariantCultureIgnoreCase))
                {
                    _ms3Writer = new StreamWriter(outFileWithoutExtentionName + outFileType);
                }
                if (outFileType.Equals("mzxml", StringComparison.InvariantCultureIgnoreCase))
                {
                    _mzXMLWriter = new MzXMLFileWriter(outFileWithoutExtentionName + outFileType);
                    InitMzXMLFile();
                }
            }
        }

        private void InitMzXMLFile()
        {
            int scanCount = LastScanNum - FirstScanNum + 1;

            double startTime = 0, endTime = 0;
            _rawReader.GetStartTime(ref startTime);
            _rawReader.GetEndTime(ref endTime);

            // get the instrument model
            String msModel = null;
            _rawReader.GetInstModel(ref msModel);

            // get acquisition software version
            String softwareVersion = null;
            _rawReader.GetInstSoftwareVersion(ref softwareVersion);

            String filter = null;
            _rawReader.GetFilterForScanNum(FirstScanNum, ref filter);

            String rawConverterVersion = "1.0.0.x";

            _mzXMLWriter.WriteHeader(scanCount, startTime * 60, endTime * 60, rawFileName, "Thermo Scientific",
                msModel, "nanoelectrospray", msModel, msModel, "acquisition", "Xcalibur", softwareVersion, rawConverterVersion);
        }

        public void SetOptions(bool isCentroided, int mzDecimalPlace, int intensityDecimalPlace, bool extractPrecByMz, 
            bool bypassThermoAlgorithm, bool correctPrecMz, bool correctPrecZ, bool predictPrecursors, HashSet<int> DDADataChargeStates, 
            int[] ms2PrecZ, bool showPeakChargeState, bool showPeakResolution, bool exportChargeState)
        {
            this.isCentroided = isCentroided;
            this.mzDecimalPlace = mzDecimalPlace;
            this.intensityDecimalPlace = intensityDecimalPlace;
            this.extractPrecByMz = extractPrecByMz;
            this.bypassThermoAlgorithm = bypassThermoAlgorithm;
            this.correctPrecMz = correctPrecMz;
            this.correctPrecZ = correctPrecZ;
            this.predictPrecursors = predictPrecursors;
            this.DDADataChargeStates = DDADataChargeStates;
            if (ms2PrecZ != null && ms2PrecZ.Count() > 0)
            {
                this.MS2_PRECURSOR_CHARGES = ms2PrecZ;
            }
            this.showPeakChargeState = showPeakChargeState;
            this.showPeakResolution = showPeakResolution;
            this.exportChargeState = exportChargeState;
        }

        public void Convert(TaskProgress progress)
        {
            // temporarily keep the previous MS1 and MS2 spectra for possible speeding up;
            MassSpectrum lastMS1Spec = null;
            int lastMS1ScanNum = -1;
            MassSpectrum lastMS2Spec = null;
            int lastMS2ScanNum = -1;

            int spectrumProcessed = 0;

            // get instrument method names;
            int pnNumInstMethods = 0;
            object pvarNames = null;
            _rawReader.GetInstMethodNames(ref pnNumInstMethods, ref pvarNames);

            // create instance for data preprocessing;
            PrecursorPredictor pp = null;
            if (expType == ExperimentType.DIA && predictPrecursors)
            {
                pp = new PrecursorPredictor(DEFAULT_DIA_ISO_WIN_SIZE, 1, 6, 0);
            }

            //Scan whole raw file to remember scan Number of MS1
            if (correctPrecMz)
            {
                for (int scanNum = FirstScanNum; scanNum <= LastScanNum; scanNum++)
                {
                    int MSOrder = 0;
                    _rawReader.GetNumberOfMSOrdersFromScanNum(scanNum, ref MSOrder);

                    // check the MS level;
                    if (MSOrder == 0)
                    {
                        MS1ScanNum.Add(scanNum);
                    }
                }
            }


            for (int scanNum = FirstScanNum; scanNum <= LastScanNum; scanNum++)
            {
                MassSpectrum spec = GetSpectrumByScanNum(scanNum);
               // Console.Write("^^^^^" + spec.Peaks.Count);
                if (spec == null)
                {
                    continue;
                }

                // get the mass analyzer type;
                int pnMassAnalyzerType = 0;
                _rawReader.GetMassAnalyzerTypeForScanNum(scanNum, ref pnMassAnalyzerType);

                // get the precursors;
                //if (spec.MsLevel > 1)
                //{
                //    Object pvarPrecursorInfos = null;
                //    int pnArraySize = 0;
                //    _rawReader.GetPrecursorInfoFromScanNum(scanNum, ref pvarPrecursorInfos, ref pnArraySize);
                //}

                // if the spectrum has an empty peak list, abandon this spectrum;
                if (spec.Peaks.Count > 0)
                {
                    if (spec.MsLevel > 1)
                    {
                        if (spec.PrecursorScanNumber == 0)
                        {
                            if (spec.MsLevel == 2)
                            {
                                spec.PrecursorScanNumber = lastMS1Spec == null ? 0 : lastMS1ScanNum;
                            }
                            else if (spec.MsLevel == 3)
                            {
                                spec.PrecursorScanNumber = lastMS2Spec == null ? 0 : lastMS2ScanNum;
                            }
                        }
                    }

                    if (spec.MsLevel == 2)
                    {
                        if (expType == ExperimentType.DDA)
                        {
                            if (spec.PrecursorScanNumber == lastMS1ScanNum)
                            {
                              
                                // double check the precursor m/z value in the MS1 scan;
                                ExtractPrecursorMz(spec, lastMS1Spec);
                                if (correctPrecMz)
                                {
                                    spec = CalCorrectPrecursor(spec, lastMS1Spec);
                                }
                            }
                            else
                            {
                                // check whether the precursor scan number is available;
                                if (spec.PrecursorScanNumber > 0)
                                {
                                    MassSpectrum precSpec = GetSpectrumByScanNum(spec.PrecursorScanNumber);

                                    // double check the precursor m/z value in the MS1 scan;
                                    ExtractPrecursorMz(spec, precSpec);

                                    if (correctPrecMz)
                                    {
                                        spec = CalCorrectPrecursor(spec, lastMS1Spec);
                                    }
                                }
                            }
                        }
                        else if (expType == ExperimentType.DIA && predictPrecursors)
                        {
                            if (spec.PrecursorScanNumber == lastMS1ScanNum)
                            {
                                pp.PredictPrecursors(ref spec, ref lastMS1Spec);
                            }
                            else
                            {
                                // check whether the precursor scan number is available;
                                if (spec.PrecursorScanNumber > 0)
                                {
                                    MassSpectrum precSpec = GetSpectrumByScanNum(spec.PrecursorScanNumber);
                                    pp.PredictPrecursors(ref spec, ref precSpec);
                                }
                            }
                        }
                    }
                    if (spec.MsLevel == 3)
                    {
                        if (expType == ExperimentType.DDA)
                        {
                            if (spec.PrecursorScanNumber == lastMS2ScanNum)
                            {
                                // double check the precursor m/z value in the MS2 scan;
                                ExtractPrecursorMz(spec, lastMS2Spec);
                            }
                            else
                            {
                                // check whether the precursor scan number is available;
                                if (spec.PrecursorScanNumber > 0)
                                {
                                    // double check the precursor m/z value in the MS2 scan;
                                    MassSpectrum precSpec = GetSpectrumByScanNum(spec.PrecursorScanNumber);
                                    ExtractPrecursorMz(spec, precSpec);
                                }
                            }
                        }
                        else if (expType == ExperimentType.DIA && predictPrecursors)
                        {
                            if (spec.PrecursorScanNumber == lastMS1ScanNum)
                            {
                                pp.PredictPrecursors(ref spec, ref lastMS1Spec);
                            }
                            else
                            {
                                // check whether the precursor scan number is available;
                                if (spec.PrecursorScanNumber > 0)
                                {
                                    MassSpectrum precSpec = GetSpectrumByScanNum(spec.PrecursorScanNumber);
                                    pp.PredictPrecursors(ref spec, ref precSpec);
                                }
                            }
                        }
                    }


                    if (spec.MsLevel == 2 && exportChargeState)
                    {
                        if (expType == ExperimentType.DDA)
                        {
                            //Collection charge of MS2
                            if (chargeNum.ContainsKey(spec.Precursors[0].Item2))
                            {
                                chargeNum[spec.Precursors[0].Item2] += 1;
                            }
                            else
                            {
                                chargeNum.Add(spec.Precursors[0].Item2, 1);
                            }
                        }
                    }

                    // write into the output files;
                    WriteToOutFiles(spec);

                }

                if (spec.MsLevel == 1)
                {
                    lastMS1ScanNum = spec.ScanNumber;
                    lastMS1Spec = spec;
                }
                else if (spec.MsLevel == 2)
                {
                    lastMS2ScanNum = spec.ScanNumber;
                    lastMS2Spec = spec;
                }

                if (progress.Aborted)
                {
                    break;
                }
                spectrumProcessed++;
                progress.CurrentProgress = (int)((double)spectrumProcessed / (LastScanNum - FirstScanNum + 1) * 100);
                if (progress.CurrentProgress > lastProgress)
                {
                    try
                    {
                        int currentLineCursor = Console.CursorTop;
                        Console.SetCursorPosition(0, Console.CursorTop);
                        lastProgress = progress.CurrentProgress;
                        Console.WriteLine(" Reading RAW File: " + lastProgress + "%");
                        Console.SetCursorPosition(0, currentLineCursor);
                    }
                    catch (IOException ex)
                    {
                        if (progress.CurrentProgress - lastProgress >= 10 || progress.CurrentProgress == 100)
                        {
                            lastProgress = progress.CurrentProgress;
                            Console.WriteLine(" Reading RAW File: " + lastProgress + "%");
                        }
                    }
                }
            }

            if (expType == ExperimentType.DDA && exportChargeState)
            {
                _chargeNumWriter = new StreamWriter(outFileWithoutExtentionName + "txt");
                //export charge states to file           
                ExportChargeStatesToFile(chargeNum);
            }


            // complete mzXML file writing if mzXMLWriter is available;
            if (_mzXMLWriter != null)
            {
                _mzXMLWriter.WriteIndex();
                _mzXMLWriter.WriteEnd();
            }
        }

        private void ExportChargeStatesToFile(Dictionary<int, int> chargeNum)
        {
            //Select number of different charges
            var chargeStates = from pair in chargeNum
                               orderby pair.Key ascending
                               select pair;

            //write header of file
            _chargeNumWriter.Write("Charge" + "\t" + "Number" + Environment.NewLine);

            //write number of different charges to file
            foreach (KeyValuePair<int, int> pair in chargeStates)
            {
                _chargeNumWriter.Write(pair.Key + "\t" + pair.Value + Environment.NewLine);
            }
        }

        private void WriteToOutFiles(MassSpectrum spec)
        {
            // MS1 file;
            if (_ms1Writer != null && spec.MsLevel == 1)
            {
                if (_isFirstMS1Scan)
                {
                    TextFileWriter.WriteMSnHeader(_ms1Writer, "MS1", LastScanNum, spec);
                    _isFirstMS1Scan = false;
                }
                TextFileWriter.WriteToMS1(_ms1Writer, spec, mzDecimalPlace, intensityDecimalPlace, showPeakChargeState, showPeakResolution);
            }

            // MS2 file;
            if (_ms2Writer != null && spec.MsLevel == 2)
            {
                if (_isFirstMS2Scan)
                {
                    
                    TextFileWriter.WriteMSnHeader(_ms2Writer, "MS2", LastScanNum, spec);
                    _isFirstMS2Scan = false;
                }
                //TextFileWriter.WriteToMSn(_ms2Writer, spec, mzDecimalPlace, intensityDecimalPlace, showPeakChargeState, showPeakResolution,_verifyWriter);
                if (expType == ExperimentType.DIA && predictPrecursors)
                {
                    TextFileWriter.WriteToMSnWithDuplicates(_ms2Writer, spec, mzDecimalPlace, intensityDecimalPlace, showPeakChargeState, showPeakResolution);
                }
                else
                {
                    TextFileWriter.WriteToMSn(_ms2Writer, spec, mzDecimalPlace, intensityDecimalPlace, showPeakChargeState, showPeakResolution);
                }
            }

            // MS3 file;
            if (_ms3Writer != null && spec.MsLevel == 3)
            {
                if (_isFirstMS3Scan)
                {
                    TextFileWriter.WriteMSnHeader(_ms3Writer, "MS3", LastScanNum, spec);
                    _isFirstMS3Scan = false;
                }
                TextFileWriter.WriteToMSn(_ms3Writer, spec, mzDecimalPlace, intensityDecimalPlace, showPeakChargeState, showPeakResolution);
            }

            // MGF file;
            if (_mgfWriter != null && spec.MsLevel == 2)
            {
                TextFileWriter.WriteToMGF(_mgfWriter, spec, rawFileName, mzDecimalPlace, intensityDecimalPlace, showPeakChargeState, showPeakResolution);
            }

            // mzXML file;
            if (_mzXMLWriter != null)
            {
                _mzXMLWriter.WriteScan(spec);
            }
        }

        private MassSpectrum GetSpectrumByScanNum(int scanNum)
        {
            //if (scanNum >= 24021 && scanNum <= 24021)
            //{
            //    return null;
            //}
            string scanFilter = null;
            _rawReader.GetFilterForScanNum(scanNum, ref scanFilter);

            // get scan type;
            int nScanType = 0;
            _rawReader.GetScanTypeForScanNum(scanNum, ref nScanType);

            // get scan header information of the spectrum;
            int numPackets = 0, numChannels = 0, uniformTime = 0;
            double startTime = 0, lowMass = 0, highMass = 0, tic = 0, basePeakMass = 0, basePeakIntensity = 0, frequency = 0;
            _rawReader.GetScanHeaderInfoForScanNum(scanNum, ref numPackets, ref startTime, ref lowMass, ref highMass,
                ref tic, ref basePeakMass, ref basePeakIntensity, ref numChannels, ref uniformTime, ref frequency);

            // get the instrument type;
            InstrumentType instrumentType = InstrumentType.ELSE;
            if (scanFilter.StartsWith("FTMS"))
            {
                instrumentType = InstrumentType.FTMS;
            }
            else if (scanFilter.StartsWith("ITMS"))
            {
                instrumentType = InstrumentType.ITMS;
            }

            // get MS spectrum list;
            string spectrumId = "controllerType=0 controllerNumber=1 scan=" + scanNum.ToString();

            // get the retention time;
            double retentionTime = double.NaN;
            _rawReader.RTFromScanNum(scanNum, ref retentionTime);

            // get the ion injection time and the precursor information;
            object trailerLabelsObj = null;
            object trailerValuesObj = null;
            int trailer_array_size = -1;
            _rawReader.GetTrailerExtraForScanNum(scanNum, ref trailerLabelsObj, ref trailerValuesObj, ref trailer_array_size);
            string[] trailerLabels = (string[])trailerLabelsObj;
            string[] trailerValues = (string[])trailerValuesObj;

            double ionInjectionTime = 0;
            double precMz = 0;
            int precZ = 0;
            int precScanNum = 0;
            double isolationWindowSize = 0;
            for (int trailerIdx = trailerLabels.GetLowerBound(0); trailerIdx <= trailerLabels.GetUpperBound(0); trailerIdx++)
            {
                if (trailerLabels[trailerIdx].StartsWith("Ion Injection Time"))
                {
                    ionInjectionTime = double.Parse(trailerValues[trailerIdx]);
                }
                if (trailerLabels[trailerIdx].StartsWith("Monoisotopic M/Z"))
                {
                    precMz = double.Parse(trailerValues[trailerIdx]);
                }
                if (trailerLabels[trailerIdx].StartsWith("Charge State"))
                {
                    precZ = int.Parse(trailerValues[trailerIdx]);
                }
                if (trailerLabels[trailerIdx].StartsWith("Master Scan Number"))
                {
                    precScanNum = int.Parse(trailerValues[trailerIdx]);
                }
                if (trailerLabels[trailerIdx].StartsWith("MS2 Isolation Width"))
                {
                    isolationWindowSize = double.Parse(trailerValues[trailerIdx]);
                }
            }

            // get the analyzer temperature from the status log;
            object logLabelsObj = null;
            object logValuesObj = null;
            int pnArraySize = 0;
            _rawReader.GetStatusLogForScanNum(scanNum, 1.0, ref logLabelsObj, ref logValuesObj, ref pnArraySize);
            double analyzerTemperature = -1.0;

            if (logLabelsObj != null && logValuesObj != null)
            {
                string[] logLabels = (string[])logLabelsObj;
                string[] logValues = (string[])logValuesObj;

                for (int idx = logLabels.GetLowerBound(0); idx <= logLabels.GetUpperBound(0); idx++)
                {
                    if (logLabels[idx].Contains("FT Analyzer Temp"))
                    {
                        analyzerTemperature = double.Parse(logValues[idx]);
                    }

                    if (logLabels[idx].Contains("Analyzer Temperature"))
                    {
                        analyzerTemperature = double.Parse(logValues[idx]);
                    }
                }
            }

            // create an instance of MassSpectrum;
            List<Ion> peaks = null;
            MassSpectrum spectrum = new MassSpectrum(
                (int)scanNum,
                spectrumId,
                retentionTime,
                peaks,
                ionInjectionTime,
                instrumentType,
                scanFilter,
                analyzerTemperature,
                correctPrecMz);

            // check the MS level;
            if (scanFilter.Contains(" ms "))
            {
                spectrum.MsLevel = 1;

                // do not filter peaks for MS1 as all peaks may be needed for later masss/charge correction;
                spectrum.Peaks = GetPeaks(scanNum, scanFilter, false);
            }
            else if (scanFilter.Contains(" ms2 "))
            {
                spectrum.MsLevel = 2;
                int activationType = 0;
                //int pnNumMSOrders = 0;
                _rawReader.GetActivationTypeForScanNum(scanNum, spectrum.MsLevel, ref activationType);
                //_rawReader.GetNumberOfMSOrdersFromScanNum(scanNum, ref pnNumMSOrders);
                //_rawReader.GetIsolationWidthForScanNum(scanNum, pnNumMSOrders, ref isolationWindowSize);
                spectrum.IsolationWindowSize = isolationWindowSize;
                spectrum.ActivationMethod = (Activation)activationType;
                spectrum.Peaks = GetPeaks(scanNum, scanFilter, false);
                spectrum.Precursors = new List<Tuple<double, int>>();
                spectrum.PrecursorIntensity = 0;
                spectrum.PrecursorScanNumber = precScanNum;

                // get the precMz from the filter;
                int begin = scanFilter.IndexOf(" ms2 ") + 5;
                int end = begin + 1, len = 1;
                while (end < scanFilter.Count() && (scanFilter[end] >= '0' && scanFilter[end] <= '9') || scanFilter[end] == '.')
                {
                    end++;
                    len++;
                }
                spectrum.PrecMzFromFilter = double.Parse(scanFilter.Substring(begin, len));

                //Console.Write(bypassThermoAlgorithm);
                //Console.Write(precMz);               
                if (precMz == 0 || bypassThermoAlgorithm)
                {
                 
                    precMz = spectrum.PrecMzFromFilter;
                }
               
                if (precZ == 0 && ALWAYS_USE_PRECURSOR_CHARGE_STATE_RANGE)
                {
                    foreach (int z in MS2_PRECURSOR_CHARGES)
                    {
                        spectrum.Precursors.Add(new Tuple<double, int>(precMz, z));
                    }
                }
                else
                {
                    spectrum.Precursors.Add(new Tuple<double, int>(precMz, precZ));
                }

            }
            else if (scanFilter.Contains(" ms3 "))
            {
                spectrum.MsLevel = 3;
                if (precMz == 0)
                {
                    int end = scanFilter.Count() - 1;
                    while (end >= 0 && scanFilter[end] != '@')
                    {
                        end--;
                    }
                    int begin = end - 1, len = 1;
                    while (begin >= 0 && (scanFilter[begin] >= '0' && scanFilter[begin] <= '9') || scanFilter[begin] == '.')
                    {
                        begin--;
                        len++;
                    }
                    begin++;
                    len--;
                    precMz = double.Parse(scanFilter.Substring(begin, len));
                }

                // TODO: MS3 spectrum mass/charge correction need to be taken care;
                spectrum.Precursors = new List<Tuple<double, int>>();
                spectrum.Precursors.Add(new Tuple<double, int>(precMz, precZ));

                spectrum.Peaks = GetPeaks(scanNum, scanFilter, false);
                if (precMz < 0.01)
                {
                    for (int trailerIdx = trailerLabels.GetLowerBound(0); trailerIdx <= trailerLabels.GetUpperBound(0); trailerIdx++)
                    {
                        if (trailerLabels[trailerIdx].StartsWith("SPS Mass 1"))
                        {
                            precMz = double.Parse(trailerValues[trailerIdx]);
                            break;
                        }
                    }
                }
                spectrum.Precursors = new List<Tuple<double, int>>();
                spectrum.PrecursorIntensity = double.NaN;
                spectrum.PrecursorScanNumber = precScanNum;
                int activationType = 0;
                _rawReader.GetActivationTypeForScanNum(scanNum, spectrum.MsLevel, ref activationType);
                spectrum.ActivationMethod = (Activation)activationType;

                if (precZ == 0 && ALWAYS_USE_PRECURSOR_CHARGE_STATE_RANGE)
                {
                    int maxCharge = 0;
                    foreach (int z in MS2_PRECURSOR_CHARGES)
                    {
                        if (maxCharge < z)
                        {
                            maxCharge = z;
                        }
                    }
                    for (int z = 0; z <= maxCharge; z++)
                    {
                        spectrum.Precursors.Add(new Tuple<double, int>(precMz, z));
                    }
                }
                else
                {
                    spectrum.Precursors.Add(new Tuple<double, int>(precMz, precZ));
                }
            }

            // get the isolation window size;
            //double isoWinSize = 0;
            //int numMSOrders = 0;
            //_rawReader.GetNumberOfMSOrdersFromScanNum(scanNum, ref numMSOrders);
            //for (int i = 0; i <= 10; i++)
            //{
            //    _rawReader.GetIsolationWidthForScanNum(scanNum, i, ref isoWinSize);
            //    Console.WriteLine(i + "\t" + isoWinSize);
            //}
            //spectrum.IsolationWindowSize = isoWinSize;
            //_rawReader.GetIsolationWidthForScanNum(scanNum, spectrum.MsLevel, ref isoWinSize);
            //Console.WriteLine(spectrum.MsLevel + "\t" + isoWinSize);

            // set the scan header information;
            spectrum.TotIonCurrent = tic;
            spectrum.LowMz = lowMass;
            spectrum.HighMz = highMass;
            spectrum.BasePeakMz = basePeakMass;
            spectrum.BasePeakIntensity = basePeakIntensity;

            return spectrum;
        }

        private void ExtractPrecursorMz(MassSpectrum spec, MassSpectrum precSpec)
        {
            if (spec == null || precSpec == null)
            {
                return;
            }

            List<Ion> peakMzList = precSpec.Peaks;

            double precMz = spec.Precursors[0].Item1;
            int precZ = spec.Precursors[0].Item2;
            Ion qPeak = new Ion(precMz, 0, precZ);
            IonMzComparer imc = new IonMzComparer();
            
            
            int pos = peakMzList.BinarySearch(qPeak, imc);
            
            if (pos < 0)
            {
                pos = -pos - 1;
            }

            // calculate the upper and lower bounds of considered m/z values;
            double err = 0;
            if (precSpec.InstrumentType == InstrumentType.FTMS)
            {
                err = precMz * FTMS_ERR_TOL / 1000000;
            }
            else
            {
                err = precMz * ITMS_ERR_TOL / 1000000;
            }
            double upper = precMz + err;
            double lower = precMz - err;

            // select all the peaks with m/z in this range;
            List<Ion> peaksInWindow = new List<Ion>();
            for (int i = pos - 1; i >= 0; i--)
            {
                if (peakMzList[i].MZ >= lower)
                {
                    peaksInWindow.Add(peakMzList[i]);
                }
            }
            for (int i = pos; i < peakMzList.Count; i++)
            {
                if (peakMzList[i].MZ <= upper)
                {
                    peaksInWindow.Add(peakMzList[i]);
                }
            }

            // if the peaksInWindow list is empty, just return;
            if (peaksInWindow.Count == 0)
            {
                return;
            }
            
            // search for the peak with closest m/z and the peak with the highest intensity;
            double mzDist = double.MaxValue;
            int closestIdx = 0;
            double maxH = double.MinValue;
            int maxHIdx = 0;
            for (int i = 0; i < peaksInWindow.Count; i++)
            {
                if (mzDist > Math.Abs(precMz - peaksInWindow[i].MZ))
                {
                    mzDist = Math.Abs(precMz - peaksInWindow[i].MZ);
                    closestIdx = i;
                }
                if (maxH < peaksInWindow[i].Intensity)
                {
                    maxH = peaksInWindow[i].Intensity;
                    maxHIdx = i;
                }
            }
            

            // determine the precursor m/z value;
            
            if (extractPrecByMz)
            {
                List<Tuple<double, int>> newPrecList = new List<Tuple<double, int>>();
                for (int i = 0; i < spec.Precursors.Count; i++)
                {
                    newPrecList.Add(new Tuple<double, int>(peaksInWindow[closestIdx].MZ, spec.Precursors[i].Item2));
                }
                spec.Precursors = newPrecList;
                spec.PrecursorIntensity = peaksInWindow[closestIdx].Intensity;
            }
            else
            {
                List<Tuple<double, int>> newPrecList = new List<Tuple<double, int>>();
                for (int i = 0; i < spec.Precursors.Count; i++)
                {
                    newPrecList.Add(new Tuple<double, int>(peaksInWindow[maxHIdx].MZ, spec.Precursors[i].Item2));
                }
                spec.Precursors = newPrecList;
               
                spec.PrecursorIntensity = peaksInWindow[maxHIdx].Intensity;
            }

        }

        private List<double> GetPeakMzValues(int scanNumber, string scanFilter)
        {
            // read the peaks from RAW data;
            double[,] label_data = GetFragmentationData(_rawReader, scanNumber, scanFilter);
            List<double> peakMzList = new List<double>(label_data.GetLength(1));
            for (int peak_index = label_data.GetLowerBound(1); peak_index <= label_data.GetUpperBound(1); peak_index++)
            {
                peakMzList.Add(label_data[(int)RawLabelDataColumn.MZ, peak_index]);
            }

            // sort the peaks;
            peakMzList.Sort(delegate(double mz1, double mz2)
            {
                if (mz1 < mz2)
                    return -1;
                else if (mz1 > mz2)
                    return 1;
                else
                    return 0;
            });

            return peakMzList;
        }

        private List<Ion> GetPeaks(int scanNumber, string scanFilter, bool doFilter)
        {
            // read the peaks from RAW data;
            double[,] labelData = GetFragmentationData(_rawReader, scanNumber, scanFilter);
            if (labelData == null)
            {
                return new List<Ion>();
            }

            List<Ion> peaks = new List<Ion>(labelData.GetLength(1));
            for (int peakIndex = labelData.GetLowerBound(1); peakIndex <= labelData.GetUpperBound(1); peakIndex++)
            {
                peaks.Add(new Ion(labelData[(int)RawLabelDataColumn.MZ, peakIndex],
                    labelData[(int)RawLabelDataColumn.Intensity, peakIndex],
                    (int)RawLabelDataColumn.Charge < labelData.GetLength(0) ? (int)labelData[(int)RawLabelDataColumn.Charge, peakIndex] : 0,
                    (int)RawLabelDataColumn.Resolution < labelData.GetLength(0) ? (int)labelData[(int)RawLabelDataColumn.Resolution, peakIndex] : 0));
            }

            // sort the peaks;
            peaks.Sort(delegate(Ion p1, Ion p2)
            {
                if (p1.MZ < p2.MZ)
                    return -1;
                else if (p1.MZ > p2.MZ)
                    return 1;
                else
                    return 0;
            });

            // do filteration if required;
            if (doFilter)
            {
                peaks = FilterPeaks(peaks, absoluteThreshold, relativeThresholdPercent, maximumNumberOfPeaks);
            }
            return peaks;
        }

        /// <summary>
        /// Get fragmentation data from RAW data.
        /// </summary>
        /// <param name="raw"></param>
        /// <param name="scanNumber"></param>
        /// <param name="scanFilter"></param>
        /// <returns></returns>
        private double[,] GetFragmentationData(IXRawfile2 raw, int scanNumber, string scanFilter)
        {
            double[,] data;
            
            if (scanFilter.ToLower().Contains("ftms") && isCentroided)
            {
                //get the FT-PROFILE labels as the centroided peaks;
                object labelsObj = null;
                object flagsObj = null;
                raw.GetLabelData(ref labelsObj, ref flagsObj, ref scanNumber);
                data = (double[,])labelsObj;
            }
            else
            {
                int centroidStatus = isCentroided ? 1 : 0;
                double centroidPeakWidth = double.NaN;
                object massList = null;
                object peakFlags = null;
                int arraySize = -1;
                raw.GetMassListFromScanNum(scanNumber, null, 0, 0, 0, centroidStatus, ref centroidPeakWidth, ref massList, ref peakFlags, ref arraySize);
                data = (double[,])massList;
            }

            return data;
        }

        /// <summary>
        /// Determine what kind of fragmentation is contained into file
        /// </summary>
        /// <param name="scanFilter"></param>
        /// <returns></returns>
        private Activation GetFragmentationMethod(string scanFilter)
        {
            if (scanFilter.Contains("cid"))
            {
                return Activation.CID;
            }
            else if (scanFilter.Contains("mpd"))
            {
                return Activation.MPD;
            }
            else if (scanFilter.Contains("pqd"))
            {
                return Activation.PQD;
            }
            else if (scanFilter.Contains("hcd"))
            {
                return Activation.HCD;
            }
            else if (scanFilter.Contains("ecd"))
            {
                return Activation.ECD;
            }
            else if (scanFilter.Contains("etd"))
            {
                return Activation.ETD;
            }
            else
            {
                //not found
                return Activation.Any;
            }
        }

        /// <summary>
        /// Filter better peaks
        /// </summary>
        /// <param name="peaks"></param>
        /// <param name="absoluteThreshold"></param>
        /// <param name="relativeThresholdPercent"></param>
        /// <param name="maximumNumberOfPeaks"></param>
        /// <returns></returns>
        private List<Ion> FilterPeaks(List<Ion> peaks, double absoluteThreshold, double relativeThresholdPercent, int maximumNumberOfPeaks)
        {
            List<Ion> filteredPeaks = new List<Ion>(peaks);

            double relativeThreshold = -1.0;
            if (relativeThresholdPercent > 0.0)
            {
                double maxIntensity = -1.0;
                foreach (Ion peak in filteredPeaks)
                {
                    double intensity = peak.Intensity;
                    if (intensity > maxIntensity)
                    {
                        maxIntensity = intensity;
                    }
                }
                relativeThreshold = maxIntensity * relativeThresholdPercent / 100.0;
            }

            double threshold = Math.Max(absoluteThreshold, relativeThreshold);

            int p = 0;
            while (p < filteredPeaks.Count)
            {
                Ion peak = filteredPeaks[p];
                if (peak.Intensity < threshold)
                {
                    filteredPeaks.RemoveAt(p);
                }
                else
                {
                    p++;
                }
            }

            if (maximumNumberOfPeaks > 0 && filteredPeaks.Count > maximumNumberOfPeaks)
            {
                filteredPeaks.Sort(Ion.DescendingIntensityComparison);
                filteredPeaks.RemoveRange(maximumNumberOfPeaks, filteredPeaks.Count - maximumNumberOfPeaks);
                // re-sort the peak list according to the m/z values;
                filteredPeaks.Sort(Ion.AscendingMassComparison);
            }

            return filteredPeaks;
        }

        //Perdicted monoisotopic with 3 MS1
        private MassSpectrum CalCorrectPrecursor(MassSpectrum currentMS2, MassSpectrum currentMS1Spec)
        {
            if (currentMS1Spec == null || currentMS2 == null)
            {
                return currentMS2;
            }

            List<MassSpectrum> currentMS1 = new List<MassSpectrum>();

            //Add current MS1
            currentMS1.Add(currentMS1Spec);

            CurrentIndexInMS1 = MS1ScanNum.IndexOf(currentMS1Spec.ScanNumber);

            //Add current MS1 - 1 
            if (CurrentIndexInMS1 - 1 > 0)
            {
                MassSpectrum MS1spec = GetSpectrumByScanNum(MS1ScanNum[CurrentIndexInMS1 - 1]);
                currentMS1.Add(MS1spec);
            }
            //Add current MS1 + 1 
            if (CurrentIndexInMS1 + 1 < MS1ScanNum.Count)
            {
                MassSpectrum MS1spec = GetSpectrumByScanNum(MS1ScanNum[CurrentIndexInMS1 + 1]);
                currentMS1.Add(MS1spec);
            }

            ////Add current MS1 - 2 
            //if (CurrentIndexInMS1 - 2 > 0)
            //{
            //    MassSpectrum MS1spec = GetSpectrumByScanNum(MS1ScanNum[CurrentIndexInMS1 - 2]);
            //    currentMS1.Add(MS1spec);
            //}
            ////Add current MS1 + 2 
            //if (CurrentIndexInMS1 + 2 < MS1ScanNum.Count)
            //{
            //    MassSpectrum MS1spec = GetSpectrumByScanNum(MS1ScanNum[CurrentIndexInMS1 + 2]);
            //    currentMS1.Add(MS1spec);
            //}


            PrecursorCorrector pc = new PrecursorCorrector();

            // set the precursor scan number for current spectrum;
            currentMS2.PrecursorScanNumber = currentMS1Spec.ScanNumber;

            // get the precursor m/z from the scan filter;
            double precMz = currentMS2.Precursors.Count == 0 ? 0 : currentMS2.Precursors[0].Item1;
            int precZ = currentMS2.Precursors.Count == 0 ? 0 : currentMS2.Precursors[0].Item2;

            pc.CorrectPrecursor(ref currentMS2, precMz, precZ, currentMS1Spec.ScanNumber, currentMS1);

            return currentMS2;
        }

        internal enum PrecursorMassType
        {
            Isolation,
            Monoisotopic
        }

        internal enum RawLabelDataColumn
        {
            MZ = 0,
            Intensity = 1,
            Resolution = 2,
            NoiseBaseline = 3,
            NoiseLevel = 4,
            Charge = 5
        }
    }

    //struct PrecursorInfo
    //{
    //    double dIsolationMass;
    //    double dMonoIsoMass;
    //    int nChargeState;
    //    int nScanNumber;
    //};
}
