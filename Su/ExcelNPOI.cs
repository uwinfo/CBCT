using Microsoft.AspNetCore.Mvc;
using NPOI.HPSF;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Net.Http.Headers;

namespace Su
{
    public class ExcelNPOI
    {
        /// <summary>
        /// DtToExcel 時，日期的格式。
        /// </summary>
        public static string ExportDateTimeFormat { get; set; }

        /// <summary>
        /// 這個是給 .Net Framework 用的
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="fileName"></param>
        /// <param name="sheetName"></param>
        /// <param name="alFinalRow"></param>
        /// <param name="isNeedHyperLink"></param>
        /// <param name="dateTimeFormat"></param>
        /// <returns></returns>
        public static HttpResponseMessage DtToHttpResponseMessage(DataTable dt, string fileName, string sheetName = "Sheet1", ArrayList alFinalRow = null, bool isNeedHyperLink = false, string dateTimeFormat = null, List<List<string>> alHeaderRows = null)
        {
            var workbook = Su.ExcelNPOI.DtToWorkbook(dt, sheetName, alFinalRow, isNeedHyperLink, dateTimeFormat, alHeaderRows);

            MemoryStream stream = new MemoryStream();
            workbook.Write(stream, true);
            stream.Seek(0, SeekOrigin.Begin);

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StreamContent(stream);
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
            response.Content.Headers.ContentDisposition.FileName = fileName;
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

            return response;
        }

        public static FileStreamResult DtToFileStreamResult(DataTable dt, string fileName, string sheetName = "Sheet1", ArrayList alFinalRow = null, bool isNeedHyperLink = false, string dateTimeFormat = null, List<List<string>> alHeaderRows = null)
        {
            var workbook = Su.ExcelNPOI.DtToWorkbook(dt, sheetName, alFinalRow, isNeedHyperLink, dateTimeFormat, alHeaderRows);

            MemoryStream stream = new MemoryStream();
            workbook.Write(stream, true);
            stream.Seek(0, SeekOrigin.Begin);

            return new FileStreamResult(stream, "application/ms-excel")
            {
                FileDownloadName = fileName
            };
        }

        public static XSSFWorkbook DtToWorkbook(DataTable DT, string sheetName = "Sheet1", ArrayList alFinalRow = null, bool isNeedHyperLink = false, string dateTimeFormat = null, List<List<string>> alHeaderRows = null)
        {
            XSSFWorkbook wb = new XSSFWorkbook();

            DtToWorkSheet((XSSFSheet)wb.CreateSheet(sheetName), DT, alFinalRow, isNeedHyperLink, dateTimeFormat, true, alHeaderRows);

            return wb;
        }

