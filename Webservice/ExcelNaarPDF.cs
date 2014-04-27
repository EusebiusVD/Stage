using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;
using System.Xml;
using Excel = Microsoft.Office.Interop.Excel;

namespace Webservice
{
    public class ExcelNaarPDF
    {
        private Logging loggingService; private string applicatieNaam = "WordNaarPDF";
        private string bronPad = HttpContext.Current.Server.MapPath("~/bestanden/");

        private DirectoryInfo dirInfo;
        private static List<FileInfo> excelFiles = new List<FileInfo>();

        private bool officeAanwezig = true;
        private static Application excel;
        private Workbook excelWorkBook;
        private XlFixedFormatType paramExportFormat;
        private XlFixedFormatQuality paramExportQuality;
        private object paramMissing = Type.Missing;
        private static string gewichtLinksVooraan;
        private static string gewichtRechtsVooraan;
        private static string gewichtAchterLinks;
        private static string gewichtAchterRechts;
        private static string gewichtAchterMidden;

        private static List<string> lijstGewichten;

        /// <summary>
        /// Bij het oproepen van deze klasse wordt een Excel applicatie gestart.
        /// Deze blijft in de achtergrond als proces draaien zodat excel sheets behandeld kunnen worden zonder
        /// tussenkomst van de gebruiker.
        /// 
        /// Bijkomend, als een nieuwe word applicatie aangemaakt is, wordt het id van het proces opgeslagen in de settings.config file.
        /// Dit is een workaround om gegevens op te slaan, aangezien normaal enkel geschreven kan worden naar USERS. Hiervan maken wij geen gebruik.
        /// </summary>
        public ExcelNaarPDF()
        {
            try
            {
                if (excel == null)
                {
                    string opgeslagenExcelProcessId = System.Configuration.ConfigurationManager.AppSettings.Get("excelAppProcessId");
                    Process[] processen = Process.GetProcessesByName("Excel");
                    IEnumerable<Process> bestaandProces =
                        from proces in processen
                        where proces.Id.ToString().Equals(opgeslagenExcelProcessId)
                        select proces;

                    if (bestaandProces.Any())
                        bestaandProces.First().Kill();

                    excel = new Application();
                    excel.Visible = false;
                    excel.ScreenUpdating = false;
                    excel.DisplayAlerts = false;

                    processen = Process.GetProcessesByName("Excel");
                    int excelAppProcessId;
                    processen.OrderBy(proces => proces.StartTime);
                    if (processen.Any())
                    {
                        excelAppProcessId = processen.Last().Id;

                        //BRON: http://www.geekzone.co.nz/dmw/1755
                        //Extra config bestand, rede = waarden in web.config bijwerken vereist server herstart.
                        //waarde in extra bestand bijwerken en cache update is voldoende en heeft geen herstart nodig.
                        XmlDocument xmldoc = new XmlDocument();
                        string baseDir = System.Web.HttpRuntime.AppDomainAppPath;
                        string configPath = Path.Combine(baseDir, "Settings.config");
                        xmldoc.Load(Path.Combine(HttpRuntime.AppDomainAppPath, "Settings.config"));
                        XmlNode node = xmldoc.SelectSingleNode("appSettings/add[@key = 'excelAppProcessId']");
                        XmlNode valueAttribute = node.Attributes.GetNamedItem("value");
                        if (valueAttribute != null)
                        {
                            valueAttribute.InnerXml = excelAppProcessId.ToString();
                            xmldoc.Save(configPath);
                        }

                        //update the cached setting
                        System.Configuration.ConfigurationManager.AppSettings.Set("excelAppProcessId", excelAppProcessId.ToString());
                    }
                }
                
            }
            catch (COMException)
            {
                officeAanwezig = false;
                Logging.log.WriteLine(applicatieNaam, "Deze computer kan geen excelbestanden converteren omdat er geen Microsoft Office pakket geïnstalleerd is.");
            }            
        }

