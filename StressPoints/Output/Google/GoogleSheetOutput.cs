using StressPoints.Core;
using System;
using System.Collections.Generic;

namespace StressPoints.Output
{
    public class GoogleSheetOutput : IOutput
    {
        private string appName;

        private string sheetId;

        private GoogleApi googleApi;

        private HashSet<long> storedIds;

        public GoogleSheetOutput(string appName, string sheetId)
        {
            this.appName = appName;
            this.sheetId = sheetId;

            googleApi = new GoogleApi(appName, sheetId);
            storedIds = googleApi.GetIds();
        }

        public void Write(PointsData data)
        {
            if (storedIds.Contains(data.Id))
            {
                Console.WriteLine("Skip: {0}", data.Id);
                return;
            }

            object[] values = new object[6];
            values[0] = data.Date.ToString("yyyy-MM-dd");
            values[1] = data.Id;
            values[2] = data.Type;
            values[3] = data.TrimpPoints > 0 ? (double?)data.TrimpPoints : null;
            values[4] = data.TriScorePoints > 0 ? (double?)data.TriScorePoints : null;
            values[5] = data.TssPoints > 0 ? (double?)data.TssPoints : null;

            googleApi.Write(new List<IList<object>>() { values });
        }

        public DateTime? GetLastDate()
        {
            return this.googleApi.GetLast()?.Item1;
        }

    }
}
