using RawConverter.Common;
using RawConverter.MassSpec;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Globalization;

namespace RawConverter.Converter
{
    class TextFileWriter
    {

        public static void WriteMSnHeader(StreamWriter writer, String msType, int scanCount, MassSpectrum spec)
        {
            // MSn header format;
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            string msnHeader = "H\tCreation Date\t" + DateTime.Now.ToString() + "\n"
                + "H\tExtractor\tRawConverter\n"
                + "H\tExtractorVersion\t1.0.0.1\n"
                + "H\tComments\tRawConverter written by Lin He, 2014\n"
                + "H\tComments\tRawConverter modified by Yen-Yin Chu, 2015\n"
                + "H\tComments\tRawConverter modified by Rohan Rampuria, 2016\n"
                + "H\tExtractorOptions\tMSn\n"
                + "H\tAcquisitionMethod\tData-Dependent\n"
                + "H\tInstrumentType\t" + spec.InstrumentType + "\n"
                + "H\tDataType\tCentroid\n"
                + "H\tScanType\t" + msType + "\n"
                + "H\tResolution\n"
                + "H\tIsolationWindow\n"
                + "H\tFirstScan\t" + 1 + "\n"
                + "H\tLastScan\t" + scanCount + "\n";
            writer.Write(msnHeader);
        }

        public static void WriteToMS1(StreamWriter writer, MassSpectrum spec, int mzDecimalPlace, 
            int intensityDecimalPlace, bool showPeakChargeState, bool showPeakResolution)
        {
            writer.Write("S\t" + String.Format("{0:000000}", spec.ScanNumber) + "\t"
                + String.Format("{0:000000}", spec.ScanNumber) + "\n"
                + "I\tRetTime\t" + spec.RetentionTime + "\n"
                + "I\tIonInjectionTime\t" + spec.IonInjectionTime + "\n"
                + "I\tInstrumentType\t" + spec.InstrumentType + "\n"
                );

            foreach (Ion peak in spec.Peaks)
            {
                writer.Write(Math.Round(peak.MZ, mzDecimalPlace) + " "
                    + Math.Round(peak.Intensity, intensityDecimalPlace));
                if (showPeakChargeState && spec.InstrumentType == InstrumentType.FTMS)
                {
                    writer.Write(" " + peak.Charge);
                }
                if (showPeakResolution && spec.InstrumentType == InstrumentType.FTMS)
                {
                    writer.Write(" " + Math.Round(peak.Resolution, 2));
                }
                writer.Write("\n");
            }
            writer.Flush();
        }

        /** 
         * write to MSn file without duplicates.
         */
        public static void WriteToMSn(StreamWriter writer, MassSpectrum spec, int mzDecimalPlace,
           int intensityDecimalPlace, bool showPeakChargeState, bool showPeakResolution)
        {
            if (spec.Precursors == null || spec.Precursors.Count == 0)
            {
                return;
            }

            writer.Write("S\t" + String.Format("{0:000000}", spec.ScanNumber) + "\t"
                + String.Format("{0:000000}", spec.ScanNumber) + "\t"
                + Math.Round(spec.Precursors[0].Item1, mzDecimalPlace + 1) + "\n"
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
                writer.Write(Math.Round(spec.PrecursorIntensity, intensityDecimalPlace) + "\n");
            }
            else
            {
                writer.Write("\n");
            }

            // if precursor(s) is/are corrected or prediected, write the corrected/predicted m/z's and charge states;
            if (spec.PrecursorRefined)
            {
                foreach (Tuple<double, int> prec in spec.Precursors)
                {
                    writer.Write("I\tPredicted Precursor: " + Math.Round(prec.Item1, mzDecimalPlace + 1) + " x " + prec.Item2 + "\n");
                }
            }

            foreach (Tuple<double, int> prec in spec.Precursors)
            {
                double precMH = (prec.Item1 - Utils.PROTON_MASS) * prec.Item2 + Utils.PROTON_MASS;
                writer.Write("Z\t" + prec.Item2 + "\t" + Math.Round(precMH, mzDecimalPlace + 1) + "\n");
            }


            foreach (Ion peak in spec.Peaks)
            {
                writer.Write(Math.Round(peak.MZ, mzDecimalPlace) + " "
                    + Math.Round(peak.Intensity, intensityDecimalPlace));
                if (showPeakChargeState && spec.ActivationMethod == Activation.HCD)
                {
                    writer.Write(" " + peak.Charge);
                }
                if (showPeakResolution && spec.ActivationMethod == Activation.HCD)
                {
                    writer.Write(" " + Math.Round(peak.Resolution, 2));
                }
                writer.Write("\n");
            }

            writer.Flush();
        }

