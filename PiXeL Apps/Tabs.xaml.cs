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
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace PiXeL_Apps
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class Tabs : Page
    {
        private List<KeyValuePair<string, string>> inhoudExcel;
        private double oliepeil;
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private static List<string> pdfNamen = new List<string>();
        private static IReadOnlyList<StorageFolder> storageFolders;
        private static string panel;
        private static int geselecteerdScript;
        private Point beginPunt;
        private UserControls.Menu ucMenu;
        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// In deze constructor worden de methodes VulVoorschriften, Vulwagenmap en haalGewichtenOp opgeroepen
        /// </summary>
        public Tabs()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;

            inhoudExcel = new List<KeyValuePair<string, string>>();

            slOliepeil.Value = OilOdoInput.GetOliepeil() / 100;

            //Dynamisch menu (usercontrol) inladen
            ucMenu = new UserControls.Menu(false);
            menuPanel.Children.Add(ucMenu);

            //Pagina grid linken aan twee events die lijsteren naar gestures
            paginaGrid.ManipulationDelta += PaginaGrid_ManipulationDelta;
            paginaGrid.ManipulationStarted += PaginaGrid_ManipulationStarted;

            try
            {
                HaalGewichtenOp();
                VulVoorschriften();
                VulWagenmap();
                vulRijinstructies();
              
            }
            catch (Exception)
            {
                lblErrorVoorschriften.Text = "De pdf kon niet geladen worden, gelieven de tablet opnieuw op te starten indien mogenlijk";
                lblErrorWagenmap.Text = "De pdf kon niet geladen worden, gelieven de tablet opnieuw op te starten indien mogenlijk";
            }
        }

        /// <summary>
        /// Get's called when a swype gesture starts.
        /// The difference between ManipulationStarting and this method is that this function needs
        /// the absolute starting point (X & Y).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PaginaGrid_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            beginPunt = e.Position;
        }

        /// <summary>
        /// Called during swyoe
        /// Checks if the swype is done (e.IsInertial) and when it's done determines the end point.
        /// there will be checked if the distance on the X-axis is far enough to start the menu animation.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PaginaGrid_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (e.IsInertial)
            {
                Point eindPunt = e.Position;
                double afstand = eindPunt.X - beginPunt.X;
                if (afstand >= 500)//500 is the threshold value, where you want to trigger the swipe right event
                {
                    e.Complete();
                    if (!ucMenu.IsMenuOpen())
                        ucMenu.BeginMenuAnimatie();
                }
                else if (afstand <= -500)
                {
                    e.Complete();
                    if (ucMenu.IsMenuOpen())
                        ucMenu.BeginMenuAnimatie();
                }
            }
        }
        /// <summary>
        /// Het ophalen van de gewichten, deze worden op de wagenmap getoond
        /// </summary>
        public async void HaalGewichtenOp()
        {
            var lijstGewichten = await LocalDB.database.GetGewichtenExcel();
            if (lijstGewichten!= null)
            {
                lblVooraanRechts.Text = lijstGewichten.ElementAt(1);
                lblVooraanLichts.Text = lijstGewichten.ElementAt(0);
                lblAchteraanRechts.Text = lijstGewichten.ElementAt(3);
                lblAchteraanMidden.Text = lijstGewichten.ElementAt(4);
                lblAchteraanLinks.Text = lijstGewichten.ElementAt(2);
            }
            else
            {
                //lblMelding.Foreground = new SolidColorBrush(Colors.Yellow);
                //lblMelding.Text = "Door een fout kunnen de gegevens niet getoond worden, gelieve uw administrator te contracteren";
            }
        }

        /// <summary>
        /// De pdfNamen en storageFolders worden opgevuld
        /// </summary>
        /// <returns></returns>
        private async Task HaalPdfNamenOp()
        {
            storageFolders = await (await Windows.Storage.ApplicationData.Current.LocalFolder.GetFolderAsync(LocalDB.database.GetAutoId())).GetFoldersAsync();
            pdfNamen = await PdfSupport.pdfSupport.GetPdfNamen(false);
        }
        /// <summary>
        /// Via deze methode wordt de wagenmap opgehaald en getoont op het scherm
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
                    panel = "wagenspecificaties";
                    try
                    {
                        await VoegPdfToeAanBijlagen(pdfNaam, panel);
                    }
                    catch (NullReferenceException)
                    {
                        TextBlock txtFout = new TextBlock();
                        txtFout.Text = pdfNaam + " bevindt zich niet op deze tablet.";
                        txtFout.FontSize = 25;
                        txtFout.Margin = new Thickness(30, 20, 0, 10);
                        wagenMapPanel.Children.Add(txtFout);
                    }
                }
            }
        }

     
        /// <summary>
        /// Via deze methode worden de voorschiften opgehaald en getoont op het scherm
        /// </summary>
        private async void VulVoorschriften()
        {
            if (pdfNamen.Count == 0)
            {
                await HaalPdfNamenOp();
            }

            foreach (string pdfNaam in pdfNamen)
            {
                if (pdfNaam.ToLower().Equals("testvoorschriften"))
                {
                    panel = "voorschriften";
                    try
                    {
                        await VoegPdfToeAanBijlagen(pdfNaam, panel);
                    }
                    catch (NullReferenceException)
                    {
                        TextBlock txtFout = new TextBlock();
                        txtFout.Text = pdfNaam + " bevindt zich niet op deze tablet.";
                        txtFout.FontSize = 25;
                        txtFout.Margin = new Thickness(30, 20, 0, 10);
                        voorschriftenPanel.Children.Add(txtFout);
                    }
                }
            }
        }

        private async void vulRijinstructies()
        {
            if (pdfNamen.Count == 0)
            {
                await HaalPdfNamenOp();
            }

            foreach (string pdfNaam in pdfNamen)
            {
                if (pdfNaam.ToLower().Equals("rijinstructies"))
                {
                    panel = "rijinstructies";
                    try
                    {
                        await VoegPdfToeAanBijlagen(pdfNaam, panel);
                    }
                    catch (NullReferenceException)
                    {
                        TextBlock txtFout = new TextBlock();
                        txtFout.Text = pdfNaam + " bevindt zich niet op deze tablet.";
                        txtFout.FontSize = 25;
                        txtFout.Margin = new Thickness(30, 20, 0, 10);
                        rijinstructiesPanel.Children.Add(txtFout);
                    }
                }
            }
        }
        /// <summary>
        /// De pdf's toevoegen aan het Stackpanel
        /// </summary>
        /// <param name="pdfNaam">De naam van de PDF van het type string</param>
        /// <param name="panel">De naam van het panel waarin de pdf's getoond gaan worden van het type String</param>
        /// <returns>Task</returns>
        private async Task VoegPdfToeAanBijlagen(string pdfNaam, string panel)
        {
            try
            {
                var afbeeldingen = await HaalPdfAfbeeldingenOp(pdfNaam);
                foreach (StorageFile pagina in afbeeldingen)
                {
                    //Scrollview voor een afbeelding

                    BitmapImage paginaAfbeelding = new BitmapImage();
                    FileRandomAccessStream stream = (FileRandomAccessStream)await pagina.OpenAsync(FileAccessMode.Read);
                    paginaAfbeelding.SetSource(stream);
                    Image paginaAfbeeldingControl = new Image();
                    paginaAfbeeldingControl.Source = paginaAfbeelding;

                    if (panel.Equals("voorschriften"))
                    {
                        voorschriftenPanel.Children.Add(paginaAfbeeldingControl);
                    }
                    if (panel.Equals("wagenspecificaties"))
                    {
                        wagenMapPanel.Children.Add(paginaAfbeeldingControl);
                    }
                    else
                    {
                        rijinstructiesPanel.Children.Add(paginaAfbeeldingControl);
                    }
                }
            }
            catch (Exception ex)
            {
                paLogging.log.Error(String.Format("De afbeelding kon niet getoond worden op het bijlagenscherm.\n{0}", ex.Message));
            }

        }
        /// <summary>
        /// Het ophalen van de afbeeldingen van een pdf
        /// </summary>
        /// <param name="pdfNaam">De naam van de pdf</param>
        /// <returns>IReadOnlyList met StorageFile instanties</returns>
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


        /// <summary>
        /// Populates the page with content passed during navigation. Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session. The state will be null the first time a page is visited.</param>
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// 
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="GridCS.Common.NavigationHelper.LoadState"/>
        /// and <see cref="GridCS.Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion
        /// <summary>
        /// Zorgt ervoor dat de wagenmap tab zichtbaar wordt
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LblWagenmap_Tapped(object sender, TappedRoutedEventArgs e)
        {
            spVoorschriften.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            spMetingen.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            spRijinstructies.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            spWagenmap.Visibility = Windows.UI.Xaml.Visibility.Visible;

            pageTitle.Text = "Tabs: Wagenspecificaties";
        }
        /// <summary>
        /// Zorgt ervoor dat de metingentab getoont wordt
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LblMetingen_Tapped(object sender, TappedRoutedEventArgs e)
        {
            spWagenmap.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            spVoorschriften.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            spRijinstructies.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            spMetingen.Visibility = Windows.UI.Xaml.Visibility.Visible;

            pageTitle.Text = "Tabs: Metingen";
        }
        /// <summary>
        /// Zorgt ervoor dat de voorschriften tab getoont wordt
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LblVoorschriften_Tapped(object sender, TappedRoutedEventArgs e)
        {
            spWagenmap.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            spMetingen.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            spRijinstructies.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            spVoorschriften.Visibility = Windows.UI.Xaml.Visibility.Visible;

            pageTitle.Text = "Tabs: Voorschriften";
        }

        /// <summary>
        /// Deze methode wordt telkens opgeroepen wanneer de slider verschoven wordt
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SlOliepeil_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            oliepeil = slOliepeil.Value * 100;
        }

        /// <summary>
        /// Via deze methode wordt het ingegeven oliepeil opgeslagen in de LocalDB
        /// Als dit niet gaat dan wordt er een error getoond
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BtnOpslaanMetingen_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //float bandVLinks, bandVRechts, bandALinks, bandARechts;

                //if( float.TryParse(txtBandspanningVLinks.Text, out bandVLinks) &&
                //    float.TryParse(txtBandspanningVRechts.Text, out bandVRechts) &&
                //    float.TryParse(txtBandspanningARechts.Text, out bandARechts) &&
                //    float.TryParse(txtBandspanningALinks.Text, out bandALinks ))
                //{
                //    Common.LocalStorage.localStorage.SlaGegevensOp("bandVLinks", bandVLinks);
                //    Common.LocalStorage.localStorage.SlaGegevensOp("bandVRechts", bandVRechts);
                //    Common.LocalStorage.localStorage.SlaGegevensOp("bandALinks", bandALinks);
                //    Common.LocalStorage.localStorage.SlaGegevensOp("bandARechts", bandARechts);

                    if (await LocalDB.database.UpdateOliepeil(oliepeil.ToString("#.###")))
                    {
                        OilOdoInput.SetOliepeil(oliepeil);
                        lblMelding.Foreground = new SolidColorBrush(Colors.White);
                        lblMelding.Text = "De gegevens werden opgeslagen";
                        //lblError.Foreground = new SolidColorBrush(Colors.White);
                        //lblError.Text = "De gegevens werden opgeslagen";
                    }
                //}
            }
            catch (Exception)
            {
                lblMelding.Foreground = new SolidColorBrush(Colors.Yellow);
                lblMelding.Text = "Er is iets fout gelopen bij de opslag, gelieve dit later opnieuw te proberen";
            }
        }

        private void lblWagenRijinstr_Tapped(object sender, TappedRoutedEventArgs e)
        {
            spWagenmap.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            spMetingen.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            spVoorschriften.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            spRijinstructies.Visibility = Windows.UI.Xaml.Visibility.Visible;

            pageTitle.Text = "Tabs: Rij-instructies";
        }
    }
}