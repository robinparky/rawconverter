using RawConverter.MassSpec;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RawConverter.Converter
{
    class MzXMLFileWriter
    {
        private PositionableStreamWriter _writer;
        private List<Tuple<int, long>> _scanIdxList;
        private String _mzXMLFile;

        public MzXMLFileWriter(String mzXMLFile)
        {
            _mzXMLFile = mzXMLFile;
            _writer = new PositionableStreamWriter(mzXMLFile, false, Encoding.GetEncoding("ISO-8859-1"));
            _scanIdxList = new List<Tuple<int, long>>();
        }

        public void WriteHeader(int scanCount, double startTimeInSecond, double endTimeInSecond, String rawFileName, 
            String manufacturer, String msModel, String ionIsolationMethod, String massAnalyzer, String detector, String softwareType, String softwareName, 
            String softwareVersion, String rawConverterVersion) 
        {
            _writer.Write("<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>\n");
            _writer.Write("<mzXML xmlns = \"http://sashimi.sourceforge.net/schema_revision/mzXML_2.0\" xmlns:xsi = \"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://sashimi.sourceforge.net/schema_revision/mzXML_2.0 http://sashimi.sourceforge.net/schema_revision/mzXML_2.0/mzXML_idx_2.0.xsd\">\n");

            _writer.Write("\t<msRun scanCount=\"" + scanCount + "\"" + " startTime=\"PT" + startTimeInSecond + "S\" endTime=\"PT" + endTimeInSecond + "S\">\n");
            
            // add parentFile element to MSRun;
            String fileType = "RAWData";
            String fileSha1 = CalcFileSha1(rawFileName);
            _writer.Write("\t<parentFile fileName=\"" + rawFileName + "\" fileType=\"" + fileType + "\" fileSha1=\"" + fileSha1 + "\"/>\n");

            // add msInstrument to MSRun node;
            _writer.Write("\t<msInstrument>\n");
            //add MSInstrument child nodes;
            _writer.Write("\t\t<msManufacturer category=\"msManufacturer\" value=\"" + manufacturer + "\"/>\n");
            _writer.Write("\t\t<msModel category=\"msModel\" value=\"" + msModel + "\"/>\n");
            _writer.Write("\t\t<msIonisation category=\"msIonisation\" value=\"" + ionIsolationMethod + "\"/>\n");
            _writer.Write("\t\t<msMassAnalyzer category=\"msMassAnalyzer\" value=\"" + massAnalyzer + "\"/>\n");
            _writer.Write("\t\t<msDetector category=\"msDetector\" value=\"" + detector + "\"/>\n");
            _writer.Write("\t\t<software type=\"" + softwareType + "\" name=\"" + softwareName + "\" version=\"" + softwareVersion + "\"/>\n");
            _writer.Write("\t</msInstrument>\n");

            // add dataProcessing to MSRun node;
            _writer.Write("\t<dataProcessing>\n");
            _writer.Write("\t\t<software type=\"conversion\" name=\"" + "RawConverter\" version=\"" + rawConverterVersion + "\"/>\n");
            _writer.Write("\t</dataProcessing>\n");
 
            _writer.Flush();
        }

        public long WriteScan(MassSpectrum spec)
        {
            // record the start position for later using of index;
            long startPos = _writer.Position + 1;
            _scanIdxList.Add(new Tuple<int, long>(spec.ScanNumber, startPos));

            _writer.Write("\t<scan num=\"" + spec.ScanNumber + "\"");
            _writer.Write(" msLevel=\"" + spec.MsLevel + "\"");
            _writer.Write(" peaksCount=\"" + spec.Peaks.Count + "\"");
            if (spec.Filter.Contains("+"))
            {
                _writer.Write(" polarity=\"+\"");
            }
            else if (spec.Filter.Contains("-"))
            {
                _writer.Write(" polarity=\"-\"");
            }
            _writer.Write(" scanType=\"" + spec.ActivationMethod + "\"");
            _writer.Write(" filterLine=\"" + spec.Filter + "\"");
            _writer.Write(" retentionTime=\"PT" + spec.RetentionTime * 60 + "S\"");

            _writer.Write(" lowMz=\"" + (spec.Peaks.Count > 0 ? spec.Peaks.First().MZ : spec.LowMz) + "\"");
            _writer.Write(" highMz=\"" + (spec.Peaks.Count > 0 ? spec.Peaks.Last().MZ : spec.HighMz) + "\"");
            _writer.Write(" basePeakMz=\"" + spec.BasePeakMz +"\"");
            _writer.Write(" basePeakIntensity=\"" + spec.BasePeakIntensity +"\"");
            _writer.Write(" totIonCurrent=\"" + spec.TotIonCurrent +"\">\n");

            if (spec.MsLevel > 1)
            {
                _writer.Write("\t\t<precursorMz precursorScanNum=\"" + spec.PrecursorScanNumber + "\"");
                _writer.Write(" precursorIntensity=\"" + spec.PrecursorIntensity + "\"");
                _writer.Write(" activationMethod=\"" + spec.ActivationMethod + "\"");
                _writer.Write(" precursorCharge=\"" + spec.Precursors[0].Item2 + "\">");
                _writer.Write(spec.Precursors[0].Item1 + "</precursorMz>\n");
                //foreach (Tuple<double, int> prec in spec.Precursors)
                //{
                //    _writer.Write(" precursorMz =\"" + prec.Item2 + "\">" + prec.Item1 + "</precursorMz>\n");
                //}
            }
            _writer.Write("\t\t<peaks precision=\"32\" byteOrder=\"network\" pairOrder=\"m/z-int\">");
            String strBase64 = ConvertPeaksToBase64(spec.Peaks, 32);
            _writer.Write(strBase64);
            _writer.Write("</peaks>\n\t</scan>\n");
            _writer.Flush();

            return startPos;
        }

        public void WriteIndex()
        {
            _writer.Write("\t</msRun>\n");
            long startPos = _writer.Position + 1;

            // write scan indices;
            _writer.Write("\t<index name=\"scan\">\n");
            foreach (Tuple<int, long> scanIdx in _scanIdxList)
            {
                _writer.Write("\t\t<offset id=\"" + scanIdx.Item1 + "\">" + scanIdx.Item2 + "</offset>\n");
            }
            _writer.Write("\t</index>\n");

            // write index offset;
            _writer.Write("\t<indexOffset>" + startPos + "</indexOffset>\n");
        }

        public void WriteEnd()
        {
            _writer.Write("\t<sha1>");
            _writer.Flush();
            String sha1 = CalcFileSha1(_mzXMLFile);
            _writer.Write(sha1 + "</sha1>\n");
            _writer.Write("</mzXML>\n");
            _writer.Flush();
        }

        private String CalcFileSha1(String filename)
        {
            Console.WriteLine(" Calculating SHA1 for file " + filename + "...");
            FileStream stream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            SHA1 sha = new SHA1CryptoServiceProvider();
            byte[] hash = sha.ComputeHash(stream);
            stream.Close();
            return BitConverter.ToString(hash).Replace("-", String.Empty);         
        }

        public void Close()
        {
            _writer.Close();
        }

        private String ConvertPeaksToBase64(List<Ion> peaks, int precision)
        {
            String ret = null;
            byte[] byteData = null;

            int len = peaks.Count;
            if (precision == 32)
            {
                byteData = new byte[8 * len];
                for (int peakIdx = 0; peakIdx < len; peakIdx++)
                {
                    for (int idx = 0; idx < 2; idx++)
                    {
                        float value = 0.0f;
                        if (idx % 2 == 0)
                        {
                            value = Convert.ToSingle(peaks[peakIdx].MZ);
                        }
                        else
                        {
                            value = Convert.ToSingle(peaks[peakIdx].Intensity);
                        }
                        byte[] bytes = BitConverter.GetBytes(value);
                        byte[] revBytes = new byte[bytes.Length];
                        for (int i = 0; i < revBytes.Length; i++)
                        {
                            revBytes[i] = bytes[bytes.Length - i - 1];
                        }
                        Buffer.BlockCopy(revBytes, 0, byteData, peakIdx * 8 + idx * 4, 4);
                    }
                }
            }
            else if (precision == 64)
            {
                byteData = new byte[16 * len];
                for (int peakIdx = 0; peakIdx < len; peakIdx++)
                {
                    for (int idx = 0; idx < 2; idx++)
                    {
                        double value = 0.0;
                        if (idx % 2 == 0)
                        {
                            value = peaks[peakIdx].MZ;
                        }
                        else
                        {
                            value = peaks[peakIdx].Intensity;
                        }
                        byte[] bytes = BitConverter.GetBytes(value);
                        byte[] revBytes = new byte[bytes.Length];
                        for (int i = 0; i < revBytes.Length; i++)
                        {
                            revBytes[i] = bytes[bytes.Length - i - 1];
                        }
                        Buffer.BlockCopy(bytes, 0, byteData, peakIdx * 16 + idx * 8, 8);
                    }
                }
            }

            try {
                ret = System.Convert.ToBase64String(byteData, 0, byteData.Length);
            }
            catch (System.ArgumentNullException) {
                System.Console.WriteLine("Binary data array is null.");
                return null;
            }

            return ret;
        }

    }
}
