using StressPoints.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace StressPoints.Metrics
{
    public static class SwimScoreMetric
    {
        public static double SwimScore(this IList<Interval> intervals, double sts, double w, double h)
        {
            double time = intervals.TotalTime();
            double rtp = Power(w, sts);
            double lnp = XPower(intervals, w);
            double iwf = rtp > 0 ? lnp / rtp : 0.0;
            double rGovss = lnp * time * iwf;
            return rtp > 0 ? ((rGovss) / (rtp * 3600)) * 100.0 : 0.0;
        }


        private static double Power(double w, double s)
        {
            return ((0.35 * w + 2) / 0.6) * Math.Pow(s, 3);
        }

        private static double XPower(IList<Interval> intervals, double w)
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
            foreach(var i in intervals)
            {
                secs += i.Time;
                while ((weighted > 0.1) && (secs > lastSecs + secsDelta + 0.1)) {
                    weighted *= attenuation;
                    lastSecs += secsDelta;
                    total += Math.Pow(weighted, 3.0);
                    count++;
                }
                weighted *= attenuation;
                weighted += sampleWeight* Power(w, i.Speed);
                lastSecs = secs;
                total += Math.Pow(weighted, 3.0);
                count++;
            }
            return count > 0 ? Math.Pow(total / count, 1/3.0) : 0.0;
        }
    }
}
