using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Threading;
using System.Globalization;

using RawConverter.GUI;
using RawConverter.Common;
using RawConverter.MassSpec;
using System.IO;
using RawConverter.DDADataProcess;
using RawConverter.DIADataProcess;
using System.Runtime.Serialization.Formatters.Binary;
using RawConverter.Converter;

namespace RawConverter
{
    class RawConverter
    {
        public List<string> inputFiles { get; set; }
        public bool ms1Converter { get; set; }
        public bool ms2Converter { get; set; }
        public bool ms3Converter { get; set; }
        public bool mgfConverter { get; set; }
        public bool mzXMLConverter { get; set; }
        public bool mzMLConverter { get; set; }
        public bool logConverter { get; set; }
        public int terminateCode { get; set; }
        public int countFile { get; set; }
        public double totalFiles { get; set; }
        public List<string> LogList { get; set; }
        public bool errorFiles { get; set; }

        public string OutFileFolder { get; set; }

        public ExperimentType ExpType { get; set; }
        private bool isDDA = false;
        private bool isDIA = false;
        public bool correctPrecMz { get; set; }
        public bool correctPrecZ { get; set; }
        public bool predictPrecursors { get; set; }

        public bool isCentroided { get; set; }

        public static CultureInfo curCultInfo;

        public int SpectraProcessed { get; set; }
        public int LastProgress { get; set; }
        public TaskProgress ExtractProgress { get; set; }
        public string CurrentFileLabel { get; set; }

        public int MzDecimalPlace { get; set; }
        public int IntensityDecimalPlace { get; set; }
        public HashSet<int> DDADataChargeStates { get; set; }

        public bool UsingTempFiles { get; set; }
        public string TempFileFolder { get; set; }

        public int[] Ms2PrecZ { get; set; }

        public bool ExtractPrecursorByMz { get; set; }
        public bool ByPassThermoAlgorithm { get; set; }

        private string[] outFileTypes = null;

        public bool showPeakChargeStates { get; set; }
        public bool showPeakResolution { get; set; }
        public bool exportChargeState { get; set; }

        public RawConverter()
        {
            // initialize the log list;
            LogList = new List<string>();

            // initialize the predefined charge state set for DDA data;
            // charge 2+ and 3+ are the default ones;
            DDADataChargeStates = new HashSet<int>();
            DDADataChargeStates.Add(2);
            DDADataChargeStates.Add(3);

            // initialize the temporary file folder;
            UsingTempFiles = false;
            TempFileFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\temp_msd";
            Directory.CreateDirectory(TempFileFolder);

            // initialize the terminating code;
            terminateCode = 1; // 1: not finished yet; 0: finished successfully; -1: finished unsuccessfully.

            isCentroided = true;

            // initialze the decimal places;
            MzDecimalPlace = 4;
            IntensityDecimalPlace = 1;

            ExtractProgress = new TaskProgress();

            CurrentFileLabel = "File 0 / 0";

            showPeakChargeStates = true;
            showPeakResolution = true;
            exportChargeState = false;
        }

