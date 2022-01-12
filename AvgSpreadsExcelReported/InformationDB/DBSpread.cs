using AvgSpreadsExcelReported.InformationTable;
using CT;
using CT.Data;
using CT.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvgSpreadsExcelReported.InformationDB
{
    class DBSpread : ILogSource 
    {
        IDatabase database;
        ITable<Averagespreads> tAveragespreads;


        public string LogSourceName
        {
            get
            {
                return "ClassDBSpead";
            }
        }

        public DBSpread(Ini programIni)
        {
            ReadIniProgram.Read(programIni);
            database = Connector.ConnectDatabase(ReadIniProgram.connectionSTR, DbConnectionOptions.AllowCreate);
            tAveragespreads = database.GetTable<Averagespreads>(TableFlags.AllowCreate);
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

        private void Query(TimeSpan[] parameter, int j)
        {
            DateTime startTime = new DateTime(2018, 7, 2);
            startTime = startTime.Add(parameter[0]);
            DateTime endTime = new DateTime(2018, 7, 2);
            endTime = endTime.Add(parameter[1]);

            this.LogAlert("======================= {0} ============================",ReadIniProgram.allTimeSpansName[j]);
            this.LogAlert("\t{0}",startTime);
            this.LogAlert("\t{0}", endTime);
            
            foreach (var eachBroker in ReadIniProgram.GBEBroker)
            {
                Search search = Search.FieldGreaterOrEqual(nameof(Averagespreads.TimeStamp), startTime) &
                                Search.FieldSmallerOrEqual(nameof(Averagespreads.TimeStamp), endTime) & 
                                Search.FieldEquals(nameof(Averagespreads.BrokerName), eachBroker);

                List<Averagespreads> list = (List<Averagespreads>)tAveragespreads.GetStructs(search);

                this.LogAlert("\t=== BrokerName: {0} ===",eachBroker);
                if(list != null && list.Count > 0)
                {
                    this.LogAlert("\t\t Symbol\t\tAvg\t\tCount");
                }
                else
                {
                    this.LogAlert("\t\t Is null");
                }
                var result = list.GroupBy(par => par.Symbol,
                    (key, groups) => new
                    {
                        Symbol = key,
                        Avarage = groups.Average(par => par.AvgSpread),
                        Count = groups.Count()
                    });

                foreach (var row in result)
                {
                    this.LogAlert("\t\t {0}\t\t{1}\t\t{2}", row.Symbol, row.Avarage, row.Count);
                }
                this.LogAlert("\t=== End BrokerName: {0} ===", eachBroker);
            }
            this.LogAlert("=============================== Done ===============================");
        }
        public void Query()
        {
            int j = -1;
            foreach(var i in ReadIniProgram.allTimeSpans)
            {
                Query(i,++j);
            }
        }
        
    }
}
