using RawConverter.MassSpec;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RawConverter.Common
{
    public class Envelope
    {
        public Ion MonoisotPeak { get; set; }
        public int Charge { get; set; }
        public List<Ion> PeaksInEnvelope { get; set; }
        public double[] TheoIsotDist { get; set; }
        public double[] ObsvIsotDist { get; set; }
        public double Score { get; set; }
        public Ion HighestPeakInRange { get; set; }
        public double FractionQuantity { get; set; }
        private bool _updateRequired;

        public Envelope(Ion startPeak, int z, List<Ion> peaks, double[] theoIsotDist, Ion highestPeak)
        {
            MonoisotPeak = startPeak;
            Charge = z;
            PeaksInEnvelope = peaks;
            TheoIsotDist = theoIsotDist;
            HighestPeakInRange = highestPeak;
            FractionQuantity = 1;

            CalcScore();
        }

        public bool UpdateRequired
        {
            get
            {
                return _updateRequired;
            }

            set
            {
                _updateRequired = value;
                OnUpdateRequiredChanged();
            }
        }

        protected virtual void OnUpdateRequiredChanged()
        {
            if (_updateRequired)
            {
                CalcScore();
                _updateRequired = false;
            }
        }

        private void CalcScore()
        {
            ObsvIsotDist = new double[TheoIsotDist.Length];
            double maxH = 1;
            foreach (Ion peak in PeaksInEnvelope)
            {
                if (peak.Intensity > maxH)
                {
                    maxH = peak.Intensity;
                }
            }
            for (int i = 0; i < ObsvIsotDist.Length; i++)
            {
                if (i < PeaksInEnvelope.Count)
                {
                    ObsvIsotDist[i] = 100 * PeaksInEnvelope[i].Intensity / maxH;
                }
            }

            Score = 0;
            double tDenom = 0;
            double oDenom = 0;
            for (int i = 0; i < ObsvIsotDist.Length; i++)
            {
                Score += Math.Pow(ObsvIsotDist[i] - TheoIsotDist[i], 2);
                tDenom += Math.Pow(TheoIsotDist[i], 2);
                oDenom += Math.Pow(ObsvIsotDist[i], 2);
            }

            // normalize the score according to its cosine similarity;
            double cosSim = 0;
            for (int i = 0; i < ObsvIsotDist.Length; i++)
            {
                cosSim += TheoIsotDist[i] * ObsvIsotDist[i];
            }
            cosSim /= Math.Sqrt(tDenom * oDenom);
            Score = cosSim / Math.Sqrt(Score / tDenom);

            // normalize the score according to its relative;
            double coef = Math.Log10(10 * MonoisotPeak.Intensity / HighestPeakInRange.Intensity);
            Score *= coef;
        }

        public override string ToString()
        {
            return (MonoisotPeak.MZ.ToString() + ", " + Charge);
        }
    }
}
