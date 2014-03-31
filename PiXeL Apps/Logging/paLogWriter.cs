using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace PiXeL_Apps.Logging
{
    sealed class paLogWriter
    {
        public static paLogWriter writer = new paLogWriter();
        private StorageFile logBestand = null; //Bestand waar log geschreven wordt
        private string strDateFormat = "Logbestand_({0}-{1}-{2}).log"; //Afhankelijk van de datum wordt een logbestand gemaakt
        private string strLogFormat = "{0:yyyy-MM-dd HH\\:mm\\:ss\\:ffff}\tType: {1}\tBericht: '{2}'"; //Formaat van de logberichten
        private string strBestandsnaam = null; //Zal de bestandsnaam bevatten
        //Formaat van de foutmelding die weggeschreven wordt.

        //Handelt requests af, in dit geval 1 request tegelijk aangezien maar één keer tegelijk naar een bestand geschreven kan worden.
        private SemaphoreSlim semSemaphoreSlim = new SemaphoreSlim(1);

        /// <summary>
        /// Schrijver aanmaken voor het wegschrijven van opgeworpen logberichten
        /// </summary>
        public paLogWriter()
        {
            strBestandsnaam = String.Format(strDateFormat, DateTime.Now.Day, DateTime.Now.Month, DateTime.Now.Year);
            WijsBestandToe();
        }

        public void LinkDelegate()
        {
            paLogging.log.processLog += new paLogging.ProcessLog(NieuwLogVerwerken);
        }
        
        /// <summary>
        /// Het bestand waarnaar geschreven wordt ophalen indien mogelijk.
        /// </summary>
        private async void WijsBestandToe()
        {
            try
            {
                logBestand = await KnownFolders.DocumentsLibrary.CreateFileAsync(strBestandsnaam, CreationCollisionOption.OpenIfExists);
            }
            catch (UnauthorizedAccessException)
            {
                //WinRT is nog niet volledig optimaal. Deze folder is zeer zeker toegankelijk, maar dit scenario is al verschillende keren als bug gerapporteert!
                Debug.WriteLine("Logschrijver heeft vanwege onverklaarbare redenen geen toegang tot de skydrive (Microsoft bug).");
            }
        }

        /// <summary>
        /// Methode die opgeroepen wordt als de Logger een event opwerpt
        /// </summary>
        /// <param name="strLevel">Waarschuwing, etc</param>
        /// <param name="strMessage">Bericht van de log</param>
        private void NieuwLogVerwerken(string strLevel, string strMessage)
        {
            if (logBestand == null) return; //Als de storage file niet opgehaald kon worden, niets meer doen.

            List<string> lines = new List<string>();
            string newFormatedLine = String.Format(strLogFormat, DateTime.Now, strLevel, strMessage);

            Debug.WriteLine(newFormatedLine); //Wegschrijven naar de standaard debugger
            lines.Add(newFormatedLine);
            WriteToFile(lines); //Wegschrijven naar het logbestand.
        }

        /// <summary>
        /// Schrijft de opgenomen lijnen asynchroon weg naar het logbestand
        /// </summary>
        /// <param name="lines"></param>
        private async void WriteToFile(IEnumerable<string> lines)
        {
            await semSemaphoreSlim.WaitAsync();

            //Asynchrone taak voor het loggen van in-app logregels
            await Task.Run(async () =>
            {
                try
                {
                    await FileIO.AppendLinesAsync(logBestand, lines); //Het openen en de gelogde lijnen er aan toevoegen
                }
                catch (Exception)
                {
                    Debug.WriteLine("Logschrijver kon het logbestand '"+strBestandsnaam+"' niet openen."); //Indien en fout optreed kon het logbestand niet worden geopend
                }
                finally
                {
                    semSemaphoreSlim.Release(); //Huidige request wissen
                }
            });
        }
    }
}