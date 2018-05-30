using System;
using System.Collections.Generic;

namespace AcqModeling
{
    public class AnglesDistribution
    {
        private AnglesDistribution() { }
        public static AnglesDistribution Uniform(double from, double to, int count)
        {
            AnglesDistribution result = new AnglesDistribution();
            for (int i = 0; i <= count; i++)
            {
                double value = from + (to - from) * i / count;
                result.angles.Add(value);
            }
            return result;
        }
        public static AnglesDistribution FromArray(double[] Angles)
        {
            AnglesDistribution result = new AnglesDistribution();
            for (int i = 0; i < Angles.Length; i++)
                result.angles.Add(Angles[i]);
            return result;
        }

        public double[] GetAngles() { return angles.ToArray(); }

        private List<double> angles = new List<double>();

        private double First { get { return angles[0]; } }
        private double Last { get { return angles[angles.Count - 1]; } }

        public double this[int index] { get { return angles[index]; } }
        public bool Is360Degree { get { return Math.Abs(Math.Abs(Last - First) - 2 * Math.PI) < .001; } }
        public bool IsStraightOrder { get { return Last > First; } }
        public int Count { get { return angles.Count - 1; } }

        public AnglesDistribution Clone()
        {
            AnglesDistribution res = new AnglesDistribution();
            res.angles = new List<double>(angles);
            return res;
        }
    }
}