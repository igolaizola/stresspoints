using StressPoints.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace StressPoints.Metrics
{
    public static class GovssMetric
    {
        public static double Govss(this IList<Interval> intervals, double rts, double w, double h)
        {
            double time = intervals.TotalTime();
            double rtp = Power(w, h, rts);
            double lnp = Lnp(intervals, w, h);
            double iwf = rtp > 0 ? lnp / rtp : 0.0;
            double rGovss = lnp * time * iwf;
            return rtp > 0 ? ((rGovss) / (rtp * 3600)) * 100.0 : 0.0;
        }

        public static double XPace(this IList<Interval> intervals, double w, double h)
        {
            return (1 / intervals.XSpeed(w, h)) * 1000 / 60;
        }

        public static double XSpeed(this IList<Interval> intervals, double w, double h)
        {
            double lnp = Lnp(intervals, w, h);
            double low = 0.0, high = 10.0, speed;
            if (lnp <= 0.0)
                speed = low;
            else if (lnp >= Power(w, h, high))
                speed = high;
            else do
                {
                    speed = (low + high) / 2.0;
                    double watts = Power(w, h, speed);
                    if (Math.Abs(watts - lnp) < 0.001) break;
                    else if (watts < lnp) low = speed;
                    else if (watts > lnp) high = speed;
                } while (high - low > 0.01);
            return (speed > 0.01) ? speed : 0.0;
        }

        private static double Lnp(IList<Interval> intervals, double w, double h)
        {
            double[] vSp = new double[120];
            double[] vSl = new double[120];
            double[] vPw = new double[30];
            double sp = 0.0;
            double sl = 0.0;
            double pw = 0.0;
            double s0 = 0.0;
            int i120 = 0;
            int i30 = 0;
            int count = 0;
            double total = 0.0;
            foreach(var i in intervals)
            {
                sp += i.Speed;
                sp -= vSp[i120];
                vSp[i120] = i.Speed;
                double sp120 = sp / Math.Min(count + 1, 120);

                sl += i.Grade/100.0;
                sl -= vSl[i120];
                vSl[i120] = i.Grade/100.0;
                double sl120 = sl / Math.Min(count + 1, 120);

                double wt = Power(w, h, sp120, sl120, sp120 * 1, s0);
                pw += wt;
                pw -= vPw[i30];
                vPw[i30] = wt;

                total += Math.Pow(pw / Math.Min(count + 1, 30), 4);

                s0 = sp120;
                count++;
                i120 = (i120 + 1) % 120;
                i30 = (i30 + 1) % 30;
            }

            return Math.Pow(total / count, 0.25);
        }

        private static double Power(double w, double h, double s, double sl = .0, double d = .0, double s0 = .0)
        {
            double Af = (0.2025 * Math.Pow(h, 0.725) * Math.Pow(w, 0.425)) * 0.266;
            double cAero = 0.5 * 1.2 * 0.9 * Af * Math.Pow(s, 2) / w;
            double cKin = d > 0 ? 0.5 * (Math.Pow(s, 2) - Math.Pow(s0, 2)) / d : 0.0;
            double cSlope = 155.4 * Math.Pow(sl, 5) - 30.4 * Math.Pow(sl, 4) - 43.3 * Math.Pow(sl, 3) + 46.3 * Math.Pow(sl, 2) + 19.5 * sl + 3.6;
            double eff = (0.25 + 0.054 * s) * (1 - 0.5 * s / 8.33);
            return (cAero + cKin + cSlope * eff) * s * w;
        }
    }
}
