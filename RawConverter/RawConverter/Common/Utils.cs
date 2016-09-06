using Microsoft.Win32;
using RawConverter.MassSpec;
using System;
using System.Collections.Generic;
using System.Deployment.Application;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RawConverter.Common
{
    public static class Utils
    {
        [DllImport("kernel32.dll", SetLastError = false)]
        static extern bool GetProductInfo(
             int dwOSMajorVersion,
             int dwOSMinorVersion,
             int dwSpMajorVersion,
             int dwSpMinorVersion,
             out int pdwReturnedProductType);

        public const double PROTON_MASS = 1.00727826;

        public const double MASS_DIFF_C12_C13 = 1.003354826;

        public static string CleanSequence(string sequence)
        {
            return Regex.Replace(sequence, @"[\+|\d|\-|\.|(|)]", "");
        }

        public static double MassFromMZ(double mz, int charge)
        {
            return mz * Math.Abs(charge) - charge * PROTON_MASS;
        }

        public static double MZFromMass(double mass, int charge)
        {
            return (mass + charge * PROTON_MASS) / Math.Abs(charge);
        }

        /// <summary>
        /// Verify if MSFileReader (Thermo Program) is installed in pc, because the ParserRAW needs a msfilereader DLL
        /// </summary>
        /// <returns></returns>
        public static bool verifyProgramInstalled()
        {
            //Windows RegistryKey
            RegistryKey regKey = Registry.LocalMachine;
            regKey = regKey.OpenSubKey("SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall");
            if (regKey == null)
            {
                return false;
            }
            //Get key vector for each entry
            string[] keys = regKey.GetSubKeyNames();
            if (keys != null && keys.Length > 0)
            {
                //Interates key vector to try to get DisplayName
                for (int i = 0; i < keys.Length; i++)
                {
                    //Open current key
                    RegistryKey k = regKey.OpenSubKey(keys[i]);
                    try
                    {
                        //Get DisplayName
                        String appName = k.GetValue("DisplayName").ToString();

                        if (appName != null && appName.Length > 0 && appName.Contains("Thermo MSFileReader"))
                        {
                            return true;
                        }
                    }
                    catch (Exception) { }
                }
            }

            return false;
        }

        #region Converters
        
        public static void converterRAW2MSn(List<MassSpectrum> msList, string outFile, int msLvl,
            int mzDecimalPlace, int intensityDecimalPlace)
        {
            if (msList == null)
            {
                return;
            }

            StreamWriter sw = new StreamWriter(outFile);
            GenerateMSn(msList, sw, msLvl, mzDecimalPlace, intensityDecimalPlace);
        }

        /// <summary>
        /// Converter RAW file to MGF file
        /// </summary>
        /// <param name="msList"></param>
        public static void converterRAW2MGF(List<MassSpectrum> msList, string outFile)
        {
            if (msList == null)
            {
                return;
            }

            StreamWriter sw = new StreamWriter(outFile);
            GenerateMGF(msList, sw);
        }


        /// <summary>
        /// Converter MGF file to MS2 file
        /// </summary>
        /// <param name="tmsList"></param>
        //public static void convertMGF2MS2(List<MassSpectrum> msList, string outFile)
        //{
        //    if (msList == null)
        //    {
        //        return;
        //    }

        //    StreamWriter sw = new StreamWriter(outFile);
        //    generateMSn(msList, sw, 2, "");
        //}

        /// <summary>
        /// Converter MS2 file to MGF file
        /// </summary>
        /// <param name="tmsList"></param>
        public static void ConvertMS22MGF(List<MassSpectrum> msList, string outFile)
        {
            if (msList == null)
            {
                return;
            }

            StreamWriter sw = new StreamWriter(outFile);
            GenerateMGF(msList, sw);
        }

        public static MassSpectrum GetSpecByScanNum(int scanNum, string tempFileFolder)
        {
            string tempFileName = tempFileFolder + "\\" + scanNum + ".msd";
            Stream stream = File.Open(tempFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            var serializer = new BinaryFormatter();
            MassSpectrum spec = (MassSpectrum)serializer.Deserialize(stream);
            stream.Close();

            return spec;
        }

        public static void SerializeSpecByScanNum(MassSpectrum spec, string tempFileFolder)
        {
            string tempFileName = tempFileFolder + "\\" + spec.ScanNumber + ".msd";
            Stream stream = File.Open(tempFileName, FileMode.Create);
            var serializer = new BinaryFormatter();
            serializer.Serialize(stream, spec);
            stream.Close();
        }
        
        private static void GenerateMSn(List<MassSpectrum> msList, StreamWriter sw, int msLvl,
            int mzDecimalPlace, int intensityDecimalPlace)
        {
            if (msList == null)
            {
                return;
            }

            Console.WriteLine(" Writing MS" + msLvl + " File...");

            int firstScan = msList[0].ScanNumber;
            int lastScan = msList[msList.Count() - 1].ScanNumber;

            // get the program version;
            string version = "-";
            try
            {
                string[] attributes = Regex.Split(AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationContext.Identity.FullName, ",");
                version = Regex.Split(attributes[1], "=")[1];
            }
            catch (Exception e)
            {
                // unable to retrieve version number;
                Console.WriteLine("", e);
                version = "-";
            }

            sw.Write("H\tCreation Date\t" + DateTime.Now.ToString() + "\n"
                + "H\tExtractor\tRAWXtract+\n"
                + "H\tExtractorVersion\t0.1\n"
                + "H\tComments\tRawXtract+ written by Lin He, 2014\n"
                + "H\tExtractorOptions\tMS2\n"
                + "H\tAcquisitionMethod\tData-Dependent\n"
                + "H\tInstrumentType\tFTMS\n"
                + "H\tScanType\tMS2\n"
                + "H\tDataType\tCentroid\n"
                + "H\tResolution\n"
                + "H\tIsolationWindow\n"
                + "H\tFirstScan\t" + msList[0].ScanNumber + "\n"
                + "H\tLastScan\t" + msList[msList.Count - 1].ScanNumber + "\n");

            foreach (MassSpectrum ms in msList)
            {
                if (ms.MsLevel != msLvl)
                {
                    continue;
                }
                
                if (msLvl == 1)
                {
                    sw.Write("S\t" + String.Format("{0:000000}", ms.ScanNumber) + "\t" + String.Format("{0:000000}", ms.ScanNumber) + "\n"
                        + "I\tRetTime\t" + ms.RetentionTime + "\n"
                        + "I\tIonInjectionTime\t" + ms.IonInjectionTime + "\n"
                        + "I\tInstrumentType\t" + ms.InstrumentType + "\n"
                        );
                }
                else
                {
                    sw.Write("S\t" + String.Format("{0:000000}", ms.ScanNumber) + "\t" + String.Format("{0:000000}", ms.ScanNumber) + "\t"
                        + Math.Round(ms.Precursors[0].Item1, mzDecimalPlace + 1) + "\n"
                        + "I\tRetTime\t" + Math.Round(ms.RetentionTime, 2) + "\n"
                        + "I\tIonInjectionTime\t" + ms.IonInjectionTime + "\n"
                        + "I\tActivationType\t" + ms.ActivationMethod + "\n"
                        + "I\tInstrumentType\t" + ms.InstrumentType + "\n"
                        + "I\tTemperatureFTAnalyzer\t" + ms.TemperatureFTAnalyzer + "\n"
                        + "I\tFilter\t" + ms.Filter + "\n"
                        + "I\tPrecursorScan\t" + ms.PrecursorScanNumber + "\n"
                        + "I\tPrecursorInt\t"
                        );
                    if (!double.IsNaN(ms.PrecursorIntensity))
                    {
                        sw.Write(Math.Round(ms.PrecursorIntensity, intensityDecimalPlace) + "\n");
                    }
                    else
                    {
                        sw.Write("0\n");
                    }
                    foreach (Tuple<double, int> prec in ms.Precursors)
                    {
                        double precMH = (prec.Item1 - PROTON_MASS) * prec.Item2 + PROTON_MASS;
                        sw.WriteLine("Z\t" + prec.Item2 + "\t" + Math.Round(precMH, mzDecimalPlace + 1));
                    }
                }

                foreach (Ion peak in ms.Peaks)
                {
                    sw.WriteLine(Math.Round(peak.MZ, mzDecimalPlace) + " "
                        + Math.Round(peak.Intensity, intensityDecimalPlace));
                }
            }

            Console.WriteLine(" Completed . . .");
            sw.Close();
        }

        public static MassSpectrum GetPrecSpec(List<MassSpectrum> msList, int curMsIdx)
        {
            int curScanNum = msList[curMsIdx].ScanNumber;
            int precScanNum = msList[curMsIdx].PrecursorScanNumber;
            int precSpecIdx = curMsIdx - 1;
            if (precScanNum == 0)
            {
                while (precSpecIdx >= 0 && msList[precSpecIdx].MsLevel != 1)
                {
                    precSpecIdx--;
                }
                if (precSpecIdx < 0)
                {
                    Console.WriteLine("Cannot find the precursor MS1 scan for the MS2 spectrum " + curScanNum);
                    return null;
                }
            }
            else
            {
                while (precSpecIdx >= 0 && msList[precSpecIdx].ScanNumber != precScanNum)
                {
                    precSpecIdx--;
                }
                if (precSpecIdx < 0)
                {
                    Console.WriteLine("Cannot find the precursor MS1 scan for the MS2 spectrum " + curScanNum);
                    return null;
                }
            }

            return msList[precSpecIdx];
        }
        
        /// <summary>
        /// Generate an MGF file using the given spectrum list;
        /// </summary>
        /// <param name="msList"></param>
        private static void GenerateMGF(List<MassSpectrum> msList, StreamWriter sw)
        {
            if (msList == null)
            {
                return;
            }

            Console.WriteLine(" Writing MGF File . . .");

            // get the program version;
            string version = "-";
            try
            {
                string[] attributes = Regex.Split(AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationContext.Identity.FullName, ",");
                version = Regex.Split(attributes[1], "=")[1];
            }
            catch (Exception e)
            {
                // Unable to retrieve version number
                Console.WriteLine("", e);
                version = "-";
            }

            sw.Write("# Creation Date\t" + DateTime.Now.ToString() + "\n"
               + "# Extractor\tRawXtract\n"
               + "# Version\t" + version + "\n"
               + "# Comments\t This converter was developed by Lin He, 2014\n\n"
               );

            foreach (MassSpectrum ms in msList)
            {
                if (ms.MsLevel == 1)
                {
                    continue;
                }
                sw.WriteLine("BEGIN IONS");
                sw.WriteLine("TITLE=Scans:" + ms.ScanNumber + ", Fragmentation: " + ms.ActivationMethod + ", TemperatureAnalyzer:" + ms.TemperatureFTAnalyzer);
                sw.WriteLine("SCANS=" + ms.ScanNumber);
                sw.WriteLine("RTINSECONDS=" + ms.RetentionTime);
                sw.WriteLine("CHARGE=" + ms.Precursors[0].Item2 + "+");
                sw.WriteLine("PEPMASS=" + ms.Precursors[0].Item1);

                foreach (Ion ion in ms.Peaks)
                {
                    sw.WriteLine(ion.MZ + " " + ion.Intensity);
                }

                sw.WriteLine("END IONS");
            }

            Console.WriteLine(" Completed . . .");
            sw.Close();
        }
        #endregion

        #region MD5 Encoding
        public static string GetMd5Hash(MD5 md5Hash, string input)
        {

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        // Verify a hash against a string.
        public static bool VerifyMd5Hash(MD5 md5Hash, string input, string hash)
        {
            // Hash the input.
            string hashOfInput = GetMd5Hash(md5Hash, input);

            // Create a StringComparer an compare the hashes.
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            if (0 == comparer.Compare(hashOfInput, hash))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        public static void checkForUpdates()
        {
            ApplicationDeployment updateCheck = ApplicationDeployment.CurrentDeployment;
            UpdateCheckInfo info;

            try
            {
                info = updateCheck.CheckForDetailedUpdate();

                if (info.UpdateAvailable)
                {
                    DialogResult dialogResult = MessageBox.Show("An update is available. Would you like to update the application now?", "Update available", MessageBoxButtons.OKCancel);

                    if (dialogResult == DialogResult.OK)
                    {
                        updateCheck.Update();
                        MessageBox.Show("The application has been upgraded, and will now restart.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        Application.Restart();
                    }

                }
                else
                {
                    MessageBox.Show("No updates are available for the moment.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (DeploymentDownloadException dde)
            {
                MessageBox.Show(dde.Message + "\n" + dde.InnerException);
                return;
            }
            catch (InvalidDeploymentException ide)
            {
                MessageBox.Show(ide.Message + "\n" + ide.InnerException);
                return;
            }
            catch (InvalidOperationException ioe)
            {
                MessageBox.Show(ioe.Message + "\n" + ioe.InnerException);
                return;
            }
            catch (Exception e10)
            {
                MessageBox.Show(e10.Message);
                return;
            }
        }

        public enum WindowsVersion
        {
            None = 0,
            Windows_1_01,
            Windows_2_03,
            Windows_2_10,
            Windows_2_11,
            Windows_3_0,
            Windows_for_Workgroups_3_1,
            Windows_for_Workgroups_3_11,
            Windows_3_2,
            Windows_NT_3_5,
            Windows_NT_3_51,
            Windows_95,
            Windows_NT_4_0,
            Windows_98,
            Windows_98_SE,
            Windows_2000,
            Windows_Me,
            Windows_XP,
            Windows_Server_2003,
            Windows_Vista,
            Windows_Home_Server,
            Windows_7,
            Windows_2008_R2,
            Windows_8,
        }

        public static string Is64Bits()
        {
            if (Environment.Is64BitOperatingSystem)
            {
                return "64 bits";
            }
            else
            {
                return "32 bits";
            }
        }

        public static string GetWindowsVersion()
        {
            string platform = System.Environment.OSVersion.Platform.ToString();
            if (platform.ToLower().Contains("nt"))
            {
                platform = Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ProductName", null).ToString();
            }
            else
            {
                platform = Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion", "ProductName", null).ToString();
            }
            return platform;
        }
    }

    public class TaskProgress
    {
        public int CurrentProgress { get; set; }
        public bool Aborted { get; set; }

        public TaskProgress()
        {
            CurrentProgress = 0;
            Aborted = false;
        }

        public void Reset() 
        {
            CurrentProgress = 0;
            Aborted = false;
        }
    }
}