        /** 
         * write to MSn file with duplicates.
         */
        public static void WriteToMSnWithDuplicates(StreamWriter writer, MassSpectrum spec, int mzDecimalPlace,
           int intensityDecimalPlace, bool showPeakChargeState, bool showPeakResolution)
        {
            if (spec.Precursors == null || spec.Precursors.Count == 0)
            {
                return;
            }

            foreach (Tuple<double, int> prec in spec.Precursors)
            {
                double precMH = (prec.Item1 - Utils.PROTON_MASS) * prec.Item2 + Utils.PROTON_MASS;
                writer.Write("S\t" + String.Format("{0:000000}", spec.ScanNumber) + "\t"
                    + String.Format("{0:000000}", spec.ScanNumber) + "\t"
                    + Math.Round(prec.Item1, mzDecimalPlace + 1) + "\n"
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
                    writer.Write(Math.Round(spec.PrecursorIntensity, intensityDecimalPlace) + "\n");
                }
                else
                {
                    writer.Write("\n");
                }

                // if precursor(s) is/are corrected or prediected, write the corrected/predicted m/z's and charge states;
                if (spec.PrecursorRefined)
                {
                    writer.Write("I\tPredicted Precursor: " + Math.Round(prec.Item1, mzDecimalPlace + 1) + " x " + prec.Item2 + "\n");
                }

                writer.Write("Z\t" + prec.Item2 + "\t" + Math.Round(precMH, mzDecimalPlace + 1) + "\n");

                foreach (Ion peak in spec.Peaks)
                {
                    writer.Write(Math.Round(peak.MZ, mzDecimalPlace) + " "
                        + Math.Round(peak.Intensity, intensityDecimalPlace));
                    if (showPeakChargeState && spec.ActivationMethod == Activation.HCD)
                    {
                        writer.Write(" " + peak.Charge);
                    }
                    if (showPeakResolution && spec.ActivationMethod == Activation.HCD)
                    {
                        writer.Write(" " + Math.Round(peak.Resolution, 2));
                    }
                    writer.Write("\n");
                }

                writer.Flush();
            }
        }

        public static void WriteToMGF(StreamWriter writer, MassSpectrum spec, String parentFileName, int mzDecimalPlace, 
            int intensityDecimalPlace, bool showPeakChargeState, bool showPeakResolution)
        {
            writer.Write("BEGIN IONS\n");
            writer.Write("TITLE=" + parentFileName + "\n");
            writer.Write("SCANS=" + spec.ScanNumber + "\n");
            writer.Write("RTINSECONDS=" + (spec.RetentionTime * 60) + "\n");
            writer.Write("CHARGE=" + spec.Precursors[0].Item2);
            if (spec.Precursors[0].Item2 >= 0)
            {
                writer.Write("+\n");
            }
            writer.Write("PEPMASS=" + Math.Round(spec.Precursors[0].Item1, mzDecimalPlace) + "\n");

            // write peaks;
            foreach (Ion peak in spec.Peaks)
            {
                writer.Write(Math.Round(peak.MZ, mzDecimalPlace) + " "
                    + Math.Round(peak.Intensity, intensityDecimalPlace) + "\n");
            }
            writer.Write("END IONS\n\n");
            writer.Flush();
        }

        public static void WriteToMSM(StreamWriter writer, MassSpectrum spec, String parentFileName, int mzDecimalPlace,
           int intensityDecimalPlace, bool showPeakChargeState, bool showPeakResolution)
        {
            writer.Write("BEGIN IONS\n");
            writer.Write("PEPMASS=" + Math.Round(spec.Precursors[0].Item1, mzDecimalPlace) + "\n");
            writer.Write("CHARGE=" + spec.Precursors[0].Item2);
            if (spec.Precursors[0].Item2 >= 0)
            {
                writer.Write("+\n");
            }
            writer.Write("TITLE=" + parentFileName+
                        " from: "+(spec.RetentionTime * 60)+" to "+(spec.RetentionTime * 60)+
                        " period:" + parentFileName +
                        " experiment: 1 cycles: 1" +
                        " precIntensity: ");
            if (!double.IsNaN(spec.PrecursorIntensity))
            {
                writer.Write(Math.Round(spec.PrecursorIntensity, intensityDecimalPlace));
            }
            else
            {
                writer.Write("no");
            }
            writer.Write(" FinneganScanNumber: " + spec.ScanNumber + "\n");
            // write peaks;
            foreach (Ion peak in spec.Peaks)
            {
                writer.Write(Math.Round(peak.MZ, mzDecimalPlace) + " "
                    + Math.Round(peak.Intensity, intensityDecimalPlace) + "\n");
            }
            writer.Write("END IONS\n\n");
            writer.Flush();
        }
    }
}
