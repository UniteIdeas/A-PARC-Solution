using System;
using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Packaging;
using TextClassification.Model;

namespace TextClassification.Service.Dataset.Excel
{
    public class BaseExcel : BaseDataSet
    {
        internal static List<SharedStringItem> Sst;
        internal static SheetData SheetData;
        internal static WorkbookPart WorkbookPart;

        internal static SpreadsheetDocument Init(string path)
        {
            var spreadsheetDocument = SpreadsheetDocument.Open(path, false);
            WorkbookPart = spreadsheetDocument.WorkbookPart;
            var worksheetPart = WorkbookPart.WorksheetParts.First();
            SheetData = worksheetPart.Worksheet.Elements<SheetData>().First();
            Sst = WorkbookPart.SharedStringTablePart.SharedStringTable.Elements<SharedStringItem>().ToList();
            return spreadsheetDocument;
        }

        internal static string GetValue(CellType cell, ClassifiedText c = null, bool clearText = true)
        {
            if (cell.DataType != null && cell.DataType == CellValues.SharedString)
            {
                var ssi = Sst[Convert.ToInt32(cell.CellValue.Text)];
                var result = ssi.InnerText;
                if (clearText)
                    return result;

                if (ssi.Text == null)
                {
                    var list = ssi.Elements<Run>().ToList();
                    if (list.Count > 1)
                    {
                        var found = false;
                        foreach (var run in list)
                        {
                            var properties = run.OuterXml;
                            if (!properties.Contains("<x:b />") && !properties.Contains("<x:b/>")) continue;
                            found = true;
                            if (c != null)
                                c.HighLightedText = run.InnerText;
                            result = result.Replace(run.InnerText, string.Format("<b>{0}</b>", run.InnerText));
                        }
                        if (!found)
                        {
                            if (c != null)
                                c.HighLightedText = list[0].InnerText;
                            result = result.Replace(list[0].InnerText, string.Format("<b>{0}</b>", list[0].InnerText));
                        }
                    }
                    return result;
                }

                return ssi.InnerText;
            }

            return cell.InnerText;
        }

        //internal static int GetInt(string value)
        //{
        //    int result;
        //    return int.TryParse(value, out result) ? result : 0;
        //}

        #region OldWay
        ////Add this codes in your progam code
        //private static ApplicationClass _appExcel;
        //private static Workbook _newWorkbook;
        //private static _Worksheet _objsheet;

        ////Method to initialize opening Excel
        //internal static _Worksheet Init(string path)
        //{
        //    _appExcel = new ApplicationClass();

        //    if (System.IO.File.Exists(path))
        //    {
        //        // then go and load this into excel
        //        _newWorkbook = _appExcel.Workbooks.Open(path, true, true);
        //        _objsheet = (_Worksheet)_appExcel.ActiveWorkbook.ActiveSheet;
        //        return _objsheet;
        //    }

        //    System.Runtime.InteropServices.Marshal.ReleaseComObject(_appExcel);
        //    _appExcel = null;
        //    throw new Exception("Unable to open file!");
        //}

        ////Method to get value; cellname is A1,A2, or B1,B2 etc...in excel.
        //internal static string GetValue(string cellname)
        //{
        //    string value;
        //    try
        //    {
        //        if (_objsheet == null)
        //            return string.Empty;

        //        return _objsheet.Range[cellname].Value2.ToString();
        //    }
        //    catch
        //    {
        //        value = string.Empty;
        //    }

        //    return value;
        //}

        ////Method to close excel connection
        //internal static void Close()
        //{
        //    if (_appExcel != null)
        //    {
        //        try
        //        {
        //            _newWorkbook.Close();
        //            System.Runtime.InteropServices.Marshal.ReleaseComObject(_appExcel);
        //            _appExcel = null;
        //            _objsheet = null;
        //        }
        //        catch (Exception)
        //        {
        //            _appExcel = null;
        //            throw;
        //        }
        //        finally
        //        {
        //            GC.Collect();
        //        }
        //    }
        //}

        //internal int GetInt(Range cell)
        //{
        //    if (cell == null || cell.Value2 == null)
        //        return 0;
        //    var value = cell.Value2.ToString();
        //    if (string.IsNullOrEmpty(value))
        //        return 0;

        //    int result;
        //    return int.TryParse(value, out result) ? result : 0;
        //}

        //internal string GetString(Range cell)
        //{
        //    if (cell == null || cell.Value2 == null)
        //        return string.Empty;

        //    return cell.Value2.ToString();
        //} 
        #endregion
    }
}
