using RawConverter.Common;
using RawConverter.DDADataProcess;
using RawConverter.MassSpec;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace RawConverter.Converter
{
    class MzMLConverter
    {
        private StreamWriter _mgfWriter = null;
        private StreamWriter _ms1Writer = null;
        private bool _isFirstMS1Scan = true;
        private StreamWriter _ms2Writer = null;
        private bool _isFirstMS2Scan = true;
        private StreamWriter _ms3Writer = null;
        private bool _isFirstMS3Scan = true;

        private string _mzmlFileName = null;
        private string _mzmlFile = null;
        private mzMLType mzML = null;

        private int _scanCount = 0;

        private int _mzDecimalPlace = 0;
        private int _intensityDecimalPlace = 0;
        private bool _correctPrecMz = false;

        private double _lastProgress = 0;
        private int _spectrumProcessed = 0;

        public MzMLConverter(string mzmlFile, string outFolder, string[] outFileTypes)
        {
            _mzmlFile = mzmlFile;
            InitWriters(Path.GetFileName(mzmlFile), outFolder, outFileTypes);
        }

        public void SetOptions(int mzDecimalPlace, int intensityDecimalPlace, bool correctPrecMz)
        {
            _mzDecimalPlace = mzDecimalPlace;
            _intensityDecimalPlace = intensityDecimalPlace;
            _correctPrecMz = correctPrecMz;
        }

        private void InitWriters(string inFileName, string outFolder, string[] outFileTypes)
        {
            foreach (string outFileType in outFileTypes)
            {
                string outFileWithoutExtentionName = outFolder + Path.DirectorySeparatorChar +
                    Regex.Replace(inFileName, ".mzml", ".", RegexOptions.IgnoreCase);
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
            int currentMs1ScanNum = 0;
            bool isScanMS2 = false;

            List<MassSpectrum> tmpMS1 = new List<MassSpectrum>();
            List<MassSpectrum> NeedToCorrectMS2 = new List<MassSpectrum>();

            using (XmlReader reader = XmlReader.Create(_mzmlFile))
            {

                reader.ReadToFollowing("mzML");
                XmlSerializer serializer = new XmlSerializer(typeof(mzMLType));

                //Reading mzML file to mzMLType class
                mzML = (mzMLType)serializer.Deserialize(reader);

                _scanCount = mzML.run.spectrumList.spectrum.Count();

                for (int i = 0; i < _scanCount; i++)
                {
                    int scanNum = System.Convert.ToInt32(mzML.run.spectrumList.spectrum[i].index.Replace("\"", ""));
                    InstrumentType instType = InstrumentType.ELSE;
                    string filter = "";
                    string refParGup = "";
                    double retentionTime = 0;
                    int msLevel = 0;
                    double lowMz = 0;
                    double highMz = 0;
                    double basePeakMz = 0;
                    double basePeakIntensity = 0;
                    double totIonCurrent = 0;
                    double precursorMz = 0;
                    int precursorCharge = 0;
                    byte[] uncompressedMz = null;
                    byte[] uncompressedIntensity = null;
                    int shiftByte = 8;

                    if (mzML.run.spectrumList.spectrum[i].referenceableParamGroupRef !=null )
                    {
                        // get referenceableParamGroup
                        refParGup = mzML.run.spectrumList.spectrum[i].referenceableParamGroupRef[0].@ref;
                        // reference to referenceableParamGroup includes ms level, low MZ, High MZ, basePeak MZ and basePeak Intensity
                        foreach (var refPargup in mzML.referenceableParamGroupList.referenceableParamGroup)
                        {
                            if (refPargup.id == refParGup)
                            {
                                foreach (var cvParm in refPargup.cvParam)
                                {
                                    switch (cvParm.name)
                                    {
                                        case "ms level":
                                            msLevel = System.Convert.ToInt32(cvParm.value.Replace("\"", ""));
                                            break;
                                        case "lowest observed m/z":
                                            lowMz = System.Convert.ToDouble(cvParm.value.Replace("\"", ""));
                                            break;
                                        case "highest observed m/z":
                                            highMz = System.Convert.ToDouble(cvParm.value.Replace("\"", ""));
                                            break;
                                        case "base peak m/z":
                                            basePeakMz = System.Convert.ToDouble(cvParm.value.Replace("\"", ""));
                                            break;
                                        case "base peak intensity":
                                            basePeakIntensity = System.Convert.ToDouble(cvParm.value.Replace("\"", ""));
                                            break;
                                        case "total ion current":
                                            totIonCurrent = System.Convert.ToDouble(cvParm.value.Replace("\"", ""));
                                            break;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        // if there is no referenceableParamGroupRef get parameters from spectrum's cvParam 
                        foreach (var cvParm in mzML.run.spectrumList.spectrum[i].cvParam)
                         {
                             switch (cvParm.name)
                             {
                                 case "ms level":
                                     msLevel = System.Convert.ToInt32(cvParm.value.Replace("\"", ""));
                                     break;
                                 case "lowest observed m/z":
                                     lowMz = System.Convert.ToDouble(cvParm.value.Replace("\"", ""));
                                     break;
                                 case "highest observed m/z":
                                     highMz = System.Convert.ToDouble(cvParm.value.Replace("\"", ""));
                                     break;
                                 case "base peak m/z":
                                     basePeakMz = System.Convert.ToDouble(cvParm.value.Replace("\"", ""));
                                     break;
                                 case "base peak intensity":
                                     basePeakIntensity = System.Convert.ToDouble(cvParm.value.Replace("\"", ""));
                                     break;
                                 case "total ion current":
                                     totIonCurrent = System.Convert.ToDouble(cvParm.value.Replace("\"", ""));
                                     break;
                             }
                         }
                    }

                    
                    foreach (var cvParm in mzML.run.spectrumList.spectrum[i].scanList.scan[0].cvParam)
                    {
                        switch (cvParm.name)
                        {
                            case "scan start time":
                                //get retention Time
                                retentionTime = System.Convert.ToDouble(cvParm.value.Replace("\"", ""));
                                break;
                            case "filter string":
                                //get filter
                                filter = cvParm.value.Replace("\"", "");
                                break;
                        }
                    }

                    //set current MS1
                    if (msLevel == 1)
                    {
                        currentMs1ScanNum = scanNum;
                    }

                    //create a spectrum;
                    spec = new MassSpectrum(scanNum, mzML.run.spectrumList.spectrum[i].index.Replace("\"", ""), retentionTime, new List<Ion>(), 0, instType, filter, 0,false);
                    spec.MsLevel = msLevel;
                    spec.LowMz = lowMz;
                    spec.HighMz = highMz;
                    spec.BasePeakMz = basePeakMz;
                    spec.BasePeakIntensity = basePeakIntensity;
                    spec.TotIonCurrent = totIonCurrent;
                    spec.PrecursorScanNumber = currentMs1ScanNum;
                    spec.Precursors = new List<Tuple<double, int>>();

                    //set MS2's precursor
                    if (msLevel == 2)
                    {
                        spec.PrecursorScanNumber = currentMs1ScanNum;
                        //get precursor
                        foreach (var precursorData in mzML.run.spectrumList.spectrum[i].precursorList.precursor[0].selectedIonList.selectedIon[0].cvParam)
                        {
                            if (precursorData.name == "selected ion m/z")
                            {
                                precursorMz = System.Convert.ToDouble(precursorData.value.Replace("\"", ""));
                            }
                            if (precursorData.name == "charge state")
                            {
                                precursorCharge = System.Convert.ToInt32(precursorData.value.Replace("\"", ""));
                            }
                        }
                        spec.Precursors.Add(new Tuple<double, int>(precursorMz, precursorCharge));
                    }


                    // get peaks
                    foreach (var data in mzML.run.spectrumList.spectrum[i].binaryDataArrayList.binaryDataArray)
                    {
                        bool mzArray = false;
                        bool intensityArray = false;
                        bool zlibCompression = false;

                        foreach (var cvParm in data.cvParam)
                        {
                            switch (cvParm.name)
                            {
                                case "m/z array":
                                    mzArray = true;
                                    break;
                                case "intensity array":
                                    intensityArray = true;
                                    break;
                                case "zlib compression":
                                    zlibCompression = true;
                                    break;
                                case "32-bit float":
                                    shiftByte = 4;
                                    break;
                            }
                        }

                        if (mzArray)
                        {
                            if (zlibCompression)
                            {
                                //uncompression m/z
                                uncompressedMz = Ionic.Zlib.ZlibStream.UncompressBuffer(data.binary);
                            }
                            else
                            {
                                uncompressedMz = data.binary;
                            }
                        }
                        else if (intensityArray)
                        {
                            if (zlibCompression)
                            {
                                //uncompression intensity
                                uncompressedIntensity = Ionic.Zlib.ZlibStream.UncompressBuffer(data.binary);
                            }
                            else
                            {
                                uncompressedIntensity = data.binary;
                            }
                        }
                    }

                    for (int byteIdx = 0; byteIdx < uncompressedMz.Length; byteIdx += shiftByte)
                    {
                        byte[] mzBytes = new byte[8];
                        Buffer.BlockCopy(uncompressedMz, byteIdx, mzBytes, 0, shiftByte);

                        // retrieve the mz and intensity;
                        double mz = BitConverter.ToDouble(mzBytes, 0);

                        byte[] intBytes = new byte[8];
                        Buffer.BlockCopy(uncompressedIntensity, byteIdx, intBytes, 0, shiftByte);
                        double intensity = BitConverter.ToDouble(intBytes, 0);

                        spec.Peaks.Add(new Ion(mz, intensity));
                    }

                    
                    // Precursor Correct
                    if(_correctPrecMz)
                    {
                        if (spec.MsLevel == 1)
                        {
                            if (isScanMS2)
                            {
                                List<MassSpectrum> currentMS1 = new List<MassSpectrum>();
                                currentMS1.AddRange(tmpMS1);
                                currentMS1.Add(spec);

                                for (int j=0; j < NeedToCorrectMS2.Count; j++)
                                {
                                    PrecursorCorrector pc = new PrecursorCorrector();
                                   
                                    // get the precursor m/z from the scan filter;
                                    double precMz = NeedToCorrectMS2[j].Precursors.Count == 0 ? 0 : NeedToCorrectMS2[j].Precursors[0].Item1;
                                    int precZ = NeedToCorrectMS2[j].Precursors.Count == 0 ? 0 : NeedToCorrectMS2[j].Precursors[0].Item2;

                                    MassSpectrum currentMS2 = NeedToCorrectMS2[j];
                                
                                    pc.CorrectPrecursor(ref currentMS2, precMz, precZ, currentMS2.PrecursorScanNumber, currentMS1);
                                    
                                    WriteToOutFiles(currentMS2, isFirstScan);
                                    
                                    //clear variable in program
                                    if(j==(NeedToCorrectMS2.Count-1))
                                    {
                                        WriteToOutFiles(spec, isFirstScan);
                                        isScanMS2 = false;
                                        currentMS1.Clear();
                                        NeedToCorrectMS2.Clear();
                                        if (tmpMS1.Count() >= 2)
                                        {
                                            tmpMS1.RemoveAt(1);
                                        }
                                        tmpMS1.Insert(0, spec);
                                    }
                                }
                                
                            }
                            else
                            {
                                if (tmpMS1.Count() >= 2)
                                {
                                    tmpMS1.RemoveAt(1);
                                }
                                tmpMS1.Insert(0, spec);
                            }
                        }
                        else if (spec.MsLevel == 2)
                        {
                            NeedToCorrectMS2.Add(spec);
                            isScanMS2 = true;
                        }
                    }
                    else
                    {
                        WriteToOutFiles(spec, isFirstScan);
                    }

                    isFirstScan = false;
                    _spectrumProcessed++;

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
                        Console.Write(" Reading mzML File: " + _lastProgress + "%");
                        Console.SetCursorPosition(0, currentLineCursor);
                    }
                }
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
                TextFileWriter.WriteToMGF(_mgfWriter, spec, _mzmlFileName, _mzDecimalPlace, _intensityDecimalPlace, false, false);
            }
        }

        public void Close()
        {
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