        public static DataTable ExcelToDT(Stream stream, string SheetName = null, string DateFields = "", Int32 HeaderRowIndex = 0)
        {
            IWorkbook workbook = new XSSFWorkbook(stream);

            XSSFSheet sheet = null;
            if (SheetName == null)
            {
                sheet = (XSSFSheet)workbook.GetSheetAt(0);
            }
            else
            {
                sheet = (XSSFSheet)workbook.GetSheet(SheetName);
            }

            DataTable DT = new DataTable();
            XSSFRow headerRow = (XSSFRow)sheet.GetRow(HeaderRowIndex);
            int cellCount = headerRow.LastCellNum;
            Hashtable htDate = new Hashtable();
            bool checkDateFields = false;
            if (DateFields.Length > 0)
            {
                DateFields = "," + DateFields + ",";
                checkDateFields = true;
            }

            for (Int32 i = headerRow.FirstCellNum; i <= (cellCount - 1); i += 1)
            {
                DataColumn Column;
                string a = null;
                try
                {
                    if (headerRow.GetCell(i) != null)
                    {
                        a = headerRow.GetCell(i).StringCellValue;
                        Column = new DataColumn(a);
                        DT.Columns.Add(Column);
                        if (checkDateFields && DateFields.Contains("," + headerRow.GetCell(i).StringCellValue + ","))
                        {
                            htDate[i] = true;
                        }
                    }
                }
                catch (Exception)
                {
                    throw new Exception(a);
                }
            }

            if (HeaderRowIndex > 0)
            {
                XSSFRow titleRow = (XSSFRow)sheet.GetRow(0);
                DataRow dataRow = DT.NewRow();
                for (Int32 i = titleRow.FirstCellNum; i <= (cellCount - 1); i += 1)
                {
                    if (titleRow.GetCell(i) != null)
                    {
                        dataRow[i] = titleRow.GetCell(i).StringCellValue.ToString();
                    }
                    else
                    {
                        dataRow[i] = "";
                    }
                }

                DT.Rows.Add(dataRow);
            }

            int rowCount = sheet.LastRowNum;
            for (Int32 i = (sheet.FirstRowNum + 1); i <= (sheet.LastRowNum); i += 1)
            {
                XSSFRow row = (XSSFRow)sheet.GetRow(i);
                DataRow dataRow = DT.NewRow();
                if (row != null)
                {
                    for (Int32 j = row.FirstCellNum; j <= (cellCount - 1); j += 1)
                    {
                        try
                        {
                            if (!Convert.IsDBNull(row.GetCell(j)) && row.GetCell(j) != null)
                            {
                                NPOI.SS.UserModel.ICell cell = row.GetCell(j);
                                if (cell.CellType == CellType.Numeric && htDate.Contains(j))
                                {
                                    dataRow[j] = cell.DateCellValue.ToString("yyyy-MM-dd HH:mm:ss").Replace(" 00:00:00", "");
                                }
                                else if (cell.CellType == CellType.String)
                                {
                                    dataRow[j] = cell.StringCellValue;
                                }
                                else if (cell.CellType == NPOI.SS.UserModel.CellType.Formula && cell.CachedFormulaResultType == NPOI.SS.UserModel.CellType.String)
                                {
                                    dataRow[j] = cell.StringCellValue;
                                }
                                else if (cell.CellType == NPOI.SS.UserModel.CellType.Formula && cell.CachedFormulaResultType == NPOI.SS.UserModel.CellType.Numeric)
                                {
                                    dataRow[j] = cell.NumericCellValue;
                                }
                                else
                                {
                                    dataRow[j] = cell.ToString();
                                }
                            }
                        }
                        catch (Exception)
                        {
                        }
                    }

                    DT.Rows.Add(dataRow);
                }
            }

            return DT;
        }


        public static DataTable ExcelToDT(string filename, string SheetName = null, string DateFields = "", Int32 HeaderRowIndex = 0)
        {
            using var stream = new System.IO.FileStream(filename, FileMode.Open, FileAccess.Read);
            return ExcelToDT(stream, SheetName, DateFields, HeaderRowIndex);
        }

        public static CellType GetCellTypeByDataColumn(DataColumn oC)
        {
            switch (oC.DataType.Name)
            {
                case "Int32":
                case "Decimal":
                case "Int16":
                case "Int64":
                case "Integer":
                case "Single":
                case "Double":
                case "Byte":
                    {
                        return CellType.Numeric;
                    }

                case "String":
                case "DateTime":
                case "TimeSpan":
                case "Object":
                    {
                        return CellType.String;
                    }

                case "Boolean":
                    {
                        return CellType.Boolean;
                    }

                default:
                    {
                        throw new Exception("GetCellTypeByDataColumn: Unknow Type \"" + oC.DataType.Name + "\"");
                    }
            }
        }

        public static XSSFWorkbook DtToWorkbook(DataTable DT, string SheetName = "Sheet1", ArrayList alFinalRow = null, bool IsNeedHyperLink = false, List<List<string>> alHeaderRows = null)
        {
            XSSFWorkbook wb = new XSSFWorkbook();

            wb.CreateSheet(SheetName);

            var WS = wb.GetSheet(SheetName);

            DtToWorkSheet((XSSFSheet)WS, DT, alFinalRow, IsNeedHyperLink, alHeaderRows: alHeaderRows);

            return wb;
        }


