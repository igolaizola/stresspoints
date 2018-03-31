using System;
using System.Collections.Generic;
using System.Text;

namespace StressPoints.Config
{
    public class AthleteConfig
    {
        public double runFTHeartRate;
        public double bikeFTHeartRate;
        public string runFTPace;
        public string swimFTPace;
        public double bikeFTPower;
        public double bikeFTVirtualPower;
        public double weight;
        public double height;
        public double minHeartRate;
        public double maxHeartRate;

        public double runFTSpeed
        {
            get
            {
                return (1 / TimeSpan.Parse(runFTPace).TotalHours) * 1000 / 60;
            }
        }

        public double swimFTSpeed
        {
            get
            {
                return (1 / TimeSpan.Parse(swimFTPace).TotalHours) * 100 / 60;
            }
        }
    }
}
