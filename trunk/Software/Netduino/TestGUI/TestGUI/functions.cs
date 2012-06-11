using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BluetoothLights
{
    public class functions
    {
        public static double GetMax(double[] DoubleCollection)
        {
            double max = double.MinValue;
            foreach (double i in DoubleCollection)
            {
                if (i > max)
                {
                    max = i;
                }
            }
            return max;
        }

        public static int GetMax(int[] DoubleCollection)
        {
            int max = int.MinValue;
            foreach (int i in DoubleCollection)
            {
                if (i > max)
                {
                    max = i;
                }
            }
            return max;
        }
    }
}
