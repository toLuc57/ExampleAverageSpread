using CT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvgSpreadsExcelReported.InformationTable
{
    [Table("averagespreads")]
    public struct Averagespreads
    {
        [Field(Flags = FieldFlags.ID | FieldFlags.AutoIncrement)]
        public long ID;
        [Field(Flags = FieldFlags.Index)]
        [DateTimeFormat(DateTimeKind.Utc, DateTimeType.BigIntHumanReadable)]
        public DateTime TimeStamp;
        [Field]
        public int Duration ;
        [Field]
        public string BrokerName ;
        [Field]
        public string Symbol ;
        [Field]
        public double AvgSpread ;
    }
}
