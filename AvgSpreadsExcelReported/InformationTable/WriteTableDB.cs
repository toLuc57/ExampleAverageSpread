using CT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvgSpreadsExcelReported.InformationTable
{
    [Table]
    public struct WriteTableDB
    {
        [Field (Flags = FieldFlags.ID)]
        public long ID;
        [Field(Flags = FieldFlags.Index)]
        [DateTimeFormat(DateTimeKind.Utc, DateTimeType.BigIntHumanReadable)]
        public DateTime TimeStamp;
        [Field]
        public string BrokerName;
        [Field(Flags = FieldFlags.Index)]
        public string Symbol;
        [Field]
        public double Bid;
        [Field]
        public double Ask;
        [Field]
        public double Spread;
        [Field]
        public double SpreadAvg;
    }
}