        /// <summary>
        /// Main process of RawXtract.
        /// </summary>
        /// <param name="args"></param>
        [STAThread]
        public static void Main(String[] args)
        {
            if (args.Count() == 0)
            {
                //InitializeProgram();
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new RawConverterGUI());
            }
            else
            {
                if (args[0].ToLower().Equals("-h") || args[0].ToLower().Equals("--help"))
                {
                    Usage();
                    return;
                }

                string inFile = args[0];
                List<string> inputFiles = new List<string>();
                inputFiles.Add(inFile);
                RawConverter rawXtract = new RawConverter();
                rawXtract.inputFiles = inputFiles;

                bool hasExpType = false;

                for (int i = 1; i < args.Count(); i++)
                {
                    if (args[i].ToLower().Contains("ms1"))
                    {
                        rawXtract.ms1Converter = true;
                    }
                    else if (args[i].ToLower().Contains("ms2"))
                    {
                        rawXtract.ms2Converter = true;
                    }
                    else if (args[i].ToLower().Contains("ms3"))
                    {
                        rawXtract.ms3Converter = true;
                    }
                    else if (args[i].ToLower().Contains("mgf"))
                    {
                        rawXtract.mgfConverter = true;
                    }
                    else if (args[i].ToLower().Contains("log"))
                    {
                        rawXtract.logConverter = true;
                    }
                    else if (args[i].ToLower().Contains("mzxml"))
                    {
                        rawXtract.mzXMLConverter = true;
                    }
                    else if (args[i].ToLower().Contains("out_folder"))
                    {
                        if (++i >= args.Count())
                        {
                            Usage();
                            return;
                        }
                        rawXtract.OutFileFolder = args[i];
                    }
                    else if (args[i].ToLower().Contains("select_mono_prec"))
                    {
                        if (!hasExpType)
                        {
                            hasExpType = true;
                            rawXtract.isDDA = true;
                            rawXtract.isDIA = false;
                            rawXtract.ExpType = ExperimentType.DDA;
                            rawXtract.correctPrecMz = true;
                            rawXtract.predictPrecursors = false;
                        }
                        else
                        {
                            Console.WriteLine("--select_mono_prec and --predict_precursors are two mutex options, you can only select one of them!");
                            return;
                        }
                    }
                    else if (args[i].ToLower().Contains("predict_precursors"))
                    {
                        if (!hasExpType)
                        {
                            hasExpType = true;
                            rawXtract.isDDA = false;
                            rawXtract.isDIA = true;
                            rawXtract.ExpType = ExperimentType.DIA;
                            rawXtract.correctPrecMz = false;
                            rawXtract.predictPrecursors = true;
                        }
                        else
                        {
                            Console.WriteLine("--select_mono_prec and --predict_precursors are two mutex options, you can only select one of them!");
                            return;
                        }
                    }
                    
                }

                // check the output folder;
                if (rawXtract.OutFileFolder == null)
                {
                    // set as the current folder;
                    rawXtract.OutFileFolder = ".";
                }

                //InitializeProgram();
                rawXtract.ConvertFiles();
                if (rawXtract.terminateCode < 0)
                {
                    Console.WriteLine(" Converting completed unsuccessfully! Please check your data or contact the developer.");
                }
            }
        }