        public static List<dynamic> GetListFromWorkbook(IWorkbook workbook, string sheetName)
        {

            var result = new List<dynamic>();
            XSSFSheet sheet = null;
            if (sheetName == null)
            {
                sheet = (XSSFSheet)workbook.GetSheetAt(0);
            }
            else
            {
                sheet = (XSSFSheet)workbook.GetSheet(sheetName);
            }

            for (int row = 2; row <= sheet.LastRowNum; row++)
            {
                var item = new System.Dynamic.ExpandoObject() as IDictionary<string, Object>;
                for (int col = 0; col < sheet.GetRow(0).LastCellNum; col++)
                {
                    NPOI.SS.UserModel.ICell cell = sheet.GetRow(row).GetCell(col);
                    string colName = sheet.GetRow(0).GetCell(col).ToString();
                    if (cell.CellType == CellType.Numeric)
                    {
                        item.Add(colName, cell.NumericCellValue);
                    }
                    else if (cell.CellType == CellType.String)
                    {
                        item.Add(colName, cell.StringCellValue);
                    }
                    else if (cell.CellType == NPOI.SS.UserModel.CellType.Formula && cell.CachedFormulaResultType == NPOI.SS.UserModel.CellType.String)
                    {
                        item.Add(colName, cell.StringCellValue);
                    }
                    else if (cell.CellType == NPOI.SS.UserModel.CellType.Formula && cell.CachedFormulaResultType == NPOI.SS.UserModel.CellType.Numeric)
                    {
                        item.Add(colName, cell.NumericCellValue);
                    }
                    else
                    {
                        item.Add(colName, cell.ToString());
                    }
                }

                result.Add(item);
            }
            return result;
        }

