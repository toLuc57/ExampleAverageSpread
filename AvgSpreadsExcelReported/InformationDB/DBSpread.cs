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
        private IDatabase database;
        private ITable<Averagespreads> tAveragespreads;
        private ITable<Symbols> tSymbols;
        private List<OutputTableOfBroker> listOutputTables = new List<OutputTableOfBroker>();
        private FormatShellEcxel report;      

        public string LogSourceName
        {
            get
            {
                return "ClassDBSpead";
            }
        }

        public DBSpread(Ini programIni,FormatShellEcxel report)
        {
            ReadIniProgram.Read(programIni);
            database = Connector.ConnectDatabase(ReadIniProgram.connectionSTR, DbConnectionOptions.AllowUnsafeConnections);
            tAveragespreads = database.GetTable<Averagespreads>(TableFlags.AllowCreate, "AverageSpreads");
            tSymbols = database.GetTable<Symbols>(TableFlags.AllowCreate, "Symbols");
            for (int i=0; i < ReadIniProgram.outTables.Count; ++i)
            {
                string nameTable = ReadIniProgram.outTables.ElementAt(i);
                string nameBroker = ReadIniProgram.listGBEBroker.ElementAt(i);
                ITable<LiveQuote> table = database.GetTable<LiveQuote>(TableFlags.AllowCreate, nameTable);
                listOutputTables.Add(new OutputTableOfBroker(nameBroker, table));
            }
            this.report = report;
            GetSymbolsAndBrokers();
            Query();
        }
        #region Querys
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

        private void Query(List<string> listBrokers,TimeSpan[] parameter, int j)
        {
            DateTime startTime = report.date.Date;
            startTime = startTime.Add(parameter[0]);
            DateTime endTime = report.date.Date;
            endTime = endTime.Add(parameter[1]);

            //this.LogAlert("======================= {0} ============================", ReadIniProgram.allTimeSpansName[j]);
            //this.LogAlert("\t{0}", startTime);
            //this.LogAlert("\t{0}", endTime);

            foreach (var eachBroker in listBrokers)
            {
                Search search = Search.FieldGreaterOrEqual(nameof(Averagespreads.TimeStamp), startTime) &
                                Search.FieldSmallerOrEqual(nameof(Averagespreads.TimeStamp), endTime) & 
                                Search.FieldEquals(nameof(Averagespreads.BrokerName), eachBroker);

                List<Averagespreads> list = (List<Averagespreads>)tAveragespreads.GetStructs(search);

                //this.LogAlert("\t=== BrokerName: {0} ===", eachBroker);
                //if (list != null && list.Count > 0)
                //{
                //    this.LogAlert("\t\t Symbol\t\tAvg\t\tCount");
                //}
                //else
                //{
                //    this.LogAlert("\t\t Is null");
                //}
                var result = list.GroupBy(par => par.Symbol,
                    (key, groups) => new
                    {
                        Symbol = key,
                        Avarage = groups.Average(par => par.AvgSpread),
                        Count = groups.Count()
                    });

                foreach (var row in result)
                {
                    if (ReadIniProgram.listGBEBroker.Contains(eachBroker))
                    {
                        var outputListInTable = listOutputTables.Where(par => par.name.Equals(eachBroker)).FirstOrDefault();
                        if (outputListInTable != null)
                        {
                            outputListInTable.setValueOfSymbol(row.Symbol,row.Avarage);
                        }
                    }

                    //this.LogAlert("\t\t {0}\t\t{1}\t\t{2}", row.Symbol, row.Avarage, row.Count);
                    if (report.list[j].Exists(par => par.symbolName == row.Symbol))
                    {
                        report.list[j].Find(par => par.symbolName == row.Symbol).brokers.
                            Add(new ValueBrokerOfSymbol(eachBroker, row.Avarage));
                    }
                    else
                    {
                        report.list[j].Add(new FormatRowEcxel(row.Symbol, new ValueBrokerOfSymbol(eachBroker, row.Avarage)));
                    }
                }
                //this.LogAlert("\t=== End BrokerName: {0} ===", eachBroker);
            }
            //this.LogAlert("===============================<yellow> Done <default>===============================");
        }
        public void Query()
        {
            //int j = -1;
            //foreach(var i in ReadIniProgram.allTimeSpans)
            //{
            //    Query(ReadIniProgram.listGBEBroker,i,++j);
            //}
            //if(ReadIniProgram.otherBroker != null && ReadIniProgram.otherBroker.Count != 0)
            //{
            //    j = -1;
            //    foreach (var i in ReadIniProgram.allTimeSpans)
            //    {
            //        Query(ReadIniProgram.otherBroker, i, ++j);
            //    }
            //}
            int j = -1;
            foreach (var i in ReadIniProgram.allTimeSpans)
            {
                Query(report.columnsName, i, ++j);
            }

        }
        #endregion
        private void GetSymbolsAndBrokers()
        {
            report.columnsName.AddRange(ReadIniProgram.listGBEBroker);
            if (ReadIniProgram.exportNonGBE == true && (ReadIniProgram.otherBroker.Count == 0 || ReadIniProgram.otherBroker == null))
            {
                DateTime startTime = report.date.Date;
                DateTime endTime = report.date.Date.AddDays(1);

                Search search = Search.FieldGreaterOrEqual(nameof(Averagespreads.TimeStamp), startTime) 
                    & Search.FieldSmallerOrEqual(nameof(Averagespreads.TimeStamp),endTime);
                foreach(var i in ReadIniProgram.listGBEBroker)
                {
                    search = search & Search.FieldNotEquals(nameof(Averagespreads.BrokerName),i);
                }
                var list1 = tAveragespreads.GetStructs(search).GroupBy(par => par.BrokerName).Select(par => par.Key).OrderBy(par => par);
                report.columnsName.AddRange(list1);
            }
            else
            {
                report.columnsName.AddRange(ReadIniProgram.otherBroker.OrderBy(par=>par));
            }

            var list = tSymbols.GetStructs().Select(par => par.currencypairname);
            report.rowsName.AddRange(list);
        }
        public void GetExcel()
        {
            string informationColumns = "\t";
            foreach (var str in report.columnsName)
            {
                informationColumns += str + "\t";
            }
            this.LogInfo(informationColumns);
        
            for (int i = 0; i < ReadIniProgram.allTimeSpans.Count; ++i)
            {
                this.LogInfo("-------------{0}---------------", ReadIniProgram.allTimeSpans[i]);
                foreach (string eachRowName in report.rowsName)
                {
                    string informationValues = eachRowName + ": ";
                    var eachRow = report.list[i].Where(par => par.symbolName.Equals(eachRowName)).FirstOrDefault();
                    if (eachRow == null)
                    {
                        continue;
                    }
                    bool getYellowFont = true;
                    foreach (string eachColunmName in report.columnsName)
                    {
                        var cell = eachRow.brokers.Where(par => par.brokerName == eachColunmName).FirstOrDefault();
                        if(cell == null)
                        {
                            informationValues += "null \t";
                            continue;
                        }
                        double value = cell.value;
                        if (getYellowFont && value == eachRow.GetMinValue())
                        {
                            informationValues += "<yellow>" + Math.Round(value, 5) + "<default> \t";
                            cell.isMin = true;
                            getYellowFont = false;
                        }
                        else
                        {
                            informationValues += Math.Round(value, 5) + " \t";
                        }
                    }
                    this.LogInfo(informationValues);
                }
            }
        }
    }
}
