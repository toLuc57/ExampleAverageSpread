using AvgSpreadsExcelReported.InformationTable;
using CT;
using CT.Data;
using CT.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AvgSpreadsExcelReported.InformationDB
{
    class DBSpread : ILogSource 
    {
        IDatabase database;
        ITable<Averagespreads> tAveragespreads;

        private static TimeSpan[] asianTradingSession;
        private static TimeSpan[] londonTradingSession;
        private static TimeSpan[] usTradingSession;

        public string LogSourceName
        {
            get
            {
                return "ClassDBSpead";
            }
        }

        public DBSpread(Ini programIni)
        {
            string connectionSTR = programIni.ReadString("Settings", "DatabaseConnection");

            asianTradingSession = ParseSessionTime(programIni.ReadString("Settings", "AsianTradingSession"));
            londonTradingSession = ParseSessionTime(programIni.ReadString("Settings", "LondonTradingSession"));
            usTradingSession = ParseSessionTime(programIni.ReadString("Settings", "USTradingSession"));

            database = Connector.ConnectDatabase(connectionSTR, DbConnectionOptions.AllowCreate);
            tAveragespreads = database.GetTable<Averagespreads>(TableFlags.AllowCreate);
        }

        public static TimeSpan[] ParseSessionTime(string timeString)
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
        public void ShowTimeSpan()
        {
            this.LogAlert("AsianTradingSession: {0}-{1}", asianTradingSession[0], asianTradingSession[1]);
            this.LogAlert("LondonTradingSession: {0}-{1}", londonTradingSession[0], londonTradingSession[1]);
            this.LogAlert("USTradingSession: {0}-{1}", usTradingSession[0], usTradingSession[1]);
        }

        public void QueryAll()
        {

            List<Averagespreads> list = (List<Averagespreads>)tAveragespreads.GetStructs();

            this.LogAlert("==================================================================");
            this.LogAlert("BrokerName\t\tSymbol\t\t\tDateUpdated");

            foreach (var row in list)
            {
                this.LogAlert("{0}\t\t{1}\t\t{2}", row.BrokerName, row.Symbol, row.TimeStamp);
            }
            this.LogAlert("===============================Done===============================");
        }

        private void Query(TimeSpan[] parameter)
        {
            DateTime startTime = new DateTime(2018, 7, 2);
            startTime = startTime.Add(parameter[0]);
            DateTime endTime = new DateTime(2018, 7, 2);
            endTime = endTime.Add(parameter[1]);

            Search search = Search.FieldGreaterOrEqual(nameof(Averagespreads.TimeStamp),startTime) &
                Search.FieldSmallerOrEqual(nameof(Averagespreads.TimeStamp),endTime);  
                    
            List<Averagespreads> list = (List<Averagespreads>)tAveragespreads.GetStructs(search);

            var result = list.GroupBy(par => new { par.BrokerName, par.Symbol },
                (key, groups) => new
                {
                    BrokerName = key.BrokerName,
                    Symbol = key.Symbol,
                    Avarage = groups.Average(par => par.AvgSpread),
                    Count = groups.Count()
                });

            this.LogAlert("==================================================================");
            this.LogAlert("{0}",startTime);
            this.LogAlert("{0}", endTime);
            this.LogAlert("Total: " + result.Count());

            this.LogAlert("BrokerName\t\tSymbol\t\tAvg\t\tCount");

            foreach(var row in result)
            {
                this.LogAlert("{0}\t\t{1}\t\t{2}\t\t{3}", row.BrokerName, row.Symbol, row.Avarage, row.Count);
            }
            this.LogAlert("===============================Done===============================");
        }
        public void Query()
        {
            Query(asianTradingSession);
            Query(londonTradingSession);
            Query(usTradingSession);
        }
        
    }
}
