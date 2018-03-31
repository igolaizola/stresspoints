using StravaSharp;
using StressPoints.Config;
using StressPoints.Core;
using StressPoints.Metrics;
using StressPoints.Output;
using System;
using System.Collections.Generic;

namespace StressPoints
{
    public class PointsGenerator
    {
        #region Builder

        public class Builder
        {
            public AthleteConfig AthleteConfig { get; private set; }

            public string StravaToken { get; private set; }

            public IOutput Output { get; private set; }

            public DateTime? StartDate { get; private set; }

            public long? ActivityId { get; private set; }

            public Builder WithAthleteConfig(AthleteConfig config)
            {
                this.AthleteConfig = config;
                return this;
            }

            public Builder WithStravaToken(string token)
            {
                this.StravaToken = token;
                return this;
            }

            public Builder WithOutput(IOutput output)
            {
                this.Output = output;
                return this;
            }

            public Builder WithStartDate(DateTime date)
            {
                this.StartDate = date;
                return this;
            }

            public Builder WithActivityId(long id)
            {
                this.ActivityId = id;
                return this;
            }

            public PointsGenerator Build()
            {
                return new PointsGenerator(this);
            }
        }

        #endregion

        private IOutput output;
        private Client stravaClient;
        private AthleteConfig athleteConfig;
        private DateTime? startDate;
        private long? activityId;

        public PointsGenerator(Builder builder)
        {
            this.athleteConfig = builder.AthleteConfig;
            this.output = builder.Output;
            this.startDate = builder.StartDate;
            this.activityId = builder.ActivityId;

            string accessToken = builder.StravaToken;
            TestAuthenticator authenticator = new TestAuthenticator(accessToken);
            stravaClient = new Client(authenticator);
        }

        public void Run()
        {
            IList<ActivitySummary> activities;
            if (activityId != null)
                activities = new[] { stravaClient.Activities.Get(activityId.Value).Result };
            else if (startDate != null)
                activities = stravaClient.Activities.GetAthleteActivitiesAfter(startDate.Value).Result;
            else
                activities = stravaClient.Activities.GetAthleteActivities().Result;

            foreach (var activity in activities)
            {
                Console.WriteLine("Import: {0} {1}", activity.Id, activity.Name);

                IList<Interval> intervals = null;
                try { intervals = ImportIntervals(activity.Id); }
                catch { }

                Console.WriteLine("Saving...");
                SaveStressPoints(activity, intervals);
            }
        }

        /// <summary>
        /// Import interval data from a specific strava activity
        /// </summary>
        /// <param name="activityId"></param>
        /// <returns></returns>
        public IList<Interval> ImportIntervals(long activityId)
        {
            Activity activity = stravaClient.Activities.Get(activityId).Result;
            List<Interval> intervals = new List<Interval>();

            List<Stream> streamsTimeDistance;
            try
            {
                streamsTimeDistance = stravaClient.Activities.GetActivityStreams(activityId, StreamType.Time).Result;
            }
            catch
            {
                Interval interval = new Interval(0.0, activity.ElapsedTime, 0.0, activity.Distance, 0.0, activity.TotalElevationGain, 0.0, activity.AverageHeartRate);
                return interval.GetInnerIntervals();
            }

            List<Stream> streamsHeartRate = stravaClient.Activities.GetActivityStreams(activityId, StreamType.HeartRate).Result;
            List<Stream> streamsElevation = stravaClient.Activities.GetActivityStreams(activityId, StreamType.Altitude).Result;
            List<Stream> streamsGrade = stravaClient.Activities.GetActivityStreams(activityId, StreamType.GradeSmooth).Result;
            List<Stream> streamsPower = stravaClient.Activities.GetActivityStreams(activityId, StreamType.Watts).Result;

            int n = streamsTimeDistance[0].Data.Length;
            for (int i = 0; i < n - 1; i++)
            {
                double time0 = (long)streamsTimeDistance[0].Data[i];
                double time1 = (long)streamsTimeDistance[0].Data[i + 1];
                double distance0 = streamsTimeDistance.Count > 1 ? (double)streamsTimeDistance[1].Data[i] : 0;
                double distance1 = streamsTimeDistance.Count > 1 ? (double)streamsTimeDistance[1].Data[i + 1] : 0;
                double heartRate = streamsHeartRate.Count > 1 ? (long)streamsHeartRate[1].Data[i + 1] : 0;
                double elevation0 = streamsElevation.Count > 1 ? (double)streamsElevation[1].Data[i] : 0;
                double elevation1 = streamsElevation.Count > 1 ? (double)streamsElevation[1].Data[i + 1] : 0;
                double grade = streamsGrade.Count > 1 ? (double)streamsGrade[1].Data[i + 1] : 0;

                Interval interval = new Interval(time0, time1, distance0, distance1, elevation0, elevation1, grade, heartRate);
                intervals.AddRange(interval.GetInnerIntervals());
            }
            return intervals;
        }

        /// <summary>
        /// Save calculated stress score into a google sheet
        /// </summary>
        /// <param name="activity"></param>
        /// <param name="intervals"></param>
        /// <param name="googleApi"></param>
        /// <param name="config"></param>
        public void SaveStressPoints(ActivitySummary activity, IList<Interval> intervals)
        {
            PointsData data = new PointsData(activity.Id, (int)activity.Type, activity.StartDateLocal.Date);
            var cfg = this.athleteConfig;

            switch(activity.Type)
            {
                case ActivityType.Run:
                    if(activity.AverageHeartRate > 0)
                        data.TrimpPoints = intervals.HeartRateTrimp100(cfg.runFTHeartRate, cfg.minHeartRate, cfg.maxHeartRate);
                    data.TriScorePoints = intervals.Govss(cfg.runFTSpeed, cfg.weight, cfg.height);
                    data.TssPoints = intervals.NormGradedSpeedTss(cfg.runFTSpeed);
                    break;
                case ActivityType.Ride:
                    if (activity.AverageHeartRate > 0)
                        data.TrimpPoints = intervals.HeartRateTrimp100(cfg.bikeFTHeartRate, cfg.minHeartRate, cfg.maxHeartRate);
                    if (activity.AveragePower > 0 && !activity.DeviceWatts)
                        data.TssPoints = intervals.AvgPowerTss(cfg.bikeFTPower, activity.AveragePower);
                    else if (activity.Distance > 0)
                    {
                        data.TriScorePoints = intervals.BikeScore(cfg.bikeFTVirtualPower);
                        data.TssPoints = intervals.VirtualPowerTss(cfg.bikeFTVirtualPower);
                    }
                    break;
                case ActivityType.Swim:
                    if (intervals == null)
                        data.TssPoints = intervals.AvgNormGradedSpeedTss(cfg.swimFTSpeed);
                    else
                    {
                        data.TriScorePoints = intervals.SwimScore(cfg.swimFTSpeed, cfg.weight, cfg.height);
                        data.TssPoints = intervals.NormGradedSpeedTss(cfg.swimFTSpeed);
                    }
                    break;
            }

            output.Write(data);
        }
    }
}
