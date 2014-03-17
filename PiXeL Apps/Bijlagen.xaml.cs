using PiXeL_Apps.Classes;
using PiXeL_Apps.Common;
using PiXeL_Apps.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;



namespace PiXeL_Apps
{
    public sealed partial class Bijlagen : Page
    {
        private static List<string> pdfNamen = new List<string>();
        private static IReadOnlyList<StorageFolder> storageFolders;
        private static List<Comment> opmerkingen = new List<Comment>();
        private static List<ObjectCodes> objecten = new List<ObjectCodes>();
        private static List<DefectCodes> defecten = new List<DefectCodes>();
        private static List<Comment> vijfOpmerkingen = new List<Comment>();
        private Boolean afbeeldingVergroot = false;

        /// <summary>
        /// In this constructor the methods GetObjectEnDefectCodes() and VeranderConnectiviteit are called()
        /// </summary>
        public Bijlagen()
        {
            this.InitializeComponent();
            btnGelezen.IsEnabled = false;

            paLogging.log = new paLogging();
            paLogWriter.writer.LinkDelegate();

            lblBijlagenVullenStatus.Text = "Klaar om te starten.";

            //Event for checking network connectivity
            VeranderingConnectiviteit();
        }

        /// <summary>
        /// Gets the inspections for the selected and assigned car, if they exist
        /// </summary>
        private void StartVoorbereidingen()
        {
            VerschillendeInspecties.HaalInspectiesOp(new List<Inspectie>());
            ProblemenTest.HaalCodesOp();
        }

        /// <summary>
        /// This method gets all folders and searches after PDF files
        /// </summary>
        /// <returns>Task</returns>
        private async Task ControleerPdfAfbeeldingen()
        {
            storageFolders = await PdfSupport.pdfSupport.GetPdfStorageFolders(true);
            pdfNamen = await PdfSupport.pdfSupport.GetPdfNamen(true); //Haal pdf namen op (niet vernieuwen)
            if (storageFolders.Count != pdfNamen.Count)
            {
                //Gewijzigde data resetten.
                LocalStorage.localStorage.SlaGegevensOp("PdfWijzigingsData", new List<string>());
                storageFolders = await PdfSupport.pdfSupport.GetPdfStorageFolders(true);
            }
        }

        /// <summary>
        /// Gets all object- and defectcodes, as well as the rijberichten.
        /// The bijlagen and the first 5 rijberichten will be showed
        /// If there are no PDF files a message will be showed
        /// </summary>
        private async Task VulBijlagen()
        {
            lblBijlagenVullenStatus.Text = "Verbonden!";
            prSynchroniseren.IsActive = true;
            prSynchroniseren.Visibility = Windows.UI.Xaml.Visibility.Visible;

            bool databaseBestaat = true;
            try
            {
                StorageFile sqLiteDatabase = await Windows.Storage.ApplicationData.Current.LocalFolder.GetFileAsync("LocalDBSQLite.sqlite");
                if ((await sqLiteDatabase.GetBasicPropertiesAsync()).Size == 0)
                    databaseBestaat = false;
            }catch (FileNotFoundException)
            {
                databaseBestaat = false;
            }

            if (databaseBestaat)
            {
                await LocalDB.database.HaalToegewezenAutoOp();

                string geselecteerdScript = LocalDB.database.GetAutoId();
                if (!geselecteerdScript.Equals(String.Empty))
                {
                    await VulRecenteCommentaren();

                    #region PDFBestanden

                    await ControleerPdfAfbeeldingen();
                    lblBijlagenVullenStatus.Text = "Even kijken of er nieuwe bijlagen zijn.";
                    await PdfSupport.pdfSupport.maakPDFBestanden(true);

                    await VulScrollViewerMetAfbeeldingen();

                    #endregion
                }
                else
                {
                    grInloggen.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    srvBijlagen.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                    lblBijlagen.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    btnGelezen.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    lblError.Text = "Er nog geen wagen aan de tablet toegewezen, neem contact op met een beheerder.";
                    lblError.FontSize = 24;
                    grStatus.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    prSynchroniseren.IsActive = false;
                }
            }
            else
            {
                //Juiste velden tonen voor foutmelding
                grInloggen.Visibility = Windows.UI.Xaml.Visibility.Visible;
                srvBijlagen.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                lblBijlagen.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                btnGelezen.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                lblError.Text = "Dit is de eerste keer dat u de applicatie start. Contacteer een beheerder om een wagen toe te wijzen.";
                lblError.FontSize = 24;
                grStatus.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                prSynchroniseren.IsActive = false;

                //Inlogknop aanpassen en database creëren
                object huidigeInlogButtonInhoud = btnInloggen.Content;
                btnInloggen.Content = "Database opvullen...";
                await LocalDB.database.CreateDatabase(false);
                btnInloggen.Content = huidigeInlogButtonInhoud;
            }
        }

