using Microsoft.Office.Interop.Word;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;
using System.Xml;
using Webservice.Properties;

namespace Webservice
{
    /// <summary>
    /// Bron : http://stackoverflow.com/questions/607669/how-do-i-convert-word-files-to-pdf-programmatically
    /// Zet word documenten uit een bepaalde map om in pdf bestanden
    /// </summary>
    public class WordNaarPDF
    {
        private string applicatieNaam = "WordNaarPDF";
        private string bronPad = HttpContext.Current.Server.MapPath("~/bestanden/");
        private DirectoryInfo dirInfo;
        private static List<FileInfo> wordFiles = new List<FileInfo>();

        private bool officeAanwezig = true;
        private Document doc;
        private Microsoft.Office.Interop.Word.Application word;
        private object oMissing;

        public WordNaarPDF()
        {
            try
            {
                //Nieuwe instantie van een word document (letterlijk applicatie instantie)
                if (word == null)
                {
                    string opgeslagenWordProcessId = System.Configuration.ConfigurationManager.AppSettings.Get("wordAppProcessId");
                    Process[] processen = Process.GetProcessesByName("WinWord");
                    IEnumerable<Process> bestaandProces =
                        from proces in processen
                        where proces.Id.ToString().Equals(opgeslagenWordProcessId)
                        select proces;

                    if (bestaandProces.Any())
                        bestaandProces.First().Kill();

                    word = new Application();
                    word.Visible = false; //Op de achtergrond
                    word.ScreenUpdating = false; //Geen visuele updates tonen aan de gebruiker
                    word.DisplayAlerts = WdAlertLevel.wdAlertsNone;

                    processen = Process.GetProcessesByName("WinWord");
                    int wordAppProcessId;
                    processen.OrderBy(proces => proces.StartTime);
                    if (processen.Any())
                    {
                        wordAppProcessId = processen.Last().Id;

                        //BRON: http://www.geekzone.co.nz/dmw/1755
                        //Extra config bestand, rede = waarden in web.config bijwerken vereist server herstart.
                        //waarde in extra bestand bijwerken en cache update is voldoende en heeft geen herstart nodig.
                        XmlDocument xmldoc = new XmlDocument();
                        string baseDir = System.Web.HttpRuntime.AppDomainAppPath;
                        string configPath = Path.Combine(baseDir, "Settings.config");
                        xmldoc.Load(Path.Combine(HttpRuntime.AppDomainAppPath, "Settings.config"));
                        XmlNode node = xmldoc.SelectSingleNode("appSettings/add[@key = 'wordAppProcessId']");
                        XmlNode valueAttribute = node.Attributes.GetNamedItem("value");
                        if (valueAttribute != null)
                        {
                            valueAttribute.InnerXml = wordAppProcessId.ToString();
                            xmldoc.Save(configPath);
                        }

                        //update the cached setting
                        System.Configuration.ConfigurationManager.AppSettings.Set("wordAppProcessId", wordAppProcessId.ToString());
                    }

                    /*do
                    {
                        
                        for (int p_count = 0; p_count < processen.Length; p_count++)
                        {
                            
                            if (processen[p_count].MainWindowTitle.Equals(wordAppId))
                            {
                                wordAppProcessId = processen[p_count].Id;

                                //BRON: http://www.geekzone.co.nz/dmw/1755
                                //Extra config bestand, rede = waarden in web.config bijwerken vereist server herstart.
                                //waarde in extra bestand bijwerken en cache update is voldoende en heeft geen herstart nodig.
                                XmlDocument xmldoc = new XmlDocument();
                                string baseDir = System.Web.HttpRuntime.AppDomainAppPath;
                                string configPath = Path.Combine(baseDir, "Settings.config");
                                xmldoc.Load(Path.Combine(HttpRuntime.AppDomainAppPath, "Settings.config"));
                                XmlNode node = xmldoc.SelectSingleNode("appSettings/add[@key = 'wordAppProcessId']");
                                XmlNode valueAttribute = node.Attributes.GetNamedItem("value");
                                if (valueAttribute != null)
                                {
                                    valueAttribute.InnerXml = wordAppProcessId.ToString();
                                    xmldoc.Save(configPath);
                                }

                                //update the cached setting
                                System.Configuration.ConfigurationManager.AppSettings.Set("wordAppProcessId", wordAppProcessId.ToString());
                                break;
                            }
                        }
                    } while (wordAppProcessId == 0);
                    */
                }
            }
            catch (COMException)
            {
                officeAanwezig = false;
                Logging.log.WriteLine(applicatieNaam, "Deze computer kan geen wordbestanden converteren omdat er geen Microsoft Office pakket geïnstalleerd is.");
            }
        }

