using System;
using System.Collections.Generic;

namespace StressPoints.Core
{
    public class Interval
    {
        public double Time;
        public double Distance;
        public double Grade;
        public double GradeS;
        public double HeartRate;
        public double Pace;
        public double Speed;

        /// <summary>
        /// Normalized graded speed
        /// </summary>
        public double NormGradedSpeed;

        /// <summary>
        /// Normalized graded pace
        /// </summary>
        public double NormGradedPace;

        private double elevation0, elevation1;

        public Interval(double time0, double time1, double distance0, double distance1, double elevation0, double elevation1, double grade, double heartRate)
        {
            Time = time1 - time0;
            Distance = distance1 - distance0;
            this.elevation0 = elevation0;
            this.elevation1 = elevation1;
            Grade = CalcGrade(elevation1 - elevation0, distance1 - distance0);
            GradeS = grade;
            HeartRate = heartRate;
            Speed = (distance1 - distance0) / (time1 - time0);
            NormGradedSpeed = CalcNormGradedSpeed(Speed, Grade);
            Pace = (1 / Speed) * 1000 / 60;
            NormGradedPace = (1 / NormGradedSpeed) * 1000 / 60;
        }

        private double CalcGrade(double a, double d)
        {
            if (d == 0)
                return 0;
            return (180 * Math.Atan(a / d)) / Math.PI;
        }

        private double CalcNormGradedSpeed(double speed, double grade)
        {
            var pace = 1 / speed;
            if (grade > 0)
            {
                double coefficient = 0.033;
                pace = pace / (1 + (coefficient * grade));
            }
            else
            {
                double coefficient = 0.01815;
                pace = pace / (1 - (coefficient * grade));
            }
            return 1 / pace;
        }

        public IList<Interval> GetInnerIntervals()
        {
            if (Time <= 1)
                return new List<Interval>() { this };
            else
            {
                IList<Interval> intervals = new List<Interval>();
                for (double j = 0; j < Time - 1; j++)
                {
                    Interval interval = new Interval(
                        j,
                        j + 1,
                        j * (Distance / Time),
                        (j + 1) * (Distance / Time),
                        j * ((elevation1 - elevation0) / Time),
                        (j + 1) * ((elevation1 - elevation0) / Time),
                        GradeS,
                        HeartRate
                        );
                    intervals.Add(interval);
                }
                return intervals;
            }
        }
    }
}
