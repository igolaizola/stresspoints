using Newtonsoft.Json;
using StressPoints.Config;
using StressPoints.Output;
using System;
using System.IO;

namespace StressPoints
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Stress Points Generator");

            DateTime defaultStartDate = new DateTime(2017, 8, 1);

            ProgramConfig config = JsonConvert.DeserializeObject<ProgramConfig>(File.ReadAllText(Path.Combine("conf","config.json")));

            // Athlete parameters
            AthleteConfig athleteConfig = config.athlete;

            // Strava token
            string stravaToken = config.strava.token;

            // Configuration for Google Sheet
            string googleAppName = config.google.appName;
            string sheetId = config.google.sheetId;
            GoogleSheetOutput output = new GoogleSheetOutput(googleAppName, sheetId);
            
            // Try get last date from google sheet data
            DateTime? startDate = output.GetLastDate() ?? defaultStartDate;

            PointsGenerator generator = new PointsGenerator.Builder()
                .WithAthleteConfig(athleteConfig)
                .WithStravaToken(stravaToken)
                .WithOutput(output)
                .WithStartDate(startDate.Value)
                .Build();

            generator.Run();

            Console.WriteLine("Finished!");
            Console.ReadLine();
        }
        
    }
}