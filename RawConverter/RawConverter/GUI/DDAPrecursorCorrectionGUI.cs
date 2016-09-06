using RawConverter.Common;
using RawConverter.DDADataProcess;
using RawConverter.DIADataProcess;
using RawConverter.MassSpec;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RawConverter.GUI
{
    public partial class DDAPrecursorCorrectorGUI : Form
    {
        public DDAPrecursorCorrectorGUI()
        {
            InitializeComponent();
        }

        private void btPredict_Click(object sender, EventArgs e)
        {
            lbOutput.Items.Clear();
            string[] lines = tbExportedSpectrum.Lines;
            // define the pattern for matching m/z lines;
            string mzLinePattern = @"\d+.\d+\t[ ]+\d+.\d+";
            string valuePattern = @"\d+.\d+";
            List<Ion> peakList = new List<Ion>();
            for (int idx = 0; idx < lines.Length; idx++)
            {
                MatchCollection matches = Regex.Matches(lines[idx], mzLinePattern);
                if (matches.Count > 0)
                {
                    matches = Regex.Matches(matches[0].Groups[0].Value, valuePattern);
                    double mz = double.Parse(matches[0].Groups[0].Value);
                    double h = double.Parse(matches[1].Groups[0].Value);
                    peakList.Add(new Ion(mz, h));
                }
            }

            // get the instrument designated precursor m/z and charge state;
            double precMz = double.Parse(lines[lines.Length - 2].Trim());
            int precZ = int.Parse(lines[lines.Length - 1].Trim());

            // predict the precursors;
            PrecursorCorrector pc = new PrecursorCorrector();
            List<Envelope> envList = pc.FindEnvelopes(peakList, precMz, precZ);
            int counter = 0;
            foreach (Envelope env in envList)
            {
                lbOutput.Items.Add("Envelope " + (++counter) + ":");
                lbOutput.Items.Add("\tm/z = " + env.MonoisotPeak.MZ + ", z = " + env.Charge);
                lbOutput.Items.Add("\tscore = " + env.Score);
                lbOutput.Items.Add("");
            }
        }
    }
}
