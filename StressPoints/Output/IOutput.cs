using StressPoints.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace StressPoints.Output
{
    public interface IOutput
    {
        void Write(PointsData data);
    }
}
