using System;
using System.Collections.Generic;
using System.Text;

namespace StressPoints.Core
{
    public class PointsData
    {
        public long Id;

        public int Type;

        public DateTime Date;

        public double TrimpPoints;

        public double TriScorePoints;

        public double TssPoints;

        public PointsData(long id, int type, DateTime date)
        {
            this.Id = id;
            this.Type = type;
            this.Date = date;
        }
    }
}
