using RawConverter.Common;
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

using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Factorization;
using MathNet.Numerics;

namespace RawConverter.GUI
{
    public partial class DIAPrecursorPredictorGUI : Form
    {
        public DIAPrecursorPredictorGUI()
        {
            InitializeComponent();
        }

        private void btPredict_Click(object sender, EventArgs e)
        {
            testMatrix();
            lbOutput.Items.Clear();
            string[] lines = tbExportedSpectrum.Lines;
            // define the pattern for matching m/z lines;
            string mzLinePattern = @"\d+.\d+\t[ ]*\d+.\d+";
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

            // predict the precursors;
            PrecursorPredictor dpp = new PrecursorPredictor(5, 1, 6, 0);
            List<Envelope> envList = dpp.PredictPrecursors(peakList);
            int counter = 0;

            foreach (Envelope env in envList)
            {
                if (env.Score >= dpp.Threshold)
                {   
                    lbOutput.Items.Add("Envelope " + (++counter) + ":");
                    lbOutput.Items.Add("\tm/z = " + env.MonoisotPeak.MZ + ", z = " + env.Charge);
                    lbOutput.Items.Add("\tscore = " + env.Score);
                    lbOutput.Items.Add("");
                }
            }
        }

        private void testMatrix()
        {
            double[,] arr = new double[,] {
                {0, 100, 0, 0, 0, 0, 0, 0, 0}, 
                {0, 44.86000, 0, 0, 0, 0, 0, 0, 0}, 
                {0, 11.93000, 0, 0, 0, 0, 0, 0, 0}, 
                {0, 0, 0, 100, 0, 0, 0, 0, 0}, 
                {0, 0, 0, 0, 0, 0, 0, 74.62000, 0}, 
                {0, 0, 0, 0, 0, 0, 0, 0, 100}, 
                {0, 0, 0, 88.37000, 0, 0, 0, 100, 0}, 
                {0, 0, 0, 0, 0, 0, 100, 0, 0}, 
                {100, 0, 0, 0, 0, 0, 88.37000, 0, 44.86000}, 
                {88.37000, 0, 100, 0, 0, 0, 47.08000, 0, 0}, 
                {47.08000, 0, 0, 0, 0, 0, 18.23000, 0, 11.93000}, 
                {18.23000, 0, 44.86000, 0, 0, 0, 5.33000, 0, 0}, 
                {0, 0, 0, 0, 0, 100, 0, 0, 0}, 
                {0, 0, 0, 0, 0, 88.37000, 0, 0, 0}, 
                {0, 0, 0, 0, 100, 0, 0, 0, 0}, 
                {0, 0, 0, 0, 88.37000, 0, 0, 0, 0}
            };
            var A = SparseMatrix.OfArray(arr);
            string aStr = A.ToMatrixString(16, 9);
            var piA = PseudoInverse(A);
            string piaStr = piA.ToMatrixString(9, 16);

            var B = A * piA;
            string bStr = B.ToMatrixString(16, 16);

            Console.Write("");
        }

        public Matrix<double> PseudoInverse(Matrix<double> M) 
        { 
            var svd = M.Svd(true);
            var W = svd.W;
            var s = svd.S;

            // The first element of W has the maximum value. 
            double tolerance = Precision.EpsilonOf(2) * Math.Max(M.RowCount, M.ColumnCount) * W[0, 0];

            for (int i = 0; i < s.Count; i++) 
            { 
                if (s[i] < tolerance) 
                    s[i] = 0; 
                else 
                    s[i] = 1 / s[i]; 
            } 
            W.SetDiagonal(s);

            // (U * W * VT)T is equivalent with V * WT * UT 
            return (svd.U * W * svd.VT).Transpose(); 
        }
    }
}