        public static CellType GetCellTypeByObject(object obj)
        {
            try
            {
                if (obj.GetType().ToString().ToLower().StartsWith("system.int") && obj.ToString().IsNumeric())
                {
                    return CellType.Numeric;
                }
            }
            catch (Exception)
            {
            }

            switch (obj.GetType().ToString().ToLower())
            {
                case "system.decimal":
                case "system.single":
                case "system.double":
                    {
                        return CellType.Numeric;
                    }

                default:
                    {
                        return CellType.String;
                    }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="WS"></param>
        /// <param name="DT"></param>
        /// <param name="alFinalRow"></param>
        /// <param name="isNeedHyperLink"></param>
        /// <param name="dateTimeFormat"></param>
        /// <param name="isAutoToDate">自動判斷 "日期" 欄位(無時分秒)，指定了 dateTimeFormat 時，仍然可以自動把日期欄位輸出為 yyyy-MM-dd 使用</param>
        /// <param name="alHeaderRows">只能塞純文字</param>
        public static void DtToWorkSheet(XSSFSheet WS, DataTable DT, ArrayList alFinalRow = null,
            bool isNeedHyperLink = false, string dateTimeFormat = null, bool isAutoToDate = true, List<List<string>> alHeaderRows = null)
        {
            var Count = 0; // 建立 Row 時要用的

            if (alHeaderRows != null)
            {
                foreach (List<string> rowlist in alHeaderRows)
                {
                    var myRow = WS.CreateRow(Count);
                    for (int i = 0; i < rowlist.Count; i++)
                    {
                        myRow.CreateCell(i).SetCellValue(rowlist[i]);
                    }
                    Count++;
                }
            }

            // 先建立表頭
            var WR = WS.CreateRow(Count);
            Count += 1;

            XSSFCellStyle customStyle = (XSSFCellStyle)WS.Workbook.CreateCellStyle();
            customStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Grey25Percent.Index;
            customStyle.FillPattern = FillPattern.SolidForeground;

            for (int Cell = 0; Cell <= DT.Columns.Count - 1; Cell++)
            {
                DataColumn oC = DT.Columns[Cell];
                WR.CreateCell(Cell).SetCellValue(oC.ColumnName);
                // 如果有 header rows 就加灰階背景 以區別
                if (alHeaderRows != null)
                {
                    WR.GetCell(Cell).CellStyle = customStyle;
                }
            }

            //轉為日期的欄位
            var toDateDictionary = new Dictionary<string, bool>();
            if (isAutoToDate)
            {
                for (int Cell = 0; Cell <= DT.Columns.Count - 1; Cell++)
                {
                    DataColumn oC = DT.Columns[Cell];
                    toDateDictionary.Add(oC.ColumnName, true); //一率先放 true
                    foreach (DataRow row in DT.Rows)
                    {
                        if (!Convert.IsDBNull(row[oC.ColumnName]))
                        {
                            if (oC.DataType.Name == "DateTime")
                            {
                                var d = (DateTime)row[oC.ColumnName];

                                if (d.Hour > 0 || d.Minute > 0 || d.Second > 0)
                                {
                                    //只要有一個的時分秒為非 0 值, 就改為 false，並不再檢查。
                                    toDateDictionary[oC.ColumnName] = false;
                                    break;
                                }
                            }
                            else
                            {
                                //非日期欄位
                                toDateDictionary[oC.ColumnName] = false;
                                break;
                            }
                        }
                    }
                }
            }

            // 資料的部份
            foreach (DataRow row in DT.Rows)
            {
                WR = WS.CreateRow(Count);

                for (int Cell = 0; Cell <= DT.Columns.Count - 1; Cell++)
                {
                    DataColumn oC = DT.Columns[Cell];

                    WR.CreateCell(Cell).SetCellType(GetCellTypeByDataColumn(oC));

                    if (GetCellTypeByDataColumn(oC) == CellType.Numeric)
                    {
                        if (!Convert.IsDBNull(row[oC.ColumnName]))
                        {
                            WR.GetCell(Cell).SetCellValue(System.Convert.ToDouble(row[oC.ColumnName]));
                        }
                    }
                    else
                    {
                        string CellText = "";
                        if (!Convert.IsDBNull(row[oC.ColumnName]))
                        {
                            if (oC.DataType.Name == "DateTime")
                            {
                                if (isAutoToDate && toDateDictionary[oC.ColumnName] == true)
                                {
                                    CellText = ((DateTime)row[oC.ColumnName]).ToString("yyyy-MM-dd");
                                }
                                if (!string.IsNullOrEmpty(dateTimeFormat))
                                {
                                    CellText = ((DateTime)row[oC.ColumnName]).ToString(dateTimeFormat);
                                }
                                else if (!string.IsNullOrEmpty(ExportDateTimeFormat))
                                {
                                    CellText = ((DateTime)row[oC.ColumnName]).ToString(ExportDateTimeFormat);
                                }
                                else
                                {
                                    CellText = ((DateTime)row[oC.ColumnName]).ToString("yyyy-MM-dd HH:mm:ss").Replace(" 00:00:00", "");
                                }
                            }
                            else
                            {
                                CellText = row[oC.ColumnName].ToString();
                            }
                        }

                        // 有 Enter 時, 讓 cell 要可以自動換行
                        if (CellText.Contains("\r") || CellText.Contains("\n"))
                        {
                            // 不能直接用 WR.GetCell(Cell).CellStyle.WrapText = True, 否則會整張表的 Cell 的 WrapText 都被設為 true

                            var CS = WS.Workbook.CreateCellStyle();
                            CS.WrapText = true;
                            WR.GetCell(Cell).CellStyle = CS;
                        }
                        if (isNeedHyperLink == true)
                        {
                            ICell a = WR.GetCell(Cell);
                            if (oC.ColumnName.ToLower().Contains("url") || oC.ColumnName.ToLower().Contains("link") || oC.ColumnName.Contains("連結"))
                            {
                                IHyperlink templink = new XSSFHyperlink(HyperlinkType.Url);
                                templink.Address = CellText;
                                a.Hyperlink = templink;
                            }
                        }
                        WR.GetCell(Cell).SetCellValue(CellText);
                    }
                }
                Count += 1;
            }

            // 最後一個 Row 
            if (alFinalRow != null)
            {
                WR = WS.CreateRow(Count);
                for (int cell = 0; cell <= alFinalRow.Count - 1; cell++)
                {
                    if (alFinalRow[cell] != null)
                    {
                        WR.CreateCell(cell).SetCellType(GetCellTypeByObject(alFinalRow[cell]));
                        if (GetCellTypeByObject(alFinalRow[cell]) == CellType.Numeric)
                            WR.GetCell(cell).SetCellValue(System.Convert.ToDouble(alFinalRow[cell]));
                        else
                        {
                            string CellText = (string)alFinalRow[cell];
                            if (alFinalRow[cell].GetType().ToString() == "DateTime")
                                CellText = ((DateTime)alFinalRow[cell]).ToString("yyyy-MM-dd HH:mm:ss").Replace(" 00:00:00", "");

                            // 有 Enter 時, 讓 cell 要可以自動換行
                            if (CellText.Contains("\r") || CellText.Contains("\n"))
                            {
                                // 不能直接用 WR.GetCell(Cell).CellStyle.WrapText = True, 否則會整張表的 Cell 的 WrapText 都被設為 true

                                var CS = WS.Workbook.CreateCellStyle();
                                CS.WrapText = true;
                                WR.GetCell(cell).CellStyle = CS;
                            }

                            WR.GetCell(cell).SetCellValue(CellText);
                        }
                    }
                }
            }
            // 設定寬度, 先使用自動寬度, 若是有額外指定寬度, 就使用指定的寬度再設定一次.
            int TotalColumn = DT.Columns.Count - 1;
            for (int I = 0; I <= TotalColumn; I++)
            {
                WS.AutoSizeColumn(I);
            }
        }

        public static void DtToExcel(DataTable dt, string fileName, string sheetName = "Sheet1", ArrayList alFinalRow = null, bool isNeedHyperLink = false, List<List<string>> alHeaderRows = null)
        {
            var wb = DtToWorkbook(dt, sheetName, alFinalRow: alFinalRow, IsNeedHyperLink: isNeedHyperLink, alHeaderRows: alHeaderRows);

            FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            wb.Write(fs);
            fs.Close();
        }

        public static void DtToExcel(DataTable dt, MemoryStream memoryStream, string sheetName = "Sheet1", ArrayList alFinalRow = null, bool isNeedHyperLink = false, List<List<string>> alHeaderRows = null)
        {
            var wb = DtToWorkbook(dt, sheetName, alFinalRow: alFinalRow, isNeedHyperLink: isNeedHyperLink, alHeaderRows: alHeaderRows);
            wb.Write(memoryStream);
        }

        /// <summary>
        /// 欄位內容(用來檢查)
        /// </summary>
        public class DataColumnContent
        {
            /// <summary>
            /// 欄位名稱
            /// </summary>
            public string name { get; set; }
            /// <summary>
            /// 欄位內容型態
            /// </summary>
            public Validator.Type type { get; set; }
            /// <summary>
            /// 是否可以空白
            /// </summary>
            public bool isEmptyOK { get; set; }
            /// <summary>
            /// 最大值
            /// </summary>
            public double? maxValue { get; set; }
            /// <summary>
            /// 最小值
            /// </summary>
            public double? minValue { get; set; }
            /// <summary>
            /// 檢查發現錯誤要顯示文字
            /// </summary>
            public string action { get; set; }
        }
    }
}
