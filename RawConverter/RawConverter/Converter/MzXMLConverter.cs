using RawConverter.Common;
using RawConverter.MassSpec;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace RawConverter.Converter
{
    class MzXMLConverter
    {
        private XmlReader _xmlReader = null;
        private StreamWriter _mgfWriter = null;
        private StreamWriter _ms1Writer = null;
        private bool _isFirstMS1Scan = true;
        private StreamWriter _ms2Writer = null;
        private bool _isFirstMS2Scan = true;
        private StreamWriter _ms3Writer = null;
        private bool _isFirstMS3Scan = true;

        private String _mzxmlFileName = null;

        private int _scanCount = 0;
        
        private int _mzDecimalPlace = 0;
        private int _intensityDecimalPlace = 0;

        private double _lastProgress = 0;
        private int _spectrumProcessed = 0;

        public MzXMLConverter(string xmlFile, string outFolder, string[] outFileTypes, ExperimentType expType)
        {
            _xmlReader = new XmlTextReader(File.Open(xmlFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
            _mzxmlFileName = xmlFile;
            InitWriters(Path.GetFileName(xmlFile), outFolder, outFileTypes);
        }

        public void SetOptions(int mzDecimalPlace, int intensityDecimalPlace)
        {
            _mzDecimalPlace = mzDecimalPlace;
            _intensityDecimalPlace = intensityDecimalPlace;
        }

        private void InitWriters(string inFileName, string outFolder, string[] outFileTypes)
        {
            foreach (string outFileType in outFileTypes)
            {
                string outFileWithoutExtentionName = outFolder + Path.DirectorySeparatorChar +
                    Regex.Replace(inFileName, ".mzxml", ".", RegexOptions.IgnoreCase);
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
            }
        }

        public void Convert(TaskProgress progress)
        {
            MassSpectrum spec = null;
            bool isFirstScan = true;

            while (_xmlReader.Read())
            {
                if (_xmlReader.IsStartElement())
                {
                    switch (_xmlReader.Name)
                    {
                        case "msRun":
                            _scanCount = System.Convert.ToInt32(_xmlReader.GetAttribute("scanCount"));
                            break;
                        case "scan":
                            int scanNum = System.Convert.ToInt32(_xmlReader.GetAttribute("num"));
                            int msLevel = System.Convert.ToInt32(_xmlReader.GetAttribute("msLevel"));
                            int peaksCount = System.Convert.ToInt32(_xmlReader.GetAttribute("peaksCount"));
                            String filter = _xmlReader.GetAttribute("filterLine");
                            String strRtTime = _xmlReader.GetAttribute("retentionTime");
                            double retentionTime = 0;
                            if (strRtTime != null)
                            {
                                strRtTime = strRtTime.Replace("PT", "");
                                strRtTime = strRtTime.Replace("S", "");
                                retentionTime = System.Convert.ToDouble(strRtTime) / 60;
                            }
                            double lowMz = System.Convert.ToDouble(_xmlReader.GetAttribute("lowMz"));
                            double highMz = System.Convert.ToDouble(_xmlReader.GetAttribute("highMz"));
                            double basePeakMz = System.Convert.ToDouble(_xmlReader.GetAttribute("basePeakMz"));
                            double basePeakIntensity = System.Convert.ToDouble(_xmlReader.GetAttribute("basePeakIntensity"));
                            double totIonCurrent = System.Convert.ToDouble(_xmlReader.GetAttribute("totIonCurrent"));

                            InstrumentType instType = InstrumentType.ELSE;
                            if (filter != null && filter.Contains("FTMS"))
                            {
                                instType = InstrumentType.FTMS;
                            }
                            else if (filter != null && filter.Contains("ITMS"))
                            {
                                instType = InstrumentType.ITMS;
                            }

                            // create a spectrum;
                            spec = new MassSpectrum(scanNum, _xmlReader.GetAttribute("num"), retentionTime, new List<Ion>(), 0, instType, filter, 0, false);
                            spec.MsLevel = msLevel;
                            spec.LowMz = lowMz;
                            spec.HighMz = highMz;
                            spec.BasePeakMz = basePeakMz;
                            spec.BasePeakIntensity = basePeakIntensity;
                            spec.TotIonCurrent = totIonCurrent;
                            spec.Precursors = new List<Tuple<double, int>>();
                            break;
                        case "precursorMz":
                            int precursorScanNum = System.Convert.ToInt32(_xmlReader.GetAttribute("precursorScanNum"));
                            double precursorIntensity = System.Convert.ToDouble(_xmlReader.GetAttribute("precursorIntensity"));
                            String activationMethod = _xmlReader.GetAttribute("activationMethod");
                            String strPrecCharge = _xmlReader.GetAttribute("precursorCharge");
                            int precursorCharge = strPrecCharge.Length > 0 ? System.Convert.ToInt32(strPrecCharge) : 0;
                            // read the precursor m/z value;
                            _xmlReader.Read();
                            String strPrecMz = _xmlReader.Value;
                            double precursorMz = strPrecMz.Length > 0 ? System.Convert.ToDouble(strPrecMz) : 0;
                            spec.PrecursorIntensity = precursorIntensity;
                            spec.PrecursorScanNumber = precursorScanNum;
                            spec.ActivationMethod = (Activation)Enum.Parse(typeof(Activation), activationMethod); 
                            spec.Precursors.Add(new Tuple<double, int>(precursorMz, precursorCharge));
                            break;
                        case "peaks":
                            int precision = System.Convert.ToInt32(_xmlReader.GetAttribute("precision"));
                            String pairOrder = _xmlReader.GetAttribute("pairOrder");
                            // read the base64 code;
                            _xmlReader.Read();
                            String strBase64 = _xmlReader.Value;
                            if (strBase64 != null)
                            {
                                ConvertBase64ToPeakList(strBase64, spec.Peaks, precision);
                            }
                            WriteToOutFiles(spec, isFirstScan);
                            isFirstScan = false;
                            _spectrumProcessed++;
                            break;
                    }
                }

                if (progress.Aborted)
                {
                    break;
                }

                progress.CurrentProgress = (int)((double)_spectrumProcessed / _scanCount * 100);
                //Console.WriteLine(_spectrumProcessed + "\t" + progress.CurrentProgress);
                if (progress.CurrentProgress > _lastProgress)
                {
                    _lastProgress = progress.CurrentProgress;
                    int currentLineCursor = Console.CursorTop;
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write(" Reading mzXML File: " + _lastProgress + "%");
                    Console.SetCursorPosition(0, currentLineCursor);
                }
            }
        }

        private void ConvertBase64ToPeakList(string strBase64, List<Ion> peakList, int precision)
        {
            byte[] byteData = System.Convert.FromBase64String(strBase64);
            if (precision == 32)
            {
                if (byteData.Length % 4 != 0)
                {
                    Console.WriteLine("!!!!!!!!!Some nauty thing happened in ConvertBase64ToPeakList, do not believe in the output!!!!!!!!");
                    return;
                }

                for (int byteIdx = 0; byteIdx < byteData.Length; byteIdx+=8)
                {
                    byte[] mzBytes = new byte[4];
                    byte[] hBytes = new byte[4];
                    Buffer.BlockCopy(byteData, byteIdx, mzBytes, 0, 4);
                    Buffer.BlockCopy(byteData, byteIdx + 4, hBytes, 0, 4);

                    // reverse the byte arrays;
                    ReverseByteArray(mzBytes);
                    ReverseByteArray(hBytes);

                    // retrieve the mz and intensity;
                    double mz = BitConverter.ToSingle(mzBytes, 0);
                    double intensity = BitConverter.ToSingle(hBytes, 0);
                    peakList.Add(new Ion(mz, intensity));
                }
            }
            else if (precision == 64)
            {
                if (byteData.Length % 8 != 0)
                {
                    Console.WriteLine("!!!!!!!!!Some nauty thing happened in ConvertBase64ToPeakList, do not believe in the output!!!!!!!!");
                    return;
                }

                for (int byteIdx = 0; byteIdx < byteData.Length; byteIdx += 16)
                {
                    byte[] mzBytes = new byte[8];
                    byte[] hBytes = new byte[8];
                    Buffer.BlockCopy(byteData, byteIdx, mzBytes, 0, 8);
                    Buffer.BlockCopy(byteData, byteIdx + 8, hBytes, 0, 8);

                    // reverse the byte arrays;
                    ReverseByteArray(mzBytes);
                    ReverseByteArray(hBytes);

                    // retrieve the mz and intensity;
                    double mz = BitConverter.ToDouble(mzBytes, 0);
                    double intensity = BitConverter.ToDouble(hBytes, 0);
                    peakList.Add(new Ion(mz, intensity));
                }
            }

        }

        private void ReverseByteArray(byte[] byteArr)
        {
            int begin = 0, end = byteArr.Length - 1;
            while (begin < end)
            {
                byte temp = byteArr[begin];
                byteArr[begin] = byteArr[end];
                byteArr[end] = temp;
                begin++;
                end--;
            }
        }

        private void WriteToOutFiles(MassSpectrum spec, bool isFirstScan)
        {
            // MS1 file;
            if (_ms1Writer != null && spec.MsLevel == 1)
            {
                if (_isFirstMS1Scan)
                {
                    TextFileWriter.WriteMSnHeader(_ms1Writer, "MS1", _scanCount, spec);
                    _isFirstMS1Scan = false;
                }
                TextFileWriter.WriteToMS1(_ms1Writer, spec, _mzDecimalPlace, _intensityDecimalPlace, false, false);
            }

            // MS2 file;
            if (_ms2Writer != null && spec.MsLevel == 2)
            {
                if (_isFirstMS2Scan)
                {
                    TextFileWriter.WriteMSnHeader(_ms2Writer, "MS2", _scanCount, spec);
                    _isFirstMS2Scan = false;
                }
                TextFileWriter.WriteToMSn(_ms2Writer, spec, _mzDecimalPlace, _intensityDecimalPlace, false, false);
            }

            // MS3 file;
            if (_ms3Writer != null && spec.MsLevel == 3)
            {
                if (_isFirstMS3Scan)
                {
                    TextFileWriter.WriteMSnHeader(_ms3Writer, "MS3", _scanCount, spec);
                    _isFirstMS3Scan = false;
                }
                TextFileWriter.WriteToMSn(_ms3Writer, spec, _mzDecimalPlace, _intensityDecimalPlace, false, false);
            }

            // MGF file;
            if (_mgfWriter != null && spec.MsLevel == 2)
            {
                TextFileWriter.WriteToMGF(_mgfWriter, spec, _mzxmlFileName, _mzDecimalPlace, _intensityDecimalPlace, false, false);
            }
        }

        public void Close()
        {
            _xmlReader.Close();

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
        }
        
    }
}
