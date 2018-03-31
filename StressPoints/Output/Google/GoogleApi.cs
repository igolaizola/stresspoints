using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace StressPoints
{
    public class GoogleApi
    {
        static string[] Scopes = { SheetsService.Scope.Spreadsheets };
    
        private string appName;
        private string sheetId;
        private SheetsService service;

        public GoogleApi(string appName, string sheetId)
        {
            this.appName = appName;
            this.sheetId = sheetId;
            service = AuthorizeGoogleApp(); 
        }

        public void Write(IList<IList<Object>> values)
        {
            string newRange = GetRange();
            Update(values, newRange);
        }

        public Tuple<DateTime, long> GetLast()
        {
            String range = "B2:C";
            SpreadsheetsResource.ValuesResource.GetRequest getRequest =
                       service.Spreadsheets.Values.Get(sheetId, range);

            ValueRange getResponse = getRequest.Execute();
            IList<IList<Object>> getValues = getResponse.Values;
            if (getValues == null)
                return null;

            long id = 0;
            DateTime dt;
            if (!long.TryParse(getValues.Last()[1].ToString(), out id) || !DateTime.TryParse(getValues.Last()[0].ToString(), out dt))
                return null;

            return new Tuple<DateTime, long>(dt, id);
        }

        public HashSet<long> GetIds()
        {
            String range = "C2:C";
            SpreadsheetsResource.ValuesResource.GetRequest getRequest =
                       service.Spreadsheets.Values.Get(sheetId, range);

            HashSet<long> ids = new HashSet<long>();
            ValueRange getResponse = getRequest.Execute();
            IList<IList<Object>> getValues = getResponse.Values;
            if (getValues == null)
                return ids;

            foreach(var v in getValues)
            {
                long id = 0;
                if (long.TryParse(v[0].ToString(), out id))
                    ids.Add(id);
            }

            return ids;
        }

        private SheetsService AuthorizeGoogleApp()
        {
            UserCredential credential;
            using (var stream = new FileStream(Path.Combine("conf","client_secret.json"), FileMode.Open, FileAccess.Read))
            {
                string credPath = Directory.GetCurrentDirectory();
                credPath = Path.Combine(credPath, ".credentials/sheets.googleapis.com-dotnet-quickstart.json");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Google Sheets API service.
            var service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = this.appName,
            });

            return service;
        }


        private string GetRange()
        {
            String range = "B2:B";
            SpreadsheetsResource.ValuesResource.GetRequest getRequest =
                       service.Spreadsheets.Values.Get(sheetId, range);

            ValueRange getResponse = getRequest.Execute();
            IList<IList<Object>> getValues = getResponse.Values;

            int currentCount = getValues != null ? getValues.Count + 2 : 1;

            String newRange = "B" + currentCount + ":B";

            return newRange;
        }


        private void Update(IList<IList<Object>> values, string newRange)
        {
            SpreadsheetsResource.ValuesResource.AppendRequest request =
               service.Spreadsheets.Values.Append(new ValueRange() { Values = values }, sheetId, newRange);
            request.InsertDataOption = SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.INSERTROWS;
            request.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
            var response = request.Execute();
        }
    }
}
