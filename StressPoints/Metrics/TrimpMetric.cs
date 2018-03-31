using StressPoints.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StressPoints.Metrics
{
    public static class TrimpMetric
    {
        public static double HeartRateTrimp100(this IList<Interval> intervals, double functionalThreshold, double minHr, double maxHr)
        {
            return intervals.Sum(i => i.HeartRateTrimp100(functionalThreshold, minHr, maxHr));
        }

        public static double HeartRateTrimp100(this Interval i, double functionalThreshold, double minHr, double maxHr)
        {
            double ftHrr = (functionalThreshold - minHr) / (maxHr - minHr);
            double trimpFt = 60.0 * ftHrr * 0.64 * Math.Exp(1.92 * ftHrr);
            double hrr = (i.HeartRate - minHr) / (maxHr - minHr);
            double trimp = (i.Time / 60.0) * hrr * 0.64 * Math.Exp(1.92 * hrr);
            return 100 * (trimp / trimpFt);
        }

    }
}