        /// <summary>
        /// Sluit de excelapplicatie zodra de webservice afgesloten word.
        /// Anders blijft een Excel applicatie actief op de achtergrond waarin excel sheets verwerkt worden.
        /// </summary>
        ~ExcelNaarPDF()
        {
            // Excel afsluiten indien deze geopend werd.
            if (excel != null)
            {
                excel.Quit();
                excel = null;
            }

            //Alle resources twee maal verwijderen (bug?).
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        /// <summary>
        /// Deze methode haalt voorbereidend een lijst van excel bestanden op in een bepaalde map.
        /// Eveneens wordt de array met FileInfo objecten aangemaakt voor later gebruik in de ConverteerNaarWord methode.
        /// </summary>
        /// <returns>IReadOnlyList met string instanties</returns>
        public IReadOnlyList<string> HaalBestandsNamenOp(string vehicle_id, List<string> pdfNamen)
        {

            //Lijst van word document in een bepaalde map opvragen.
            dirInfo = new DirectoryInfo(String.Format("{0}{1}", bronPad, vehicle_id));
            IEnumerable<string> bestandsNamen = new List<string>();
            try
            {
                bestandsNamen =
                    from bestands in (excelFiles = dirInfo.GetFiles("*.xls").Where(bestand => !bestand.Name.StartsWith("~$")).ToList<FileInfo>())
                    select bestands.Name;

                if (pdfNamen != null)
                    //Opnieuw het toewijzen van alle wordFiles en controleren of deze FileInfo's (pdf-bestanden) gevraagd werden door de app of niet.
                    bestandsNamen =
                        from gevraagdBestand in excelFiles = excelFiles.Where(bestand => pdfNamen.Contains(bestand.Name.Substring(0, bestand.Name.LastIndexOf(".")))).ToList<FileInfo>()
                        select gevraagdBestand.Name;
            }
            catch (DirectoryNotFoundException)
            {
                Logging.log.WriteLine(applicatieNaam, String.Format("De map voor auto {0} bestaat niet in {1}", vehicle_id, bronPad));
            }

            return bestandsNamen.ToList<string>();
        }

        /// <summary>
        /// Deze methode start een nieuwe instantie van een Office Excel applicatie.
        /// De print service naar 
        /// van het Office pakket wordt gebruikt om wordt documenten om te zetten naar een PDF-bestand.
        /// </summary>
        /// <param name="vehicle_id">voor welke auto?</param>
        /// <param name="pdfNamen">Voor welke documenten?</param>
        public void ConverteerExcelNaarPDF(string vehicle_id, List<string> pdfNamen)
        {
            if (excelFiles.Count() == 0)
                HaalBestandsNamenOp(vehicle_id, pdfNamen);

            if (pdfNamen.Count != 0 && officeAanwezig)
            {
                paramExportFormat = XlFixedFormatType.xlTypePDF;
                paramExportQuality = XlFixedFormatQuality.xlQualityStandard;

                foreach (FileInfo excelFile in excelFiles)
                {
                    ConverteerBestand(excelFile);
                }
            }
        }


        /// <summary>
        /// Methode die het excel sheet opslaat als PDF bestand.
        /// </summary>
        /// <param name="excelBestand">Excel bestand om te zetten</param>
        public void ConverteerBestand(FileInfo excelBestand)
        {
            try
            {
                string bestandsnaam = excelBestand.Name;
                string bestandsPad = excelBestand.FullName;
                string doelPad = String.Format("{0}.pdf", bestandsPad.Substring(0, bestandsPad.LastIndexOf('.')));

                // Het excel bestand openen
                excelWorkBook = excel.Workbooks.Open(bestandsPad,
                    paramMissing, paramMissing, paramMissing, paramMissing,
                    paramMissing, paramMissing, paramMissing, paramMissing,
                    paramMissing, paramMissing, paramMissing, paramMissing,
                    paramMissing, paramMissing);

                // Opslaan als PDF-bestand
                if (excelWorkBook != null)
                {
                    excelWorkBook.ExportAsFixedFormat(paramExportFormat,
                        doelPad, paramExportQuality,
                        true, true, Type.Missing,
                        Type.Missing, false,
                        paramMissing);
                    //Het ophalen van de gewichten en bandenspanning
                    if (bestandsnaam.Equals("BandingspanningGewichten.xlsx"))
                    {
                        string currentSheet = "Sheet1";
                        Excel.Sheets excelSheets = excelWorkBook.Worksheets;
                        Excel.Worksheet xlws = (Excel.Worksheet)excelSheets.get_Item(currentSheet);
                        lijstGewichten = new List<string>();
                        lijstGewichten.Add(((Excel.Range)xlws.Cells[12, 4]).Value2.ToString());
                        lijstGewichten.Add(((Excel.Range)xlws.Cells[13, 4]).Value.ToString());
                        lijstGewichten.Add(((Excel.Range)xlws.Cells[15, 4]).Value.ToString());
                        lijstGewichten.Add(((Excel.Range)xlws.Cells[16, 4]).Value.ToString());
                        lijstGewichten.Add(((Excel.Range)xlws.Cells[17, 4]).Value.ToString());

                        lijstGewichten.Add(((Excel.Range)xlws.Cells[3, 4]).Value.ToString());
                        lijstGewichten.Add(((Excel.Range)xlws.Cells[4, 4]).Value.ToString());
                        lijstGewichten.Add(((Excel.Range)xlws.Cells[6, 4]).Value.ToString());
                        lijstGewichten.Add(((Excel.Range)xlws.Cells[7, 4]).Value.ToString());
                    }

                    excelWorkBook.Close(false, paramMissing, paramMissing);
                    excelWorkBook = null;
                }
            }
            catch (Exception)
            {
                Logging.log.WriteLine(applicatieNaam, String.Format("{0} kon niet geconverteerd worden", excelBestand.Name));
            }
        }

        /// <summary>
        /// Deze methode geeft een lijst van data door aan de applicatie.
        /// Door de data te vergelijken in de app kan gecontroleerd worden of een bepaald excel bestand opgehaald moet worden.
        /// </summary>
        /// <param name="vehicle_id">Het autoid van het type string</param>
        /// <returns>Lijst met per document de laatste datum gewijzigd</returns>
        public IReadOnlyList<string> HaalBestandsDataOp(string vehicle_id)
        {
            if (excelFiles.Count() == 0)
                HaalBestandsNamenOp(vehicle_id, null);

            List<string> bestandsData = new List<string>();
            foreach (FileInfo excelBestand in excelFiles)
            {
                bestandsData.Add(excelBestand.LastWriteTime.ToString());
            }
            return bestandsData;
        }
        public static List<string> GetGewichten()
        {
            return lijstGewichten;
        }
    }
}