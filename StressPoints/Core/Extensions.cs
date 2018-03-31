using StravaSharp;
using System.Collections.Generic;
using System.Linq;

namespace StressPoints.Core
{
    public static class Extensions
    {
        public static double AveragePace(this ActivitySummary activitySummary)
        {
            return (1 / activitySummary.AverageSpeed) * 1000 / 60;
        }

        public static double TotalDistance(this IList<Interval> intervals)
        {
            return intervals.Sum(i => i.Distance);
        }

        public static double TotalTime(this IList<Interval> intervals)
        {
            return intervals.Sum(i => i.Time);
        }

        public static double AvgSpeed(this IList<Interval> intervals)
        {
            return intervals.TotalDistance() / intervals.TotalTime();
        }

        public static double AvgPace(this IList<Interval> intervals)
        {
            return (1 / intervals.AvgSpeed()) * 1000 / 60;
        }

        public static double AvgNormGradedSpeed(this IList<Interval> intervals)
        {
            double avg = 0.0;
            double totalDistance = intervals.TotalDistance();
            foreach (var i in intervals)
                avg += (i.Distance / totalDistance) * i.NormGradedSpeed;
            return avg;
        }

        public static double AvgNormGradedPace(this IList<Interval> intervals)
        {
            return (1 / intervals.AvgNormGradedSpeed()) * 1000 / 60;
        }

        public static double AvgHeartRate(this IList<Interval> intervals)
        {
            double avg = 0.0;
            double totalTime = intervals.TotalTime();
            foreach (var i in intervals)
                avg += (i.Time / totalTime) * i.HeartRate;
            return avg;
        }

    }
}
