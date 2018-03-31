using StressPoints.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace StressPoints.Metrics
{
    public static class BikeScoreMetric
    {
        public static double BikeScore(this IList<Interval> intervals, double cp)
        {
            double time = intervals.TotalTime();
            double xp = XPower(intervals);
            double ri = xp / cp;
            double normWork = xp * time;
            double rawBikeScore = normWork * ri;
            double workInAnHourAtCP = cp * 3600;
            double score = rawBikeScore / workInAnHourAtCP * 100.0;
            return score;
        }

        private static double XPower(IList<Interval> intervals)
        {
            double secsDelta = intervals[0].Time;
            double sampsPerWindow = 25.0 / secsDelta;
            double attenuation = sampsPerWindow / (sampsPerWindow + secsDelta);
            double sampleWeight = secsDelta / (sampsPerWindow + secsDelta);

            double lastSecs = 0.0;
            double weighted = 0.0;

            double total = 0.0;
            int count = 0;

            double secs = 0.0;
            foreach (var i in intervals)
            {
                secs += i.Time;
                while ((weighted > 0.1) && (secs > lastSecs + secsDelta + 0.1))
                {
                    weighted *= attenuation;
                    lastSecs += secsDelta;
                    total += Math.Pow(weighted, 4.0);
                    count++;
                }
                weighted *= attenuation;
                double kmh = i.Speed * 3.6;
                double power = VirtualPowerMetric.VirtualPower(kmh);
                weighted += sampleWeight * power;
                lastSecs = secs;
                total += Math.Pow(weighted, 4.0);
                count++;
            }
            return count > 0 ? Math.Pow(total / count, 0.25) : 0.0;
        }
    }
}