        /// <summary>
        /// Print the usage of this program.
        /// </summary>
        public static void Usage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("\tRawConverter.exe <input_file> [options]\trun the commandline version.");
            Console.WriteLine("\tOptions:");
            Console.WriteLine("\t\t--ms1\toutput MS1 file.");
            Console.WriteLine("\t\t--ms2\toutput MS2 file.");
            Console.WriteLine("\t\t--mzxml\toutput mzXML file.");
            Console.WriteLine("\t\t--ms3\toutput MS3 file.");
            Console.WriteLine("\t\t--mgf\toutput MGF file.");
            Console.WriteLine("\t\t--log\toutput log file.");
            //Console.WriteLine("\t\t--out_folder\toutput folder.");
            Console.WriteLine("\t\t--select_mono_prec\tselect the monoisotopic m/z values of precursors in DDA data.");
            Console.WriteLine("\t\t--predict_precursors\tpredict the precursors for DIA data.");
        }

        /// <summary>
        /// Convert files in the inputFiles;
        /// </summary>
        public void ConvertFiles()
        {
            // generate the out file type array;
            List<string> outFileTypeList = new List<string>();
            if (ms1Converter)
            {
                outFileTypeList.Add("ms1");
            }
            if (ms2Converter)
            {
                outFileTypeList.Add("ms2");
            }
            if (ms3Converter)
            {
                outFileTypeList.Add("ms3");
            }
            if (mgfConverter)
            {
                outFileTypeList.Add("mgf");
            }
            if (mzXMLConverter)
            {
                outFileTypeList.Add("mzXML");
            }
            if (mzMLConverter)
            {
                outFileTypeList.Add("mzML");
            }
            outFileTypes = new string[outFileTypeList.Count];
            outFileTypeList.CopyTo(outFileTypes);

            // convert files one by one;
            for (int idx = 0; idx < inputFiles.Count; idx++)
            {
                ConvertFile(inputFiles[idx], idx);
                if (ExtractProgress.CurrentProgress < 100)
                {
                    terminateCode = -2;
                    break;
                }
            }
            if (terminateCode > 0)
            {
                terminateCode = 0;
            } 

        }

        /// <summary>
        /// Reset the terminating code to 1, which indicates an un-finished status for next-time use.
        /// </summary>
        public void resetTerminateCode()
        {
            terminateCode = 1;
        }

        /// <summary>
        /// Convert a input file into a output file with a user specified format.
        /// </summary>
        /// </summary>
        /// <param name="inFile"></param>
        /// <param name="fileIdx"></param>
        public void ConvertFile(string inFile, int fileIdx)
        {
            LogList.Clear();
            ExtractProgress.Reset();

            // check whether MsFileReaderLib is installed if the input file is a RAW file;
            if (inFile.ToLower().EndsWith(".raw"))
            {
                //if (!Utils.verifyProgramInstalled())
                //{
                //    Console.WriteLine("\nERROR: Not found the necessary program to read RAW files ! Please verify that the program is installed in your computer.\n");
                //    LogList.Add("ERROR: Not found the necessary program to read Raw files ! Please verify that the program is installed in your computer.\n");
                //    errorFiles = true;
                //    return;
                //}
            }

            int totalFileNum = inputFiles.Count;
            CurrentFileLabel = "File " + (fileIdx + 1) + " / " + totalFileNum;
            Console.WriteLine(" Starting to convert file " + (fileIdx + 1) + " / " + totalFileNum + "...");
            LogList.Add(" Starting to convert file " + (fileIdx + 1) + " / " + totalFileNum + "...");

            if (inFile.EndsWith(".ms2", true, curCultInfo) && mgfConverter)
            {
                Console.WriteLine(" Parsing MS2 file: " + inFile + " . . . ");
                LogList.Add(" Parsing MS2 file: " + inFile + " . . . ");
                MS2Converter mc = new MS2Converter(inFile, OutFileFolder, outFileTypes, correctPrecMz);
                mc.SetOptions(MzDecimalPlace, IntensityDecimalPlace);
                mc.Convert(ExtractProgress);
                mc.Close();
            }
            else if (inFile.EndsWith(".mgf", true, curCultInfo) && ms2Converter)
            {
                Console.WriteLine(" Parsing MGF file: " + inFile + " . . . ");
                LogList.Add(" Parsing MGF file: " + inFile + " . . . ");
                MgfConverter mc = new MgfConverter(inFile, OutFileFolder, outFileTypes[0]);
                mc.SetOptions(MzDecimalPlace, IntensityDecimalPlace, DDADataChargeStates);
                mc.Convert(ExtractProgress);
                LogList.Add(" Conversion finished");
            }
            else if (inFile.EndsWith(".raw", true, curCultInfo))
            {
                Console.WriteLine(" Parsing RAW file: " + inFile + " . . . ");
                LogList.Add(" Parsing RAW file: " + inFile + " . . . ");
                ByPassThermoAlgorithm = true;
                RawFileConverter rc = new RawFileConverter(inFile, OutFileFolder, outFileTypes, ExpType, exportChargeState);
                rc.SetOptions(isCentroided, MzDecimalPlace, IntensityDecimalPlace, ExtractPrecursorByMz, ByPassThermoAlgorithm, 
                    correctPrecMz, correctPrecZ, predictPrecursors, DDADataChargeStates, Ms2PrecZ, 
                    showPeakChargeStates, showPeakResolution, exportChargeState);
                rc.Convert(ExtractProgress);
                rc.Close();
            }
            else if (inFile.EndsWith(".mzxml", true, curCultInfo))
            {
                Console.WriteLine(" Parsing mzXML file: " + inFile + " . . . ");
                LogList.Add(" Parsing mzXML file: " + inFile + " . . . ");

                MzXMLConverter mc = new MzXMLConverter(inFile, OutFileFolder, outFileTypes, ExpType);
                mc.SetOptions(MzDecimalPlace, IntensityDecimalPlace);
                mc.Convert(ExtractProgress);
                mc.Close();
            }
            else if (inFile.EndsWith(".mzml", true, curCultInfo))
            {
                Console.WriteLine(" Parsing mzML file: " + inFile + " . . . ");
                LogList.Add(" Parsing mzML file: " + inFile + " . . . ");

                MzMLConverter mc = new MzMLConverter(inFile, OutFileFolder, outFileTypes);
                mc.SetOptions(MzDecimalPlace, IntensityDecimalPlace, correctPrecMz);
                mc.Convert(ExtractProgress);
                mc.Close();
            }
            //ExtractProgress.CurrentProgress = 100;
            LogList.Add("  \n");
        }
        
        /// <summary>
        /// Method responsible for setting main parameters
        /// </summary>
        public static void InitializeProgram()
        {
            #region Setting Language
            curCultInfo = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = curCultInfo;

            string platform = System.Environment.OSVersion.Platform.ToString();
            if (platform.Contains("Win"))
            {
                if (!Microsoft.Win32.Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\International", "LocaleName", null).ToString().ToLower().Equals("en-us"))
                {
                    DialogResult answer = MessageBox.Show("The Default Language does not English. Do you want to change it to English ?\nThis tool works if only the default language is English.", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (answer == DialogResult.Yes)
                    {
                        Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "Locale", "00000409");
                        Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "LocaleName", "en-US");
                        Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "sCountry", "Estados Unidos");
                        Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "sCurrency", "$");
                        Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "sDate", "/");
                        Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "sDecimal", ".");
                        Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "sGrouping", "3;0");
                        Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "sLanguage", "ENU");
                        Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "sList", ",");
                        Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "sLongDate", "dddd, MMMM dd, yyyy");
                        Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "sMonDecimalSep", ".");
                        Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "sMonGrouping", "3;0");
                        Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "sMonThousandSep", ",");
                        Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "sNativeDigits", "0123456789");
                        Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "sNegativeSign", "-");
                        Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "sPositiveSign", "");
                        Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "sShortDate", "M/d/yyyy");
                        Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "sThousand", ",");
                        Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "sTime", ":");
                        Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "sTimeFormat", "h:mm:ss tt");
                        Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "sShortTime", "h:mm tt");
                        Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "sYearMonth", "MMMM, yyyy");
                        Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "iCalendarType", "1");
                        Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "iCountry", "1");
                        Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "iCurrDigits", "2");
                        Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "iCurrency", "0");
                        Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "iDate", "0");
                        Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "iDigits", "2");
                        Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "NumShape", "1");
                        Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "iFirstDayOfWeek", "6");
                        Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "iFirstWeekOfYear", "0");
                        Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "iLZero", "1");
                        Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "iMeasure", "1");
                        Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "iNegCurr", "0");
                        Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "iNegNumber", "1");
                        Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "iPaperSize", "1");
                        Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "iTime", "0");
                        Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "iTimePrefix", "0");
                        Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\International", "iTLZero", "0");
                        MessageBox.Show("MultiXtract will be restarted!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        System.Environment.Exit(0);
                        System.Windows.Forms.Application.Exit();
                    }
                    else
                    {
                        MessageBox.Show("RawXtract will be closed!", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        System.Environment.Exit(0);
                        System.Windows.Forms.Application.Exit();
                    }
                }
            }
            #endregion

            string version = "?";
            try
            {
                string[] stuff = Regex.Split(AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationContext.Identity.FullName, ",");
                version = stuff[1];
            }
            catch (Exception e)
            {
                //Unable to retrieve version number
                Console.WriteLine("", e);
            }

            Console.WriteLine("#################################################################");
            Console.WriteLine("#                   RawConveter v1.0.0.x                        #");
            Console.WriteLine("#            Developed and maintained by Lin He                 #");
            Console.WriteLine("#################################################################\n");

        }
    }
}
