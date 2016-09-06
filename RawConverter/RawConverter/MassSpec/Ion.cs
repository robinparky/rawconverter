/**
 * Program:     MultiXtract
 * Authors:     Diogo Borges Lima, Paulo Costa Carvalho and Lin He
 * Update:      4/17/2014
 * Update by:   Diogo Borges Lima
 * Description: Ion Class.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RawConverter.Common;

namespace RawConverter.MassSpec
{
    [Serializable]
    public class Ion : ICloneable
    {
        /// <summary>
        /// Public variables
        /// </summary>
        public double MZ { get; set; }
        public double Intensity { get; set; }
        public int Charge { get; private set; }
        public double Resolution { get; set; }
        public double Mass { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="intensity"></param>
        public Ion(double intensity) { Intensity = intensity; }
        public Ion(double mz, double intensity)
        {
            MZ = mz;
            Intensity = intensity;
        }

        public Ion(double mz, double intensity, int charge)
        {
            MZ = mz;
            Intensity = intensity;
            Charge = charge;
            CalculateMass();
        }

        public Ion(double mz, double intensity, int charge, double resolution)
        {
            MZ = mz;
            Intensity = intensity;
            Charge = charge;
            Resolution = resolution;
            CalculateMass();
        }

        public int Compare(Ion i1, Ion i2)
        {
            if (i1.MZ < i2.MZ)
            {
                return -1;
            }
            else if (i1.MZ > i2.MZ)
            {
                return 1;
            }

            return 0;
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }

        public Ion Clone()
        {
            return (Ion) this.MemberwiseClone();
        }

        private void CalculateMass()
        {
            CalculateMass(Charge);
        }

        private void CalculateMass(int charge)
        {
            if (charge == 0)
            {
                charge = 1;
            }
            Mass = Utils.MassFromMZ(MZ, charge);
        }

        public static int DescendingIntensityComparison(Ion left, Ion right)
        {
            return -(left.Intensity.CompareTo(right.Intensity));
        }

        public static int AscendingMassComparison(Ion left, Ion right)
        {
            return left.Mass.CompareTo(right.Mass);
        }

        public override string ToString()
        {
            return MZ + ", " + Intensity;
        }
    }

    public class IonMzComparer : IComparer<Ion>
    {
        // Compares by m/z values
        public int Compare(Ion p1, Ion p2)
        {
            if (p1.MZ < p2.MZ)
            {
                return -1;
            }
            else if (p1.MZ > p2.MZ)
            {
                return 1;
            }

            return 0;
        }
    }
}
