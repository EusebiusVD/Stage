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
using Windows.UI;
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
    
    public sealed partial class Inlogscherm : Page
    {
        private Boolean afbeeldingVergroot = false;
        private static List<string> pdfNamen = new List<string>();
        private static IReadOnlyList<StorageFolder> storageFolders;
        private static List<string> PDFBestanden = new List<string>();

        /// <summary>
        /// in deze constructor wordt de logging instantie aangemaakt en de delegate van de logwriter gelinkt
        /// GPS wordt geïnitialiseerd, zodat deze klaar is vooraleer de gebruiker een script opvraagt en de User Control wordt opgeroepen
        /// </summary>
        public Inlogscherm()
        {
            this.InitializeComponent();
            ToonWagenNummer();
            VulWagenmap();
            GpsSupport.gpsSupport.InitialiseerGeoLocator(500); //Gegevens uit de databank halen
            //Event voor het kijken naar internetconnectie
        }
        /// <summary>
        /// Methode om het wagennummer op te halen en te tonen op het inlogscherm
        /// </summary>
        private async void ToonWagenNummer()
        {
            CompleteAuto caAuto = await LocalDB.database.GetToegewezenAuto();
            if (!caAuto.Equals(null))
            {
                lblWagen.Text = "Wagen: " + caAuto.Number;
                lblWagen.Foreground = new SolidColorBrush(Colors.White);
                
            }
            else
            {
                lblWagen.Text = "Er is geen wagen toegewezen, gelieve een beheerder te contacteren.";
                lblWagen.Foreground = new SolidColorBrush(Colors.Yellow);
            }
            
        }

        /// <summary>
        /// Deze methode zorgt ervoor dat er naar het hoofdscherm genavigeerd kan worden of naar het admin-gedeelte
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BtnInloggen_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string username = txtPersoneelsnummer.Text;
                string password = txtWachtwoord.Password;

                User user = await LocalDB.database.GetUser(username, password);

                if (user != null)
                {
                    if (user.Admin)
                    {
                        //Administrator
                        this.Frame.Navigate(typeof(Administratie), user);
                    }
                    else
                    {
                        if ((await LocalDB.database.GetToegewezenAuto()) != null)
                        {
                            //Testrijder meegeven
                            List<object> objectenLijst = new List<object>();
                            objectenLijst.Add(user);
                            objectenLijst.Add(true);
                            this.Frame.Navigate(typeof(Hoofdscherm), objectenLijst);
                        }
                        else
                        {
                            MessageDialog okAnnuleer = new MessageDialog("Er is nog geen wagen aan de tablet toegewezen.", "Geen Wagen Toegewezen");
                            okAnnuleer.Commands.Add(new UICommand("Begrepen!"));
                            var antwoord = okAnnuleer.ShowAsync();

                            paLogging.log.Info("Er werd nog geen voertuig aan de tablet toegewezen, een administrator moet een nieuwe wagen toewijzen.");
                        }
                    }
                }
                else
                {
                    lblErrorInlog.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                paLogging.log.Critical("Er ging iets fout bij het inloggen van een bestuurder: " + ex.Message);
            }
        }

        /// <summary>
        /// Deze tijdelijke methode zorgt ervoor dat alles uit de MS Access databank ingeladen wordt en in de LocalDB geplaatst wordt.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BtnTEMPDatabank_Click(object sender, RoutedEventArgs e)
        {
            paLogWriter writer = new paLogWriter(); //logwriter aanmaken, wordt verder niet gebruikt
            await LocalDB.database.CreateDatabase(true);
            paLogging.log.Debug("Database aangemaakt");
        }

        /// <summary>
        /// Via deze methode kan een pdf vergroot worden 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImgWagenmap_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (!afbeeldingVergroot)
            {
                grInloggen.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                //imgWagenmap
                wagenMapGrid.Width = rootGrid.Width - 20;
                wagenMapGrid.Height = rootGrid.Height - 20;

                wagenMapGrid.Margin = new Thickness(10, 10, 10, 10);
            }
            else
            {
                grInloggen.Visibility = Windows.UI.Xaml.Visibility.Visible;
                wagenMapGrid.Margin = new Thickness(585, 10, 10, 10);
            }

            afbeeldingVergroot = !afbeeldingVergroot;
        }

        #region Initialisatie

        /// <summary>
        /// Het ophalen van de pdfnamen en de storagefolders
        /// </summary>
        /// <returns>Task</returns>
        private async Task HaalPdfNamenOp()
        {
            storageFolders = await (await Windows.Storage.ApplicationData.Current.LocalFolder.GetFolderAsync(LocalDB.database.GetAutoId())).GetFoldersAsync();
            pdfNamen = await PdfSupport.pdfSupport.GetPdfNamen(true);
        }

        /// <summary>
        /// Via deze methode wordt de wagenmap getoont op het scherm
        /// </summary>
        private async void VulWagenmap()
        {
            if (pdfNamen.Count == 0)
            {
                   await HaalPdfNamenOp();
            }

            foreach (string pdfNaam in pdfNamen)
            {
                if (pdfNaam.ToLower().Equals("wagenspecificaties"))
                {
                    try
                    {
                    TextBlock txtPdfTitel = new TextBlock();
                        txtPdfTitel.Text = pdfNaam;
                        txtPdfTitel.FontSize = 48;
                        txtPdfTitel.TextAlignment = TextAlignment.Center;
                        txtPdfTitel.Width = wagenMapGrid.Width - 20;
                        txtPdfTitel.Margin = new Thickness(10, 20, 10, 10);
                        wagenMapPanel.Children.Add(txtPdfTitel);
                        await VoegPdfToeAanBijlagen(pdfNaam);
                    }
                    catch (NullReferenceException)
                    {
                        TextBlock txtFout = new TextBlock();
                        txtFout.Text = pdfNamen + " bevindt zich niet op deze tablet.";
                        txtFout.FontSize = 48;
                        txtFout.Margin = new Thickness(10, 0, 10, 10);
                        wagenMapPanel.Children.Add(txtFout);
                    }
                }
            }
        }
        /// <summary>
        /// De pdf's worden toegevoegd aan het Stackpanel
        /// </summary>
        /// <param name="pdfNaam">De naam van de PDF van het type string</param>
        /// <returns>Task</returns>
        private async Task VoegPdfToeAanBijlagen(string pdfNaam)
        {
            try
            {
                var afbeeldingen = await HaalPdfAfbeeldingenOp(pdfNaam);
                foreach (StorageFile pagina in afbeeldingen)
                {
                    //Scrollview voor een afbeelding
                    var scroll = new ScrollViewer();
                    scroll.HorizontalScrollMode = ScrollMode.Auto;
                    scroll.VerticalScrollMode = ScrollMode.Auto;
                    scroll.ZoomMode = ZoomMode.Enabled;

                    BitmapImage paginaAfbeelding = new BitmapImage();
                    FileRandomAccessStream stream = (FileRandomAccessStream)await pagina.OpenAsync(FileAccessMode.Read);
                    paginaAfbeelding.SetSource(stream);
                    Image paginaAfbeeldingControl = new Image();
                    paginaAfbeeldingControl.Source = paginaAfbeelding;
                    paginaAfbeeldingControl.Tapped += ImgWagenmap_Tapped;
                    scroll.Content = paginaAfbeeldingControl;

                    wagenMapPanel.Children.Add(scroll);                    
                }
            }
            catch (Exception ex)
            {
                paLogging.log.Error(String.Format("De afbeelding kon niet getoond worden op het bijlagenscherm.\n{0}", ex.Message));
            }
        }
        /// <summary>
        /// De afbeeldingen van een pdf worden opgehaald
        /// </summary>
        /// <param name="pdfNaam">De naam van de PDF van het type string</param>
        /// <returns>Task</returns>
        private async Task<IReadOnlyList<StorageFile>> HaalPdfAfbeeldingenOp(string pdfNaam)
        {
            IEnumerable<StorageFolder> afbeeldingFolder =
                from f in storageFolders
                where f.Name.Equals(pdfNaam)
                select f;

            StorageFolder folder;
            if (afbeeldingFolder.Any())
        {
                folder = afbeeldingFolder.First();
                return await folder.GetFilesAsync();
            }
            return null;
        }
        #endregion
    }
}