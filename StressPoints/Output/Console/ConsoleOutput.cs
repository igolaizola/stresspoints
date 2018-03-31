using StressPoints.Core;
using System;

namespace StressPoints.Output
{
    public class ConsoleOutput : IOutput
    {
        public void Write(PointsData data)
        {
            Console.WriteLine("{0} {1} {2} [{3},{4},{5}]",
                data.Id, data.Type, data.Date,
                data.TrimpPoints, data.TriScorePoints, data.TssPoints);
        }
    }
}
