using CT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AvgSpreadsExcelReported.InformationDB
{
    static class ReadIniProgram 
    {
        public static string connectionSTR;
        public static List<TimeSpan[]> allTimeSpans;
        public static string[] allTimeSpansName = {"AsianTradingSession","LondonTradingSession","USTradingSession" };
        public static List<string> listGBEBroker;
        public static List<string> otherBroker = null;
        public static bool exportNonGBE;

        public static void Read(Ini Iniprogram)
        {
            connectionSTR = Iniprogram.ReadString("Settings", "DatabaseConnection");
            allTimeSpans = new List<TimeSpan[]>();
            
            allTimeSpans.Add(ParseSessionTime(Iniprogram.ReadString("Settings", allTimeSpansName[0])));
            allTimeSpans.Add(ParseSessionTime(Iniprogram.ReadString("Settings", allTimeSpansName[1])));
            allTimeSpans.Add(ParseSessionTime(Iniprogram.ReadString("Settings", allTimeSpansName[2])));

            exportNonGBE = Iniprogram.ReadBool("Settings", "ExportNonGBE");
            listGBEBroker = new List<string>(Iniprogram.ReadSection("GBEBrokers"));
            if(exportNonGBE == true)
            {
                otherBroker = new List<string>(Iniprogram.ReadSection("OtherBrokers"));
            }
        }
        private static TimeSpan[] ParseSessionTime(string timeString)
        {
            TimeSpan[] sessionTime = new TimeSpan[2];
            //hh:mm-hh:mm
            string pattern = @"^\s*(?<StartHour>\d{1,2})\s*:(?<StartMinute>\d{1,2})\s*-" +
                @"\s*(?<EndHour>\d{1,2})\s*:\s*(?<EndMinute>\d{1,2})\s*$";
            Regex regex = new Regex(pattern);
            Match match = regex.Match(timeString);

            if (match.Success)
            {
                sessionTime[0] = TimeSpan.FromHours(int.Parse(match.Groups["StartHour"].Value))
                    .Add(TimeSpan.FromMinutes(int.Parse(match.Groups["StartMinute"].Value)));
                sessionTime[1] = TimeSpan.FromHours(int.Parse(match.Groups["EndHour"].Value))
                    .Add(TimeSpan.FromMinutes(int.Parse(match.Groups["EndMinute"].Value)));
            }
            return sessionTime;
        }
        public static void ShowTimeSpan()
        {
            int j = 0;
            foreach(var i in allTimeSpans)
            {
                Console.WriteLine("{0}: {1}-{2}", allTimeSpansName[j++], i[0],i[1]);
            }
        }
        
    }
}
