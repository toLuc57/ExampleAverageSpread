using CT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvgSpreadsExcelReported.InformationTable
{
    [Table("symbols")]
    public struct Symbols
    {
        [Field(Flags = FieldFlags.ID | FieldFlags.AutoIncrement)]
        public long ID;
        [Field]
        public string currencypairname;
        [Field]
        public string requestId;
        [Field]
        public int Digit;
        [Field]
        public bool LiveQuotes;
    }
}