        /// <summary>
        /// Gets a list with the 5 most recent rijberichten and shows them on the bijlage screen.
        /// </summary>
        /// <returns></returns>
        private async Task VulRecenteCommentaren()
        {
            List<Comment> opmerkingen = await OverzichtOpmerkingen.GetOverzichtComments();

            if (opmerkingen.Count >= 5)
            {
                gvwOpmerkingen.ItemsSource = opmerkingen.GetRange(0, 5);
            }
            else
            {
                if (opmerkingen.Count > 0)
                {
                    gvwOpmerkingen.ItemsSource = opmerkingen;
                }
                else
                {
                    lblOpmerkingen.Text = "Er zijn geen rijberichten aanwezig voor de toegekende wagen.";
                }
            }
        }

        private async Task VulScrollViewerMetAfbeeldingen()
        {

            lblBijlagenVullenStatus.Text = "Laden voltooid";
            storageFolders = await PdfSupport.pdfSupport.GetPdfStorageFolders(true);

            lblBijlagenVullenStatus.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            lblBijlagenVullen.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            prSynchroniseren.IsActive = false;
            prSynchroniseren.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            if (pdfNamen.Count == 0) //als de lijst met PDf namen geen strings bevat, probeer lokaal te zoeken naar bijlagen
                pdfNamen = (from pdfFolder in storageFolders select pdfFolder.Name).ToList<string>();
            if (pdfNamen.Count > 0) //Als er dan nog geen namen zijn, wil dit zeggen dat er geen bijlagen bestaan
            {
                foreach (string pdfNaam in pdfNamen)
                {
                    if (pdfNaam.ToLower().Equals("wagenspecificaties") || pdfNaam.ToLower().Equals("bijlage"))
                    {
                        try
                        {
                            await VoegPdfToeAanBijlagen(pdfNaam);
                        }
                        catch (NullReferenceException)
                        {
                            TextBlock txtFout = new TextBlock();
                            txtFout.Text = pdfNaam + " bevindt zich niet op deze tablet.";
                            txtFout.FontSize = 25;
                            txtFout.Margin = new Thickness(30, 20, 0, 10);
                            bijlagenPanel.Children.Add(txtFout);
                        }
                    }
                }
            }
            else
            {
                TextBlock txtMelding = new TextBlock();
                txtMelding.Text = String.Format("Er werden geen bijlagen gevonden voor wagen {0}.", LocalDB.database.GetAutoId());
                txtMelding.FontSize = 25;
                txtMelding.Margin = new Thickness(30, 20, 0, 10);
                bijlagenPanel.Children.Add(txtMelding);
            }
        }

        /// <summary>
        /// Eventhandler for displayed images.
        /// When they are tapped the scrollview expands to make the text more readable.
        /// When tapped again it goes back to its original size
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImgBijlage_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (!afbeeldingVergroot)
            {
                bijlagenTeVerbergen.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                //imgWagenmap
                bijlagenGrid.Width = rootGrid.Width - 20;
                srvBijlagen.Width = bijlagenGrid.Width;
                bijlagenGrid.Margin = new Thickness(10, 10, 10, 10);
            }
            else
            {
                bijlagenTeVerbergen.Visibility = Windows.UI.Xaml.Visibility.Visible;
                bijlagenGrid.Width = rootGrid.Width - 595;
                srvBijlagen.Width = bijlagenGrid.Width;

                bijlagenGrid.Margin = new Thickness(585, 10, 10, 10);
            }