        ~WordNaarPDF()
        {
            //Applicatie van word document afsluiten
            ((_Application)word).Quit(false, ref oMissing, ref oMissing);
            word = null;

            //Alle resources twee maal verwijderen (bug?).
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        /// <summary>
        /// Deze methode haalt voorbereidend een lijst van word bestanden op in een bepaalde map.
        /// Eveneens wordt de array met FileInfo objecten aangemaakt voor later gebruik in de ConverteerNaarWord methode.
        /// </summary>
        /// <param name="vehicle_id">Voor welke auto?</param>
        /// <param name="pdfNamen">Voor welke documenten?</param>
        /// <returns>IReadOnlyList met string instanties</returns>
        public IReadOnlyList<string> HaalBestandsNamenOp(string vehicle_id, List<string> pdfNamen)
        {
            //Lijst van word document in een bepaalde map opvragen.
            dirInfo = new DirectoryInfo(String.Format("{0}{1}", bronPad, vehicle_id));
            IEnumerable<string> bestandsNamen = new List<string>();
            try
            {
                //Controleren of de bestandsnaam niet begint met ~$ in een lambda zodat de lijst met FileInfo (wordFiles)
                //en de lijst met bestandsNamen opgehaald kan worden in één LINQ query
                bestandsNamen =
                    from bestands in (wordFiles = dirInfo.GetFiles("*.doc").Where(bestand => !bestand.Name.StartsWith("~$")).ToList<FileInfo>())
                    select bestands.Name;

                if (pdfNamen != null)
                    //Opnieuw het toewijzen van alle wordFiles en controleren of deze FileInfo's (pdf-bestanden) gevraagd werden door de app of niet.
                    bestandsNamen =
                        from gevraagdBestand in wordFiles = wordFiles.Where(bestand => pdfNamen.Contains(bestand.Name.Substring(0, bestand.Name.LastIndexOf(".")))).ToList<FileInfo>()
                        select gevraagdBestand.Name;
            }
            catch (DirectoryNotFoundException)
            {
                Logging.log.WriteLine(applicatieNaam, String.Format("De map voor auto {0} bestaat niet in {1}", vehicle_id, bronPad));
            }

            return bestandsNamen.ToList<string>();
        }

        /// <summary>
        /// De methode verwerkt een array met FileInfo objecten. Elk object omschrijft een word document dat elks geconverteerd wordt naar PDF.
        /// Na de convertatie wordt een array van FileInfo objecten (PDF documenten) terug gegeven voor latere bewerking.
        /// </summary>
        /// <param name="vehicle_id">Voor welke auto?</param>
        /// <param name="pdfNamen">Voor welke documenten?</param>
        public void ConverteerWordNaarPDF(string vehicle_id, List<string> pdfNamen)
        {
            if (wordFiles.Count() == 0)
                HaalBestandsNamenOp(vehicle_id, pdfNamen);

            if (pdfNamen.Count != 0 && officeAanwezig)
            {
                //C# kan geen optionele argumenten mee geven dus gebruiken we "leeg" object
                oMissing = System.Reflection.Missing.Value;

                foreach (FileInfo wordFile in wordFiles)
                {
                    ConverteerBestand(wordFile);
                }
            }
        }

        /// <summary>
        /// Methode die het word document opslaat als PDF bestand.
        /// </summary>
        /// <param name="excelBestand">Bestand om te zetten</param>
        public void ConverteerBestand(FileInfo wordBestand)
        {
            //Volledig pad ophalen en opslaan als object voor word.Documents.Open methode
            Object filename = (Object)wordBestand.FullName;

            //Optionele argumenten opvullen met leeg object
            doc = word.Documents.Open(ref filename, ref oMissing,
                ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                ref oMissing, ref oMissing, ref oMissing, ref oMissing);
            doc.Activate();

            object outputFileName = String.Format("{0}.pdf", wordBestand.FullName.Substring(0, wordBestand.FullName.LastIndexOf('.')));
            object fileFormat = WdSaveFormat.wdFormatPDF;

            //Document opslaanals PDF
            doc.SaveAs(ref outputFileName,
                ref fileFormat, ref oMissing, ref oMissing,
                ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                ref oMissing, ref oMissing, ref oMissing, ref oMissing);

            //Document sluiten maar word nog niet.
            //Doc moet naar _Document geconverteerd worden zodat de Close methode opgeroepen kan worden.     
            if (doc != null)
            {
                object saveChanges = WdSaveOptions.wdDoNotSaveChanges;
                ((_Document)doc).Close(ref saveChanges, ref oMissing, ref oMissing);
                doc = null;
            }
        }

        /// <summary>
        /// Deze methode geeft een lijst van data (laatst gewijzigd) door aan de applicatie.
        /// Door de data te vergelijken in de app kan gecontroleerd worden of een bepaald word bestand opgehaald moet worden.
        /// </summary>
        /// <param name="vehicle_id">Voor welke auto?</param>
        /// <returns>IReadOnlyList met string intstanties</returns>
        internal IReadOnlyList<string> HaalBestandsDataOp(string vehicle_id)
        {
            if (wordFiles.Count() == 0)
                HaalBestandsNamenOp(vehicle_id, null);

            List<string> bestandsData = new List<string>();
            foreach (FileInfo wordBestand in wordFiles)
            {
                bestandsData.Add(wordBestand.LastWriteTime.ToString());
            }

            return bestandsData;
        }
    }
}