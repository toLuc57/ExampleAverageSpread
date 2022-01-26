using AvgSpreadsExcelReported.InformationTable;
using CT.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvgSpreadsExcelReported.InformationDB
{
    public class OutputTableOfBroker
    {
        public string name;
        public ITable<LiveQuote> table;
        //Symbol: Key. Value: Element
        public List<string> listSymbol = new List<string>();
        private List<string> listNotUpdate = new List<string>(); 

        public OutputTableOfBroker(string name, ITable<LiveQuote> table)
        {
            this.name = name;
            this.table = table;
            var list = table.GetStructs().Select(par => par.Symbol);
            listSymbol.AddRange(list);
            listNotUpdate.AddRange(list);
        }
        public void setValueOfSymbol(string symbol, double value)
        {
            if (listSymbol.Contains(symbol))
            {
                Search search = Search.FieldEquals(nameof(LiveQuote.Symbol), symbol);
                var row = table.GetStruct(search);
                if(listNotUpdate.Contains(symbol))
                {

                    row.SpreadAvg = value;
                    listNotUpdate.Remove(symbol);
                }
                else
                {
                    if(row.SpreadAvg > value)
                    {
                        row.SpreadAvg = value;
                    }
                }
                row.TimeStamp = DateTime.Now;
                table.Update(row);            
            }
        }
    }
}
