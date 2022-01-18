using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvgSpreadsExcelReported.InformationDB
{
    class FormatShellEcxel 
    {
        public static DateTime date = new DateTime(2018, 12, 3);

        public static List<string> rowsName = new List<string>();
        public static List<string> columnsName = new List<string>();

        private static List<FormatRowEcxel> sheet1 = new List<FormatRowEcxel>();
        private static List<FormatRowEcxel> sheet2 = new List<FormatRowEcxel>();
        private static List<FormatRowEcxel> sheet3 = new List<FormatRowEcxel>();

        public static List<FormatRowEcxel>[] list = new List<FormatRowEcxel>[] { sheet1, sheet2, sheet3 };

    }
}
