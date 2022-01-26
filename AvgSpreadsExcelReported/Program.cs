using AvgSpreadsExcelReported.InformationDB;
using CT;
using CT.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvgSpreadsExcelReported
{
    class Program
    {
        static void Main(string[] args)
        {
            new MySql.Data.MySqlClient.MySqlConnection().Dispose();
            LogConsole logConsole = LogConsole.Create(LogConsoleFlags.DefaultShort);
            logConsole.DateTimeFormat = "dd HH:mm:ss.fff";
            logConsole.Level = LogLevel.Information;

            logConsole.LogInfo("---");
            DateTime dataToday = DateTime.UtcNow.LastDay();
            DateTime dateToFile = DateTime.UtcNow;
            FormatShellEcxel reportToday = new FormatShellEcxel(dataToday,dateToFile);

            logConsole.LogInfo("Done");
            Console.ReadLine();
        }
    }
}
