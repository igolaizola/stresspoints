using StressPoints.Core;
using System.Collections.Generic;
using System.Linq;

namespace StressPoints.Metrics
{
    public static class TssMetric
    {
        public static double HeartRateTss(this IList<Interval> intervals, double functionalThreshold, double minHr)
        {
            double tss = 0.0;
            functionalThreshold = functionalThreshold - minHr;
            foreach (var i in intervals)
            {
                var hr = i.HeartRate - minHr;
                if (hr > 0)
                    tss += (i.Time * hr * (hr / functionalThreshold)) / (3600 * functionalThreshold) * 100;
            }

            return tss;
        }

        public static double HeartRateZonesTss(this IList<Interval> intervals, double ft, double minHr)
        {
            return intervals.Sum(i => i.HeartRateZonesTss(ft, minHr));
        }

        public static double HeartRateZonesTss(this Interval i, double ft, double minHr)
        {
            double[] zonesMax = new double[] { 0.27, 0.54, 0.81, 0.85, 0.89, 0.93, 0.99, 1.02, 1.06 };
            double[] tssPoints = new double[] { 10, 20, 40, 50, 60, 70, 80, 100, 120, 140 };
            if (i.HeartRate < minHr)
                return 0;

            for (int j = 0; j < 9; j++)
            {
                if (i.HeartRate < ft * zonesMax[j])
                    return tssPoints[j] * (i.Time / 3600.0);
            }
            return tssPoints[9] * (i.Time / 3600.0);
        }

        public static double NormGradedSpeedTss(this IList<Interval> intervals, double ft)
        {
            double tss = 0.0;
            foreach (var i in intervals)
            {
                tss += (i.Time * i.NormGradedSpeed * (i.NormGradedSpeed / ft)) / (3600 * ft) * 100;
            }
            return tss;
        }

        public static double AvgHeartRateTss(this IList<Interval> intervals, double functionalThreshold, double minHr)
        {
            double hr = intervals.AvgHeartRate() - minHr;
            if (hr < 0)
                return 0;
            functionalThreshold = functionalThreshold - minHr;
            return (intervals.TotalTime() * hr * (hr / functionalThreshold)) / (3600 * functionalThreshold) * 100;
        }

        public static double AvgNormGradedSpeedTss(this IList<Interval> intervals, double functionalThreshold)
        {
            double ngs = intervals.AvgNormGradedSpeed();
            return (intervals.TotalTime() * ngs * (ngs / functionalThreshold)) / (3600 * functionalThreshold) * 100;
        }

        public static double AvgPowerTss(this IList<Interval> intervals, double functionalThreshold, double avgPower)
        {
            return (intervals.TotalTime() * avgPower * (avgPower / functionalThreshold)) / (3600 * functionalThreshold) * 100;
        }
    }
}
