using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Web;
using Excel = Microsoft.Office.Interop.Excel;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Webservice
{
    public class ExcelReader
    {
        private static string text;
        private static string gewichtLinksVooraan;
        private static string gewichtRechtsVooraan;
        private static string gewichtAchterLinks;
        private static string gewichtAchterRechts;
        private static string gewichtAchterMidden;

        /// <summary>
        /// In deze constructor wordt de methode ReadExcelFileSAX opgeroepen
        /// </summary>
        /// <param name="pad"></param>
        public ExcelReader(String pad)
        {

            ReadExcelFileSAX(pad);
        }
        /// <summary>
        /// Deze methode gaat de gegevens op een bepaalde plaats in een excel document ophalen
        /// </summary>
        /// <param name="fileName">De file waaruit de gegevens gehaald moeten worden</param>
        private static void ReadExcelFileSAX(string fileNaam)
        {
            Excel.Application excelApp = new Microsoft.Office.Interop.Excel.Application();

            excelApp.Visible = false;

            Excel.Workbook excelWorkbook = excelApp.Workbooks.Open(fileNaam, 0, true);

            Excel.Sheets excelSheets = excelWorkbook.Worksheets;

            string currentSheet = "Sheet1";
            Excel.Worksheet xlws = (Excel.Worksheet)excelSheets.get_Item(currentSheet);

            gewichtLinksVooraan = ((Excel.Range)xlws.Cells[12, 4]).Value2.ToString();
            gewichtRechtsVooraan = ((Excel.Range)xlws.Cells[13, 4]).Value.ToString();
            gewichtAchterLinks = ((Excel.Range)xlws.Cells[15, 4]).Value.ToString();
            gewichtAchterRechts = ((Excel.Range)xlws.Cells[16, 4]).Value.ToString();
            gewichtAchterMidden = ((Excel.Range)xlws.Cells[17, 4]).Value.ToString();

        }
        /// <summary>
        /// geeft een lijst terug aan de webservice
        /// </summary>
        /// <returns>Lijst met gewichten</returns>
        public List<string> LeesExcel()
        {
            var lijstGewichten = new List<string>();
            lijstGewichten.Add( gewichtLinksVooraan);
            lijstGewichten.Add(gewichtRechtsVooraan);
            lijstGewichten.Add(gewichtAchterLinks);
            lijstGewichten.Add( gewichtAchterRechts);
            lijstGewichten.Add(gewichtAchterMidden);

            return lijstGewichten;
        }
    }
}