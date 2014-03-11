using PiXeL_Apps.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Pdf;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;

namespace PiXeL_Apps.Common
{
    class PdfSupport
    {
        public static PdfSupport pdfSupport = new PdfSupport();
        private static IReadOnlyList<StorageFolder> storageFolders;
        private static StorageFolder doelfolder;
        private List<string> pdfNamen = new List<string>();
        

        public enum RENDEROPTIONS
        {
            NORMAL,
            ZOOM,
            PORTION
        }
        uint ZOOM_FACTOR = 3; //300% zoom
        Rect PDF_PORTION_RECT = new Rect(100, 100, 300, 400); //portion of a page

        /// <summary>
        /// Het oproepen van de methode BeriedPdfSupportVoor()
        /// </summary>
        public PdfSupport()
        {
            BereidPdfSupportVoor();
        }

        /// <summary>
        /// Asynchrone voorbereiding van PdfSupport. 
        /// </summary>
        private async Task BereidPdfSupportVoor()
        {
            try
            {
                doelfolder = await ApplicationData.Current.LocalFolder.GetFolderAsync(LocalDB.database.GetAutoId());
            }
            catch
            { /*Kan doelfolder niet aanmaken omdat geen await in catch-clause gedaan kan worden*/ }

            if (doelfolder == null) //Dus als doelfolder == null -> hier aanmaken
                doelfolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(LocalDB.database.GetAutoId());
        }

        /// <summary>
        /// De doelmap waarnaar PdfSupport zijn documenten en mappen zal schrijven kan hier opgehaald worden voor latere referentie.
        /// </summary>
        /// <returns>StorageFolder doelfolder</returns>
        public async Task<StorageFolder> GetDoelFolder()
        {
            if (doelfolder == null)
                await BereidPdfSupportVoor();

            return doelfolder;
        }

        /// <summary>
        /// Methode die gebruik maakt van de lijst van bestandsnamen ontvangen van de webservice
        /// om de bestandsnamen met .doc extensie te converteren naar een naam zonder .doc extentie.
        /// </summary>
        /// <returns></returns>
        public async Task<List<string>> GetPdfNamen(bool vernieuwen)
        {
            bool haalLokalePDFNamenOp = true;
            if (!vernieuwen && pdfNamen.Count > 0)
                return pdfNamen; //Geef pdfnamen terug als er niet vernieuwd moet worden en als er pdfnamen zijn
            else
            {
                try
                {
                    pdfNamen.Clear();
                    List<string> docNamen = await LocalDB.database.GetPdfNamen();
                    foreach (string naam in docNamen) //Als bestandsnaam begint met ~$ zijn dit tijdelijke bestanden
                    {
                        if (!naam.Contains("~$")) //als geen tijdelijk bestand -> toevoegen, anders negeren.
                            pdfNamen.Add(naam.Substring(0, naam.LastIndexOf('.')));
                    }
                }
                catch (Exception e)
                {
                    haalLokalePDFNamenOp = true;
                    paLogging.log.Error("Er is een fout opgetreden bij het ophalen van PDF namen.\nMogelijk is er geen verbinding: " + e.Message);
                }
            }
            if (haalLokalePDFNamenOp)
            {
                if (storageFolders == null || storageFolders.Count == 0)
                        await GetPdfStorageFolders(true);
                pdfNamen = (from folder in storageFolders
                            select folder.Name).ToList<string>();
            }
            return pdfNamen;
        }

        /// <summary>
        /// Methode die de pdf bestanden van de server ontvangt en vervolgens wegschrijft naar de lokale opslag van
        /// de tablet.
        /// </summary>
        public async Task maakPDFBestanden(bool vernieuwen)
        {
            await GetPdfNamen(vernieuwen);
            string huidigeNaam = String.Empty; //Voor foutbericht indien nodig
            try
            {
                IReadOnlyList<string> pdfNamenWijzigingen = null;
                if (vernieuwen) //Als er vernieuwd moet worden...
                    pdfNamenWijzigingen = await LocalDB.database.ControleerTeVernieuwenBijlagen(); //Controleer welke bestanden opnieuw opgehaald moeten worden.


                //De lijst van op te halen bestanden doorsturen naar de webservice en wachten op een lijst van pdf-bestanden
                var pdfLijst = await LocalDB.database.GetPDFBestanden(pdfNamenWijzigingen);
                if (pdfLijst != null && pdfLijst.Count() != 0)
                {
                    List<StorageFile> bestandenVoorFotos = new List<StorageFile>();
                    for (int teller = 0; teller < pdfNamenWijzigingen.Count; teller++)
                    {
                        huidigeNaam = pdfNamenWijzigingen[teller];
                        var pdf = await doelfolder.
                            CreateFileAsync(pdfNamenWijzigingen[teller] + ".pdf", Windows.Storage.CreationCollisionOption.ReplaceExisting);
                        await Windows.Storage.FileIO.WriteBytesAsync(pdf, pdfLijst[teller]);
                        bestandenVoorFotos.Add(pdf);
                    }
                    await GetAfbeeldingenVanPDF(RENDEROPTIONS.NORMAL, bestandenVoorFotos);
                }
            }
            catch (Exception e)
            {
                paLogging.log.Error(String.Format("Er is een fout opgetreden bij het wegschrijven van {0} (PDF byte array): {1}", huidigeNaam, e.Message));
            }
        }

