using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvgSpreadsExcelReported.InformationDB
{
    class FormatRowEcxel
    {
        public List<ValueBrokerOfSymbol> brokers = new List<ValueBrokerOfSymbol>();

        public string symbolName;
        public double minValue = double.MaxValue;

        public FormatRowEcxel(string symbolName, ValueBrokerOfSymbol broker)
        {
            this.symbolName = symbolName;
            if (!brokers.Contains(broker))
            {
                brokers.Add(broker);
            }
        }
        public double GetMinValue()
        {

            foreach (var i in brokers)
            {
                if (minValue > i.value)
                {
                    minValue = i.value;
                }
            }
            return minValue;
        }
        
    }
}
