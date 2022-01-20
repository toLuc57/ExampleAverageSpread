using AvgSpreadsExcelReported.InformationTable;
using CT.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvgSpreadsExcelReported.InformationDB
{
    public struct OutputTableOfBroker
    {
        public string name;
        public ITable<LiveQuote> table;
    }
}
