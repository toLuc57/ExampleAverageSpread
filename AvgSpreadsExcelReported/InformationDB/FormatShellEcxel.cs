using OfficeOpenXml;
using OfficeOpenXml.Style;
using OfficeOpenXml.Table;
using System;
using System.Collections.Generic;
using System.IO;
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

        public static void CreateExcelFile(Stream stream = null)
        {
            var file = new FileInfo("myWorkbook.xlsx");
            using (var excelPackage = new ExcelPackage(file))
            {
                List<string> listHeaderName = new List<string>();
                foreach(string i in ReadIniProgram.allTimeSpansName)
                {
                    string[] splitString = i.Split('T');
                    string name = splitString[0];
                    listHeaderName.Add(name);
                }
                for(int i = excelPackage.Workbook.Worksheets.Count; i < 3; ++i)
                {
                    excelPackage.Workbook.Worksheets.Add("Sheet " + i.ToString());
                }
                {
                    excelPackage.Workbook.Worksheets[1].Name = listHeaderName.ElementAt(0) + " Session";
                    excelPackage.Workbook.Worksheets[2].Name = listHeaderName.ElementAt(1) + " Session";
                    excelPackage.Workbook.Worksheets[3].Name = listHeaderName.ElementAt(2) + " Session";
                }
                for(int k = 0; k < 3; ++k)
                {
                    var worksheet = excelPackage.Workbook.Worksheets[k+1];
                    int countRowsEqual0 = rowsName.Count == 0 ? 0 : 1;
                    int countColumnsEqual0 = columnsName.Count == 0 ? 0 : 1;

                    worksheet.Cells["a1:p2"].Merge = true;
                    worksheet.Cells["a1:p2"].Style.Font.Bold = true;
                    worksheet.Cells["a1:p2"].Style.Font.UnderLine = true;
                    worksheet.Cells["a1:p2"].Style.Font.Size = 14;
                    worksheet.Cells["a1:p2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells["a1:p2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    worksheet.Cells["a1"].Value = "Average Spread " + date.ToShortDateString() + " " + listHeaderName.ElementAt(k) + " Session";

                    worksheet.Cells[4, 2, 4, 2 + columnsName.Count - countColumnsEqual0].Merge = true;
                    worksheet.Cells[4, 2, 4, 2 + columnsName.Count].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[4, 2, 4, 2 + columnsName.Count - countRowsEqual0].Style.Border.BorderAround(ExcelBorderStyle.Thick);
                    worksheet.Cells[4, 2].Value = "Yesterday's average spreads";

                    worksheet.Cells["a5"].Value = "Symbol";
                    worksheet.Cells["a5"].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    worksheet.Cells["a6"].LoadFromCollection(rowsName);
                    worksheet.Cells[5, 1, 5 + rowsName.Count, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    for (int i = 0; i < columnsName.Count; ++i)
                    {
                        string brokerColumnName = columnsName.ElementAt(i);
                        worksheet.Cells[5, i + 2].Value = brokerColumnName;
                        worksheet.Cells[5, i + 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        worksheet.Cells[5, i + 2].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    }

                    worksheet.Cells[5, 2, 5 + rowsName.Count, 2 + columnsName.Count - countColumnsEqual0].
                                    Style.Border.BorderAround(ExcelBorderStyle.Thick);
                    int countGBEBrokers = ReadIniProgram.listGBEBroker.Count;
                    worksheet.Cells[6, 2 + countGBEBrokers, 6 + rowsName.Count - countRowsEqual0, 2 + countGBEBrokers].
                                    Style.Border.Left.Style = ExcelBorderStyle.Thick;

                    for (int i = 0; i < rowsName.Count; ++i)
                    {
                        string symbolNameInExcel = worksheet.Cells[i + 6, 1].Value.ToString();
                        var listBrokers = list[k].Where(par => par.symbolName.Equals(symbolNameInExcel)).
                                                Select(par => par.brokers).FirstOrDefault();
                        worksheet.Cells[i + 6, 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        worksheet.Cells[i + 6, 1].Style.Font.Bold = true;
                        worksheet.Cells["A:P"].AutoFitColumns(12.33);

                        for (int j = 0; j < columnsName.Count; ++j)
                        {
                            string brokerNameInExcel = worksheet.Cells[5, 2 + j].Value.ToString();
                            var informationCell = listBrokers.Where(par => par.brokerName.Equals(brokerNameInExcel)).
                                                            Select(par => new { isMin = par.isMin, value = par.value }).FirstOrDefault();

                            if (informationCell != null)
                            {
                                worksheet.Cells[i + 6, 2 + j].Style.Numberformat.Format = "0.00";
                                worksheet.Cells[i + 6, 2 + j].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                if (informationCell.isMin == true)
                                {
                                    worksheet.Cells[i + 6, 2 + j].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                    worksheet.Cells[i + 6, 2 + j].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);
                                }
                                worksheet.Cells[i + 6, 2 + j].Value = informationCell.value;
                            }
                        }
                    }
                }               

                excelPackage.Save();
            }
        }
    }

}