            afbeeldingVergroot = !afbeeldingVergroot;
        }

        /// <summary>
        /// Show the PDF's on the screen. It converts the PDF's to a PNG image
        /// </summary>
        /// <param name="pdfNaam">naam van de PDF van het type String</param>
        /// <returns>Task</returns>
        private async Task VoegPdfToeAanBijlagen(string pdfNaam)
        {
            try
            {
                var afbeeldingen = await PdfSupport.pdfSupport.HaalPdfAfbeeldingenOp(storageFolders, pdfNaam);
                TextBlock txtPdfTitel = new TextBlock();
                txtPdfTitel.Text = pdfNaam;
                txtPdfTitel.FontSize = 40;
                txtPdfTitel.TextAlignment = TextAlignment.Center;
                txtPdfTitel.Width = bijlagenGrid.Width - 20;
                txtPdfTitel.Margin = new Thickness(10, 70, 10, 10);
                bijlagenPanel.Children.Add(txtPdfTitel);

                foreach (StorageFile pagina in afbeeldingen)
                {
                    BitmapImage paginaAfbeelding = new BitmapImage();
                    FileRandomAccessStream stream = (FileRandomAccessStream)await pagina.OpenAsync(FileAccessMode.Read);
                    paginaAfbeelding.SetSource(stream);
                    Image paginaAfbeeldingControl = new Image();
                    paginaAfbeeldingControl.Source = paginaAfbeelding;
                    paginaAfbeeldingControl.Tapped += ImgBijlage_Tapped;
                    bijlagenPanel.Children.Add(paginaAfbeeldingControl);
                }
            }
            catch (Exception ex)
            {
                paLogging.log.Error(String.Format("Een van de afbeeldingen kon niet getoond worden op het bijlagenscherm.\n{0}", ex.Message));
            }
        }

