using System;
using System.Collections.Generic;
using System.Text;

namespace StressPoints.Config
{
    public class ProgramConfig
    {
        public class GoogleConfig
        {
            public string appName;
            public string sheetId;
        }

        public class StravaConfig
        {
            public string token;
        }

        public AthleteConfig athlete;

        public StravaConfig strava;

        public GoogleConfig google;
    }
}