        /// <summary>
        /// Maakt voor elk aangemaakt pdf bestand een nieuwe map met hierin alle pagina's apart per afbeelding.
        /// </summary>
        /// <param name="renderOption"></param>
        /// <param name="bestanden">Een lijst met StorageFolders instanties</param>
        /// <returns>Eventueel de lijst van StorageFiles indien nodig</returns>
        public async Task<List<StorageFile>> GetAfbeeldingenVanPDF(RENDEROPTIONS renderOption, List<StorageFile> bestanden)
        {
            List<StorageFile> paginas = new List<StorageFile>(); //Bekomen lijst van PDF pagina als afbeelding
            foreach (StorageFile bestand in bestanden)
            {
                string bestandsNaam = bestand.Name.Replace(".pdf", ""); //PDF extensie uit de naam wissen voor de folder
                try
                {
                    await doelfolder.CreateFolderAsync(bestandsNaam, CreationCollisionOption.ReplaceExisting); //Maak de doelfolder aan en vervang indien nodig

                    //PDF bestand laden
                    PdfDocument _pdfDocument = await PdfDocument.LoadFromFileAsync(bestand); ;

                    if (_pdfDocument != null && _pdfDocument.PageCount > 0)
                    {
                        // next, generate a bitmap of the page
                        StorageFolder tempFolder = ApplicationData.Current.TemporaryFolder;
                        for (uint pagina = 0; pagina < _pdfDocument.PageCount; pagina++)
                        {
                            var pdfPage = _pdfDocument.GetPage(pagina);

                            if (pdfPage != null)
                            {
                                StorageFile jpgFile = await doelfolder.
                                    CreateFileAsync(String.Format("\\{0}\\{1}_pagina{2}.png", bestandsNaam, bestandsNaam, pagina), CreationCollisionOption.ReplaceExisting);

                                if (jpgFile != null)
                                {
                                    IRandomAccessStream randomStream = await jpgFile.OpenAsync(FileAccessMode.ReadWrite);

                                    PdfPageRenderOptions pdfPageRenderOptions = new PdfPageRenderOptions();
                                    switch (renderOption)
                                    {
                                        case RENDEROPTIONS.NORMAL:
                                            //PDF pagina inladen
                                            await pdfPage.RenderToStreamAsync(randomStream);
                                            break;
                                        case RENDEROPTIONS.ZOOM:
                                            //set PDFPageRenderOptions.DestinationWidth or DestinationHeight with expected zoom value
                                            Size pdfPageSize = pdfPage.Size;
                                            pdfPageRenderOptions.DestinationHeight = (uint)pdfPageSize.Height * ZOOM_FACTOR;
                                            //Render pdf page at a zoom level by passing pdfpageRenderOptions with DestinationLength set to the zoomed in length 
                                            await pdfPage.RenderToStreamAsync(randomStream, pdfPageRenderOptions);
                                            break;
                                    }
                                    await randomStream.FlushAsync();

                                    randomStream.Dispose();
                                    pdfPage.Dispose();

                                    paginas.Add(jpgFile);
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    paLogging.log.Error(String.Format("Er is een fout opgetreden bij het converteren van pdf {0} naar afbeeldingen: {1}", bestandsNaam, e.Message));
                }
            }
            return paginas; //Pagina's teruggeven (leeg indien exception, e.d.)
        }

        /// <summary>
        /// Methode die StorageFiles ophaalt uit de folder waar alle pagina's als van een PDF als PNG
        /// opgeslaan zijn.
        /// </summary>
        /// <param name="pdfNaam">De naam van de PDF van het type String</param>
        /// <returns>Readonly lijst van StorageFiles</returns>
        public async Task<IReadOnlyList<StorageFile>> HaalPdfAfbeeldingenOp(IReadOnlyList<StorageFolder> folders, string pdfNaam)
        {
            IEnumerable<StorageFolder> afbeeldingFolder =
                from f in folders
                where f.Name.Equals(pdfNaam)
                select f; //Selecteer elke folder waarvan de naamvoorkomt in de lijst van afbeeldingenfolders

            StorageFolder folder;
            if (afbeeldingFolder.Count() > 0) //Als er afbeeldingfolders van PDF bestanden zijn...
            {
                folder = afbeeldingFolder.First(); //Selecteer de eerste folder (er zijn toch geen dubbele folders)
                return await folder.GetFilesAsync(); //AHaal al deze bestanden (StorageFile) op en geef ze terug
            }
            return null; //Als de folder niet bestaat, geef null terug
        }

        /// <summary>
        /// Haalt een lijst van StorageFiles op waarin pdf bestanden zitten.
        /// </summary>
        /// <returns>IReadOnlyList met StorageFolder instanties</returns>
        public async Task<IReadOnlyList<StorageFolder>> GetPdfStorageFolders(bool vernieuwen)
        {
            if (vernieuwen || storageFolders == null || storageFolders.Count > 0)
                storageFolders = await doelfolder.GetFoldersAsync();

            return storageFolders;
        }

        /// <summary>
        /// Methode die controleert of bepaalde mappen of pdf documenten uit de localState verwijderd moeten worden of niet.
        /// </summary>
        public async void VerwijderPdfBestanden()
        {
            IEnumerable<StorageFolder> teVerwijderenFolders =
                from folder in storageFolders
                where !pdfNamen.Contains(folder.Name)
                select folder; //Selecteer elke folder waarvan de naam niet voorkomt in de lijst met bestaande pdfnamen

            foreach (StorageFolder folder in teVerwijderenFolders) //Elke te verwijderen pdf -> afbeeldingenfolder en pdf bestand verwijderen
            {
                StorageFile pdf = await ApplicationData.Current.LocalFolder.GetFileAsync(String.Format("{0}.pdf", folder.Name));
                await pdf.DeleteAsync(); //PDF bestand verwijderen
                await folder.DeleteAsync(); //Afbeeldingen verwijderen
            }
        }
    }

}
