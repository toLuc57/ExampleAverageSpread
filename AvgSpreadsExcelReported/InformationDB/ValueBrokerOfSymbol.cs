using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvgSpreadsExcelReported.InformationDB
{
    public class ValueBrokerOfSymbol
    {
        public string brokerName;
        public double value;

        public ValueBrokerOfSymbol(string brokerName, double value)
        {
            this.brokerName = brokerName;
            this.value = value;
        }
    }
}
