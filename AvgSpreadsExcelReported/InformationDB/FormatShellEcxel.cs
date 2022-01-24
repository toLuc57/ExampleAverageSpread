using CT;
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
        public DateTime date = new DateTime(2018, 12, 3);
        private string fileName;
        public List<string> rowsName = new List<string>();
        public List<string> columnsName = new List<string>();

        public List<FormatRowEcxel>[] list = new List<FormatRowEcxel>[] 
            {
                new List<FormatRowEcxel>(),
                new List<FormatRowEcxel>(),
                new List<FormatRowEcxel>()
            };
        public FormatShellEcxel()
        {
            fileName = "AverageSpreadsReport_" + date.Year + "." + date.Month + "." + date.Day;

            Ini programIni = Ini.ProgramIniFile;
            DBSpread readDB = new DBSpread(programIni,this);
            //logConsole.LogAlert("---");

            //readDB.Query();
            //logConsole.LogAlert("---");

            readDB.GetExcel();

            CreateExcelFile();
        }

        public FormatShellEcxel(DateTime date)
        {
            this.date = date;
            fileName = "AverageSpreadsReport_" + date.Year + "." + date.Month + "." + date.Day;

            Ini programIni = Ini.ProgramIniFile;
            DBSpread readDB = new DBSpread(programIni,this);
            //logConsole.LogAlert("---");

            //readDB.Query();
            //logConsole.LogAlert("---");

            readDB.GetExcel();

            CreateExcelFile();
        }

        public void CreateExcelFile()
        {
            var file = new FileInfo("reports//"+ fileName + ".xlsx");
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

                    worksheet.DefaultColWidth = 12.33;
                    
                    worksheet.Cells["a1"].Value = "Average Spread " + date.ToShortDateString() + " " + listHeaderName.ElementAt(k) + " Session";
                    worksheet.Cells[1, 1, 2, 1 + columnsName.Count + 2 + ReadIniProgram.listGBEBroker.Count + 1].Merge = true;
                    worksheet.Cells[1, 1, 2, 1 + columnsName.Count + 2 + ReadIniProgram.listGBEBroker.Count + 1].
                        Style.Font.Bold = true;
                    worksheet.Cells[1, 1, 2, 1 + columnsName.Count + 2 + ReadIniProgram.listGBEBroker.Count + 1].
                        Style.Font.UnderLine = true;
                    worksheet.Cells[1, 1, 2, 1 + columnsName.Count + 2 + ReadIniProgram.listGBEBroker.Count + 1].
                        Style.Font.Size = 14;
                    worksheet.Cells[1, 1, 2, 1 + columnsName.Count + 2 + ReadIniProgram.listGBEBroker.Count + 1].
                        Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[1, 1, 2, 1 + columnsName.Count + 1 + ReadIniProgram.listGBEBroker.Count + 1].
                        Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    worksheet.Cells[4, 2, 4, 2 + columnsName.Count - countColumnsEqual0].Merge = true;
                    worksheet.Cells[4, 2, 4, 2 + columnsName.Count].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[4, 2, 4, 2 + columnsName.Count - countRowsEqual0].Style.Border.BorderAround(ExcelBorderStyle.Thick);
                    worksheet.Cells[4, 2].Value = "Yesterday's average spreads";

                    worksheet.Cells["a5"].Value = "Symbol";
                    worksheet.Cells["a5"].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    worksheet.Cells["a6"].LoadFromCollection(rowsName);
                    worksheet.Cells[5, 1, 5 + rowsName.Count, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    // Show Broker's Name in Brokers
                    for (int i = 0; i < columnsName.Count; ++i)
                    {
                        string brokerColumnName = columnsName.ElementAt(i);
                        worksheet.Cells[5, i + 2].Value = brokerColumnName;
                        worksheet.Cells[5, i + 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        worksheet.Cells[5, i + 2].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    }
                    
                    //Show value (double) in each cell
                    for (int i = 0; i < rowsName.Count; ++i)
                    {
                        string symbolNameInExcel = worksheet.Cells[i + 6, 1].Value.ToString();
                        var listBrokers = list[k].Where(par => par.symbolName.Equals(symbolNameInExcel)).
                                                Select(par => par.brokers).FirstOrDefault();
                        worksheet.Cells[i + 6, 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        worksheet.Cells[i + 6, 1].Style.Font.Bold = true;
                        
                        for (int j = 0; j < columnsName.Count; ++j)
                        {
                            string brokerNameInExcel = worksheet.Cells[5, 2 + j].Value.ToString();
                            var informationCell = listBrokers.Where(par => par.brokerName.Equals(brokerNameInExcel)).
                                                            Select(par => new { isMin = par.isMin, value = par.value }).FirstOrDefault();
                            worksheet.Cells[i + 6, 2 + j].Style.Border.BorderAround(ExcelBorderStyle.Hair);
                            worksheet.Cells[i + 6, 2 + j].Style.Border.Left.Style = ExcelBorderStyle.Thin;
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
                    worksheet.Cells[5, 2, 5 + rowsName.Count, 2 + columnsName.Count - countColumnsEqual0].
                                    Style.Border.BorderAround(ExcelBorderStyle.Thick);
                    int countGBEBrokers = ReadIniProgram.listGBEBroker.Count;
                    worksheet.Cells[5, 2 + countGBEBrokers, 6 + rowsName.Count - countRowsEqual0, 2 + countGBEBrokers].
                                    Style.Border.Left.Style = ExcelBorderStyle.Thick;

                    //Show GBEBrokers
                    int indexWriteSymbol2nd = 3 + columnsName.Count;
                    worksheet.Cells[5, indexWriteSymbol2nd].Value = "Symbol";
                    worksheet.Cells[5, indexWriteSymbol2nd].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[5, indexWriteSymbol2nd].Style.Border.BorderAround(ExcelBorderStyle.Thin);

                    worksheet.Cells[4, indexWriteSymbol2nd + 1].Value = "XCore vs Xcore";
                    worksheet.Cells[4, indexWriteSymbol2nd + 1, 4, indexWriteSymbol2nd + ReadIniProgram.listGBEBroker.Count].
                        Style.Border.BorderAround(ExcelBorderStyle.Thick);
                    worksheet.Cells[4, indexWriteSymbol2nd + 1, 4, indexWriteSymbol2nd + ReadIniProgram.listGBEBroker.Count].
                        Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[4, indexWriteSymbol2nd + 1, 4, indexWriteSymbol2nd + ReadIniProgram.listGBEBroker.Count].Merge = true;

                    int count = 0;
                    foreach (var eachGBEBorker in ReadIniProgram.listGBEBroker)
                    {
                        worksheet.Cells[5, indexWriteSymbol2nd + (++count)].Value = eachGBEBorker;
                        worksheet.Cells[5, indexWriteSymbol2nd + count].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        worksheet.Cells[5, indexWriteSymbol2nd + count].Style.Font.Bold = true;
                        worksheet.Cells[5, indexWriteSymbol2nd + count].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    }
                    
                    List<FormatRowEcxel> listOfGBE = new List<FormatRowEcxel>();
                    foreach (var eachGBEBrokers in ReadIniProgram.listGBEBroker)
                    {
                        var temporaryList = list[k].Where(par =>
                        {
                            if (par.brokers.Find(par1 => par1.brokerName.Equals(eachGBEBrokers)) != null)
                            {
                                return true;
                            }
                            return false;
                        });
                        foreach (var eachSymbol in temporaryList)
                        {
                            if (!listOfGBE.Contains(eachSymbol))
                            {
                                listOfGBE.Add(eachSymbol);
                            }
                        }
                    }
                    count = 0;
                    foreach (var row in rowsName)
                    {
                        var cellsOfSymbol = listOfGBE.Where(par => par.symbolName.Equals(row)).FirstOrDefault();
                        if (cellsOfSymbol != null)
                        {
                            worksheet.Cells[6 + count, indexWriteSymbol2nd].Value = cellsOfSymbol.symbolName;
                            worksheet.Cells[6 + count, indexWriteSymbol2nd].Style.Font.Bold = true;
                            worksheet.Cells[6 + count, indexWriteSymbol2nd].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            worksheet.Cells[6 + count, indexWriteSymbol2nd].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                            int count1 = 1;
                            int indexRowOfMin = -1;
                            int indexColumnOfMin = -1;
                            double min = double.MaxValue; 
                            foreach(var eachGBEB in ReadIniProgram.listGBEBroker)
                            {
                                var informationCell = cellsOfSymbol.brokers.Where(par => par.brokerName.Equals(eachGBEB)).FirstOrDefault();
                                worksheet.Cells[6 + count, indexWriteSymbol2nd + count1].Style.HorizontalAlignment = 
                                    ExcelHorizontalAlignment.Center;
                                worksheet.Cells[6 + count, indexWriteSymbol2nd + count1].Style.Border.BorderAround(ExcelBorderStyle.Hair);
                                worksheet.Cells[6 + count, indexWriteSymbol2nd + count1].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                if (informationCell != null)
                                {
                                    worksheet.Cells[6 + count, indexWriteSymbol2nd + count1].Style.Numberformat.Format = "0.00";                                    
                                    worksheet.Cells[6 + count, indexWriteSymbol2nd + count1].Value = informationCell.value;
                                    if(min > informationCell.value)
                                    {
                                        min = informationCell.value;
                                        indexRowOfMin = 6 + count;
                                        indexColumnOfMin = indexWriteSymbol2nd + count1;
                                    }
                                }
                                ++count1;
                            }
                            if (indexRowOfMin > -1)
                            {
                                worksheet.Cells[indexRowOfMin, indexColumnOfMin].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                worksheet.Cells[indexRowOfMin, indexColumnOfMin].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);
                            }
                            ++count;
                        }

                    }
                    worksheet.Cells[5, indexWriteSymbol2nd + 1, 5 + count, indexWriteSymbol2nd + ReadIniProgram.listGBEBroker.Count].
                        Style.Border.BorderAround(ExcelBorderStyle.Thick);
                }               
                excelPackage.Save();
            }
        }
    }

}
