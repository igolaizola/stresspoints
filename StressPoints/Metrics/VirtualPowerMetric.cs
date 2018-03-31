using StressPoints.Core;
using System;
using System.Collections.Generic;

namespace StressPoints.Metrics
{
    /// <summary>
    /// Virtual power for indoor trainers
    /// </summary>
    public static class VirtualPowerMetric
    {
        public static double VirtualPowerTss(this IList<Interval> intervals, double functionalThreshold)
        {
            double tss = 0.0;
            foreach (var i in intervals)
            {
                double kmh = i.Speed * 3.6;
                double power = VirtualPower(kmh);
                if (power > 0)
                    tss += (i.Time * power * (power / functionalThreshold)) / (3600 * functionalThreshold) * 100;
            }

            return tss;
        }

        public static double VirtualPower(double kmh)
        {
            // Generic Fluid
            // y = 3.982431421·10 - 3 x3 + 2.297197932·10 - 3 x2 + 3.850158556 x - 1.358163227
            // Lifeline TT 02
            // [ 0,  25) y = 0 + 5.070539130434782x + 0.06222608695652174x2
            // [25,  35) y = 951.96 - 66.556x + 1.402x2
            // [30, INF) y = -59.678333333333335 - 2.459857142857143x + 0.3927047619047619x2
            double power = 0.0;
            if (kmh < 25)
                power = 0 + 5.070539130434782 * kmh + 0.06222608695652174 * kmh * kmh;
            else if (kmh < 35)
                power = 951.96 - 66.556 * kmh + 1.402 * kmh * kmh;
            else
                power = -59.678333333333335 - 2.459857142857143 * kmh + 0.3927047619047619 * kmh * kmh;
            return power;
        }

        /*
        public static double FluidSpeed(double power)
        {
            // Generic Fluid inverse
            // y = 8.652807311·10 - 8 x3 - 1.86779687·10 - 4 x2 + 1.542404338·10 - 1 x + 3.598683734
            double a = 8.652807311 * Math.Pow(10, -8);
            double b = 1.86779687 * Math.Pow(10, -4);
            double c = 0.1542404338;
            double d = 3.598683734;
            double kmh = a * Math.Pow(power, 3) - b * Math.Pow(power, 2) + c * power + d;
            return kmh;
        }
        */
    }
}
