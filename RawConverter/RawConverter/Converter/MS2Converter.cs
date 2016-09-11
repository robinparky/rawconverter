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
    class MS2Converter
    {
        private StreamReader _ms2Reader = null;
        private Regex _startsWithNumber = new Regex("^[0-9]", RegexOptions.Compiled);
        private StreamWriter _mgfWriter = null;
        private String _ms2File = null;

        private int _mzDecimalPlace = 0;
        private int _intensityDecimalPlace = 0;

        private int _spectrumProcessed = 0;
        private int _totalSpecNum = 0;
        private double _lastProgress = 0;
        private bool isMonoIsotopic = false;

        /// <summary>
        /// Constructor
        /// </summary>
        public MS2Converter(string ms2File, string outFolder, string[] outFileTypes, bool isMonoIsotopicPeak)
        {
            _ms2Reader = new StreamReader(File.Open(ms2File, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
            _ms2File = ms2File;
            isMonoIsotopic = isMonoIsotopicPeak;
            // get the number of spectra;
            string line = null;
            while ((line = _ms2Reader.ReadLine()) != null)
            {
                if (line.StartsWith("S\t"))
                {
                    _totalSpecNum++;
                }
            }
            _ms2Reader.Close();
            _ms2Reader = new StreamReader(ms2File);

            InitWriters(Path.GetFileName(ms2File), outFolder, outFileTypes);
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
                    Regex.Replace(inFileName, ".ms2", ".", RegexOptions.IgnoreCase);
                Console.WriteLine(" Output file: " + outFileWithoutExtentionName + outFileType);
                if (outFileType.Equals("mgf", StringComparison.InvariantCultureIgnoreCase))
                {
                    _mgfWriter = new StreamWriter(outFileWithoutExtentionName + outFileType);
                }
            }
        }

        public void Convert(TaskProgress progress)
        {
            List<MassSpectrum> specList = new List<MassSpectrum>(500);
            List<String> ms2Data = new List<String>();

            string line = null;

            while ((line = _ms2Reader.ReadLine()) != null)
            {
                if (line.Length > 0)
                {
                    if (line.StartsWith("S"))
                    {
                        if (ms2Data.Count == 0)
                        {
                            ms2Data.Add(line);
                        }
                        else
                        {
                            MassSpectrum spec = Process(ms2Data);
                            WriteToOutFiles(spec);
                            _spectrumProcessed++;
                            if (progress.Aborted)
                            {
                                return;
                            }

                            progress.CurrentProgress = (int)((double)_spectrumProcessed / _totalSpecNum * 100);
                            if (progress.CurrentProgress > _lastProgress)
                            {
                                _lastProgress = progress.CurrentProgress;
                                int currentLineCursor = Console.CursorTop;
                                Console.SetCursorPosition(0, Console.CursorTop);
                                Console.Write(" Reading MS2 File: " + _lastProgress + "%");
                                Console.SetCursorPosition(0, currentLineCursor);
                            }
                            ms2Data.Clear();
                            ms2Data.Add(line);
                        }
                    }
                    else if (ms2Data.Count > 0)
                    {
                        ms2Data.Add(line);
                    }
                }
            }

            // process the last spectrum;
            MassSpectrum lastSpec = Process(ms2Data);
            _spectrumProcessed++;
            WriteToOutFiles(lastSpec);
            if (progress.Aborted)
            {
                return;
            }

            progress.CurrentProgress = (int)((double)_spectrumProcessed / _totalSpecNum * 100);
            if (progress.CurrentProgress > _lastProgress)
            {
                _lastProgress = progress.CurrentProgress;
                int currentLineCursor = Console.CursorTop;
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write(" Reading MS2 File: " + _lastProgress + "%");
                Console.SetCursorPosition(0, currentLineCursor);
            }
        }

        private MassSpectrum Process(List<String> ms2Data)
        {
            List<Ion> peakList = new List<Ion>();
            int scanNumber = 0;
            double retTime = 0;
            double precInt = 0;
            int precScan = 0;
            double ionInjectionTime = 0;
            String activationType = null;
            String instType = null;
            List<Tuple<double, int>> precursors = new List<Tuple<double, int>>();
            String pepSeq = null;

            foreach (String line in ms2Data)
            {
                if (_startsWithNumber.IsMatch(line))
                {
                    // get peak list;
                    string[] elems = Regex.Split(line, " ");
                    if (elems.Length == 1)
                    {
                        elems = Regex.Split(line, "\t");
                    }
                    peakList.Add(new Ion(double.Parse(elems[0]), double.Parse(elems[1])));
                }
                else
                {
                    string[] elems = Regex.Split(line, "\t");
                    if (elems[0].Equals("S"))
                    {
                        scanNumber = int.Parse(elems[1]);
                    }
                    else if (elems[1].Equals("RetTime"))
                    {
                        retTime = double.Parse(elems[2]);
                    }
                    else if (elems[1].Equals("PrecursorInt"))
                    {
                        precInt = double.Parse(elems[2]);
                    }
                    else if (elems[1].Equals("PrecursorScan"))
                    {
                        precScan = int.Parse(elems[2]);
                    }
                    else if (line.StartsWith("I\tIonInjectionTime"))
                    {
                        if (elems.Length >= 3 && elems[2] != null)
                        {
                            ionInjectionTime = double.Parse(elems[2]);
                        }
                    }
                    else if (line.StartsWith("I\tActivationType"))
                    {
                        activationType = elems[2];
                    }
                    else if (line.StartsWith("I\tInstrumentType"))
                    {
                        instType = elems[2];
                    }
                    else if (line.StartsWith("I\tPeptide"))
                    {
                        pepSeq = elems[2];
                    }
                    else if (line.StartsWith("Z"))
                    {
                        double precMH = double.Parse(elems[2]);
                        int precZ = int.Parse(elems[1]);
                        double precMz = (precMH + (precZ - 1) * Utils.PROTON_MASS) / precZ;
                        precursors.Add(new Tuple<double, int>(precMz, precZ));
                    }
                }

            }

            peakList.Sort((a, b) => a.MZ.CompareTo(b.MZ));
            MassSpectrum spec = new MassSpectrum(scanNumber, "", retTime, peakList, ionInjectionTime, InstrumentType.ELSE, "", 0, false);
            spec.Precursors = precursors;
            spec.PrecursorIntensity = precInt;
            spec.PrecursorScanNumber = precScan;
            spec.PeptideSequence = pepSeq;

            return spec;
        }

        private void WriteToOutFiles(MassSpectrum spec)
        {
            // MGF file;
            if (_mgfWriter != null)
            {
                TextFileWriter.WriteToMGF(_mgfWriter, spec, _ms2File, _mzDecimalPlace, _intensityDecimalPlace, false, false);
            }
        }

        public void Close()
        {
            _ms2Reader.Close();

            if (_mgfWriter != null)
            {
                _mgfWriter.Close();
            }
        }
    }
}