        /// <summary>
        /// This method handles the click of the btnGelezen.
        /// When clicked you will be redirected to the login screen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnGelezen_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Inlogscherm));
        }

        #region internetConnectie

        /// <summary>
        /// Deze methode controleerd of er geen wijzigingen zijn in de bijlagen als er internet is
        /// Bij geen internet wordt een melding getoond en wordt automatisch gecontrolleerd of er internet is of niet.
        /// </summary>
        /// <param name="sender"></param>
        async void VeranderingConnectiviteit()
        {
            btnSyncInternet.IsEnabled = false;
            if (Common.InternetControle.ControleerInternet())
            {
                btnSyncInternet.IsEnabled = true;
                if (await LocalDB.database.ControleerGebruikers())
                {
                    try
                    {
                        await VulBijlagen();
                        prSynchroniseren.IsActive = false;
                        prSynchroniseren.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                        lblBijlagenVullenStatus.Text = "Gegevens zijn met succes binnengehaald";
                        btnGelezen.IsEnabled = true;
                        
                    }
                    catch (Exception) { }
                }
                else
                {

                    MessageDialog okAnnuleer = new MessageDialog("U heeft internet, maar de bijlagen zijn onbereikbaar.\nMaak als alternatief gebruik van een USB-stick.", "Server Vereist");
                    okAnnuleer.Commands.Add(new UICommand("Gebruik USB"));
                    okAnnuleer.Commands.Add(new UICommand("Probeer opnieuw met Wi-Fi"));
                    var antwoord = await okAnnuleer.ShowAsync();

                    if (antwoord.Label.Equals("Probeer opnieuw"))
                        VeranderingConnectiviteit();

                    btnSyncUSB.Visibility = Windows.UI.Xaml.Visibility.Visible;
                }
            }
            else
            {
                lblBijlagenVullenStatus.Text = "Geen verbinding gevonden.";
                MessageDialog okAnnuleer = new MessageDialog("U heeft geen internet, gelieve naar een Wi-Fi zone te gaan.\nU kan ook de bijlagen via USB-stick ophalen.", "Wi-Fi vereist");
                okAnnuleer.Commands.Add(new UICommand("Begrepen"));
                await okAnnuleer.ShowAsync();
                btnSyncUSB.Visibility = Windows.UI.Xaml.Visibility.Visible;
                while (!Common.InternetControle.ControleerInternet() || !await LocalDB.database.ControleerGebruikers())
                {
                    await Task.Delay(TimeSpan.FromSeconds(2));
                }

                prSynchroniseren.IsActive = false;
                prSynchroniseren.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

                btnSyncInternet.Visibility = Windows.UI.Xaml.Visibility.Visible;
                btnSyncUSB.Visibility = Windows.UI.Xaml.Visibility.Visible;

                VeranderingConnectiviteit();
                btnGelezen.IsEnabled = true;
            }
            StartVoorbereidingen(); //Eventuele voorbereidingen zoals inspecties ophalen
        }

        #endregion

        /// <summary>
        /// The login button on the bijlage screen is only visible when there isn't a car assigned to the tablet.
        /// Only administrators can login
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BtnInloggen_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                User user = await LocalDB.database.GetUser(txtPersoneelsnummer.Text, txtWachtwoord.Password);
                if (user != null && user.Admin)
                    this.Frame.Navigate(typeof(Administratie), user);
                else
                    lblErrorInlog.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                paLogging.log.Critical("Er ging iets verkeerd bij het inloggen van een bestuurder: " + ex.Message);
            }
        }

        /// <summary>
        /// Tries to retrieve tie bijlage and show them.
        /// Networkconnection will be checked again
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BtnSyncInternet_Click(object sender, RoutedEventArgs e)
        {
            lblBijlagenVullen.Text = "Even geduld...";
            lblBijlagenVullenStatus.Text = "Gegevens worden via het netwerk afgehaald";
            prSynchroniseren.IsActive = true;
            prSynchroniseren.Visibility = Windows.UI.Xaml.Visibility.Visible;
            await VulRecenteCommentaren();

            if (Common.InternetControle.ControleerInternet())
            {
                if (await LocalDB.database.ControleerGebruikers())
                {
                    try
                    {
                        await VulBijlagen();

                        prSynchroniseren.IsActive = false;
                        prSynchroniseren.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    }
                    catch (Exception) 
                    {
                    }
                }
                else
                {
                    lblBijlagenVullen.Text = "Er ging iets verkeerd. Probeer opnieuw te synchroniseren";
                    prSynchroniseren.IsActive = false;
                    prSynchroniseren.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

                    btnSyncInternet.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    btnSyncUSB.Visibility = Windows.UI.Xaml.Visibility.Visible;
                }
            }
            else
            {
                lblBijlagenVullen.Text = "Er ging iets verkeerd. Probeer opnieuw te synchroniseren";
                prSynchroniseren.IsActive = false;
                prSynchroniseren.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

                btnSyncInternet.Visibility = Windows.UI.Xaml.Visibility.Visible;
                btnSyncUSB.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
        }

        /// <summary>
        /// Calls the method in the local database which copies the files from an USB drive to a local folder (AppData)
        /// After this is done the program will continue its usual procedure
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BtnSyncUSB_Click(object sender, RoutedEventArgs e)
        {
            lblBijlagenVullen.Text = "Even geduld...";
            lblBijlagenVullenStatus.Text = "Gegevens worden van het USB-apparaat afgehaald";
            prSynchroniseren.IsActive = true;
            prSynchroniseren.Visibility = Windows.UI.Xaml.Visibility.Visible;
            await VulRecenteCommentaren();

            if (await LocalDB.database.SynchroniseerVanUSB()) //Als het ophalen van de bestanden geslaagd is...
            {
                lblBijlagenVullenStatus.Text = "Gegevens zijn met succes van het USB-apparaat afgehaald";
                prSynchroniseren.IsActive = false;
                prSynchroniseren.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

                btnGelezen.IsEnabled = true;
                await VulScrollViewerMetAfbeeldingen();
                btnSyncUSB.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                btnSyncInternet.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                StartVoorbereidingen();
            }
            else
            {
                lblBijlagenVullen.Text = "Er ging iets verkeerd. Probeer opnieuw te synchroniseren";
                prSynchroniseren.IsActive = false;
                prSynchroniseren.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

                btnSyncInternet.Visibility = Windows.UI.Xaml.Visibility.Visible;
                btnSyncUSB.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Administratie));
        }
    }
}