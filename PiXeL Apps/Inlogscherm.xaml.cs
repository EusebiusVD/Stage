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
        /// This contstructor prepares the logger and links te delegate of the logwriter.
        /// GPS is initialized to make shure it's ready when the user asks the script.
        /// The last thing this does is call the user control
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
        /// Method to get the number of the assigned vehicle and show it on the login screen
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
        /// Handles the btnInloggen click.
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
                        if (user.Username.Equals("PXL123"))
                            //Administrator
                            this.Frame.Navigate(typeof(Administratie), user);
                        else
                            this.Frame.Navigate(typeof(Whoopsie));
                    }
                    else
                    {
                        if ((await LocalDB.database.GetToegewezenAuto()) != null)
                        {
                            //Testrijder meegeven
                            List<object> objectenLijst = new List<object>();
                            objectenLijst.Add(user);
                            objectenLijst.Add(true);
                            this.Frame.Navigate(typeof(OilOdoInput), objectenLijst);
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
        /// Tis temporary method makes shure everything from the MS Access database is loaded and put in the
        /// local sqlite database.
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
        /// Method to enlarge a PDF fil
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
        /// Retrieve PDF names and storagefolders
        /// </summary>
        /// <returns>Task</returns>
        private async Task HaalPdfNamenOp()
        {
            storageFolders = await (await Windows.Storage.ApplicationData.Current.LocalFolder.GetFolderAsync(LocalDB.database.GetAutoId())).GetFoldersAsync();
            pdfNamen = await PdfSupport.pdfSupport.GetPdfNamen(true);
        }

        /// <summary>
        /// Shows the vehicle specs
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
        /// Adds PDF's to the stackpanel
        /// </summary>
        /// <param name="pdfNaam">Name of the PDF </param>
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
        /// Gets the images of the PDF's
        /// </summary>
        /// <param name="pdfNaam">Name of the PDF</param>
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