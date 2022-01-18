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
        private static IDatabase database;
        private static ITable<Averagespreads> tAveragespreads;
        private static ITable<Symbols> tSymbols;
        private static List<OutputTableOfBroker> listOutputTables = new List<OutputTableOfBroker>();

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
            tSymbols = database.GetTable<Symbols>(TableFlags.AllowCreate);
            for(int i=0; i < ReadIniProgram.outTables.Count; ++i)
            {
                string nameTable = ReadIniProgram.outTables.ElementAt(i);
                string nameBroker = ReadIniProgram.listGBEBroker.ElementAt(i);
                listOutputTables.Add(new OutputTableOfBroker()
                {
                    name = nameBroker,
                    table = database.GetTable<WriteTableDB>(TableFlags.AllowCreate,nameTable)                    
                });
            }

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
            DateTime startTime = FormatShellEcxel.date.Date;
            startTime = startTime.Add(parameter[0]);
            DateTime endTime = FormatShellEcxel.date.Date;
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
                    //this.LogAlert("\t\t {0}\t\t{1}\t\t{2}", row.Symbol, row.Avarage, row.Count);
                    if (FormatShellEcxel.list[j].Exists(par => par.symbolName == row.Symbol))
                    {
                        FormatShellEcxel.list[j].Find(par => par.symbolName == row.Symbol).brokers.
                            Add(new ValueBrokerOfSymbol(eachBroker, row.Avarage));
                    }
                    else
                    {
                        FormatShellEcxel.list[j].Add(new FormatRowEcxel(row.Symbol, new ValueBrokerOfSymbol(eachBroker, row.Avarage)));
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
                Query(FormatShellEcxel.columnsName, i, ++j);
            }

        }
        #endregion
        private static void GetSymbolsAndBrokers()
        {
            FormatShellEcxel.columnsName.AddRange(ReadIniProgram.listGBEBroker);
            if (ReadIniProgram.exportNonGBE == true && (ReadIniProgram.otherBroker.Count == 0 || ReadIniProgram.otherBroker == null))
            {
                DateTime startTime = FormatShellEcxel.date.Date;
                DateTime endTime = FormatShellEcxel.date.Date.AddDays(1);

                Search search = Search.FieldGreaterOrEqual(nameof(Averagespreads.TimeStamp), startTime) 
                    & Search.FieldSmallerOrEqual(nameof(Averagespreads.TimeStamp),endTime);
                foreach(var i in ReadIniProgram.listGBEBroker)
                {
                    search = search & Search.FieldNotEquals(nameof(Averagespreads.BrokerName),i);
                }
                var list1 = tAveragespreads.GetStructs(search).GroupBy(par => par.BrokerName).Select(par => par.Key);
                FormatShellEcxel.columnsName.AddRange(list1);
            }
            else
            {
                FormatShellEcxel.columnsName.AddRange(ReadIniProgram.otherBroker);
            }

            var list = tSymbols.GetStructs().Select(par => par.currencypairname);
            FormatShellEcxel.rowsName.AddRange(list);
        }
        public void GetExcel()
        {
            string informationColumns = "\t";
            foreach (var str in FormatShellEcxel.columnsName)
            {
                informationColumns += str + "\t";
            }
            this.LogAlert(informationColumns);
        
            for (int i = 0; i < ReadIniProgram.allTimeSpans.Count; ++i)
            {
                this.LogAlert("-------------{0}---------------", ReadIniProgram.allTimeSpans[i]);
                foreach (var eachRow in FormatShellEcxel.list[i])
                {
                    string informationValues = eachRow.symbolName + ": ";
                    bool getYellowFont = true;
                    foreach (var eachColunmName in FormatShellEcxel.columnsName)
                    {
                        var cell = eachRow.brokers.Where(par => par.brokerName == eachColunmName).FirstOrDefault();
                        if(cell == null)
                        {
                            continue;
                        }
                        double value = cell.value;
                        if (getYellowFont && value == eachRow.GetMinValue())
                        {
                            informationValues += "<yellow>" + Math.Round(value, 5) + "<default>\t";
                            cell.isMin = true;
                            getYellowFont = false;
                        }
                        else
                        {
                            informationValues += Math.Round(value, 5) + "\t";
                        }

                        if (ReadIniProgram.listGBEBroker.Contains(cell.brokerName))
                        {
                            var updateTable = listOutputTables.Where(par => par.name.Equals(cell.brokerName)).
                                                                                    Select(par => par.table).First();
                            
                            if(updateTable.Exist(nameof(WriteTableDB.Symbol), eachRow.symbolName))
                            {
                                var updateRow = updateTable.GetStruct(nameof(WriteTableDB.Symbol), eachRow.symbolName);
                                if(updateRow.SpreadAvg > value)
                                {
                                    updateRow.SpreadAvg = value;
                                    updateRow.TimeStamp = DateTime.Now;
                                    updateTable.Update(updateRow);                                    
                                }
                            }
                            else
                            {

                                WriteTableDB insertRow = new WriteTableDB()
                                {
                                    TimeStamp = DateTime.Now,
                                    BrokerName = cell.brokerName,
                                    Symbol = eachRow.symbolName,
                                    SpreadAvg = value
                                };
                                var value1 =  updateTable.Insert(insertRow);
                            }
                        }
                    }
                    this.LogAlert(informationValues);
                }
            }
        }
    }
}
