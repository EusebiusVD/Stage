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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PiXeL_Apps
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Routes : Page
    {
        private UserControls.Menu ucMenu;
        private Point beginPunt;
        private static IReadOnlyList<StorageFolder> storageFolders;
        private static List<string> pdfNamen = new List<string>();
        private static string panel;
        public Routes()
        {
            this.InitializeComponent();

            cbbRoute.PlaceholderText = "Selecteer uw route...";
            cbbRoute.Items.Add("Inter city A");
            cbbRoute.Items.Add("Inter city B");
            cbbRoute.Items.Add("Inter city C");
            cbbRoute.Items.Add("Rural");
            cbbRoute.Items.Add("Highway");
            cbbRoute.Items.Add("Shakedown");
            cbbRoute.Items.Add("Hasselt Stop/Start");
            cbbRoute.Items.Add("Interne Stop/Start");
            cbbRoute.Items.Add("Interne Transit");
            cbbRoute.Items.Add("Interne Highway");
            cbbRoute.Items.Add("Interne Rural");

            //Dynamisch menu (usercontrol) inladen
            ucMenu = new UserControls.Menu(false);
            menuPanel.Children.Add(ucMenu);

            //Pagina grid linken aan twee events die lijsteren naar gestures
            paginaGrid.ManipulationDelta += PaginaGrid_ManipulationDelta;
            paginaGrid.ManipulationStarted += PaginaGrid_ManipulationStarted;
            try
            {
                vulHighway();
                vulIHasseltStopStart();
                vulInterCityA();
                vulInterCityB();
                vulInterCityC();
                vulInternHighway();
                vulInternRural();
                vulInternStopStart();
                vulInternTransit();
                vulRural();
                vulShakedown();
            }
            catch (Exception)
            {
                //lblErrorVoorschriften.Text = "De pdf kon niet geladen worden, gelieven de tablet opnieuw op te starten indien mogenlijk";
                //lblErrorWagenmap.Text = "De pdf kon niet geladen worden, gelieven de tablet opnieuw op te starten indien mogenlijk";
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

        private async void vulInterCityA()
        {
            if (pdfNamen.Count == 0)
            {
                await HaalPdfNamenOp();
            }

            foreach (string pdfNaam in pdfNamen)
            {
                if (pdfNaam.ToLower().Equals("intercitya"))
                {
                    panel = "intercitya";
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
                        IntercityAPanel.Children.Add(txtFout);
                    }
                }
            }
        }

        private async void vulInterCityB()
        {
            if (pdfNamen.Count == 0)
            {
                await HaalPdfNamenOp();
            }

            foreach (string pdfNaam in pdfNamen)
            {
                if (pdfNaam.ToLower().Equals("intercityb"))
                {
                    panel = "intercityb";
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
                        IntercityBPanel.Children.Add(txtFout);
                    }
                }
            }
        }

        private async void vulInterCityC()
        {
            if (pdfNamen.Count == 0)
            {
                await HaalPdfNamenOp();
            }

            foreach (string pdfNaam in pdfNamen)
            {
                if (pdfNaam.ToLower().Equals("intercityc"))
                {
                    panel = "intercityc";
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
                        IntercityCPanel.Children.Add(txtFout);
                    }
                }
            }
        }
        private async void vulHighway()
        {
            if (pdfNamen.Count == 0)
            {
                await HaalPdfNamenOp();
            }

            foreach (string pdfNaam in pdfNamen)
            {
                if (pdfNaam.ToLower().Equals("highway"))
                {
                    panel = "highway";
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
                        HighwayPanel.Children.Add(txtFout);
                    }
                }
            }
        }

        private async void vulRural()
        {
            if (pdfNamen.Count == 0)
            {
                await HaalPdfNamenOp();
            }

            foreach (string pdfNaam in pdfNamen)
            {
                if (pdfNaam.ToLower().Equals("rural"))
                {
                    panel = "rural";
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
                        RuralPanel.Children.Add(txtFout);
                    }
                }
            }
        }

        private async void vulShakedown()
        {
            if (pdfNamen.Count == 0)
            {
                await HaalPdfNamenOp();
            }

            foreach (string pdfNaam in pdfNamen)
            {
                if (pdfNaam.ToLower().Equals("shakedown"))
                {
                    panel = "shakedown";
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
                        ShakedownPanel.Children.Add(txtFout);
                    }
                }
            }
        }
        private async void vulIHasseltStopStart()
        {
            if (pdfNamen.Count == 0)
            {
                await HaalPdfNamenOp();
            }

            foreach (string pdfNaam in pdfNamen)
            {
                if (pdfNaam.ToLower().Equals("hasseltstopstart"))
                {
                    panel = "hasseltstopstart";
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
                        HasseltStopStartPanel.Children.Add(txtFout);
                    }
                }
            }
        }
        private async void vulInternStopStart()
        {
            if (pdfNamen.Count == 0)
            {
                await HaalPdfNamenOp();
            }

            foreach (string pdfNaam in pdfNamen)
            {
                if (pdfNaam.ToLower().Equals("internestartstop"))
                {
                    panel = "internstopstart";
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
                        InterneStopStartPanel.Children.Add(txtFout);
                    }
                }
            }
        }
        private async void vulInternTransit()
        {
            if (pdfNamen.Count == 0)
            {
                await HaalPdfNamenOp();
            }

            foreach (string pdfNaam in pdfNamen)
            {
                if (pdfNaam.ToLower().Equals("internetransit"))
                {
                    panel = "interntransit";
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
                        InterneTransitPanel.Children.Add(txtFout);
                    }
                }
            }
        }

        private async void vulInternHighway()
        {
            if (pdfNamen.Count == 0)
            {
                await HaalPdfNamenOp();
            }

            foreach (string pdfNaam in pdfNamen)
            {
                if (pdfNaam.ToLower().Equals("internehighway"))
                {
                    panel = "internhighway";
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
                        InterneHighwayPanel.Children.Add(txtFout);
                    }
                }
            }
        }
        private async void vulInternRural()
        {
            if (pdfNamen.Count == 0)
            {
                await HaalPdfNamenOp();
            }

            foreach (string pdfNaam in pdfNamen)
            {
                if (pdfNaam.ToLower().Equals("internerural"))
                {
                    panel = "internrural";
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
                        InterneRuralPanel.Children.Add(txtFout);
                    }
                }
            }
        }

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

                    if (panel.Equals("intercitya"))
                    {
                        IntercityAPanel.Children.Add(paginaAfbeeldingControl);
                    }
                    if (panel.Equals("intercityb"))
                    {
                        IntercityBPanel.Children.Add(paginaAfbeeldingControl);
                    }
                    if (panel.Equals("intercityc"))
                    {
                        IntercityCPanel.Children.Add(paginaAfbeeldingControl);
                    }
                    if (panel.Equals("rural"))
                    {
                        RuralPanel.Children.Add(paginaAfbeeldingControl);
                    }
                    if (panel.Equals("highway"))
                    {
                        HighwayPanel.Children.Add(paginaAfbeeldingControl);
                    }
                    if (panel.Equals("shakedown"))
                    {
                        ShakedownPanel.Children.Add(paginaAfbeeldingControl);
                    }
                    if (panel.Equals("hasseltstopstart"))
                    {
                        HasseltStopStartPanel.Children.Add(paginaAfbeeldingControl);
                    }
                    if (panel.Equals("internstopstart"))
                    {
                        InterneStopStartPanel.Children.Add(paginaAfbeeldingControl);
                    }
                    if (panel.Equals("interntransit"))
                    {
                        InterneTransitPanel.Children.Add(paginaAfbeeldingControl);
                    }
                    if (panel.Equals("internhighway"))
                    {
                        InterneHighwayPanel.Children.Add(paginaAfbeeldingControl);
                    }
                    if (panel.Equals("internrural"))
                    {
                        InterneRuralPanel.Children.Add(paginaAfbeeldingControl);
                    }
                }
            }
            catch (Exception ex)
            {
                paLogging.log.Error(String.Format("De afbeelding kon niet getoond worden op het bijlagenscherm.\n{0}", ex.Message));
            }
        }

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

        private async Task HaalPdfNamenOp()
        {
            storageFolders = await (await Windows.Storage.ApplicationData.Current.LocalFolder.GetFolderAsync(LocalDB.database.GetAutoId())).GetFoldersAsync();
            pdfNamen = await PdfSupport.pdfSupport.GetPdfNamen(false);
        }

        private void lblShakedown_Tapped(object sender, TappedRoutedEventArgs e)
        {
            spHasseltStopStart.Visibility = Visibility.Collapsed;
            spHighway.Visibility = Visibility.Collapsed;
            spIntercityA.Visibility = Visibility.Collapsed;
            spIntercityB.Visibility = Visibility.Collapsed;
            spIntercityC.Visibility = Visibility.Collapsed;
            spInterneHighway.Visibility = Visibility.Collapsed;
            spInterneRural.Visibility = Visibility.Collapsed;
            spInterneStopStart.Visibility = Visibility.Collapsed;
            spInterneTransit.Visibility = Visibility.Collapsed;
            spRural.Visibility = Visibility.Collapsed;

            spShakedown.Visibility = Visibility.Visible;
        }

        private void lblIntercityA_Tapped(object sender, TappedRoutedEventArgs e)
        {
            spHasseltStopStart.Visibility = Visibility.Collapsed;
            spHighway.Visibility = Visibility.Collapsed;
            spShakedown.Visibility = Visibility.Collapsed;
            spIntercityB.Visibility = Visibility.Collapsed;
            spIntercityC.Visibility = Visibility.Collapsed;
            spInterneHighway.Visibility = Visibility.Collapsed;
            spInterneRural.Visibility = Visibility.Collapsed;
            spInterneStopStart.Visibility = Visibility.Collapsed;
            spInterneTransit.Visibility = Visibility.Collapsed;
            spRural.Visibility = Visibility.Collapsed;
            spIntercityA.Visibility = Visibility.Visible;
        }

        private void lblIntercityB_Tapped(object sender, TappedRoutedEventArgs e)
        {
            spHasseltStopStart.Visibility = Visibility.Collapsed;
            spHighway.Visibility = Visibility.Collapsed;
            spShakedown.Visibility = Visibility.Collapsed;
            spIntercityB.Visibility = Visibility.Visible;
            spIntercityC.Visibility = Visibility.Collapsed;
            spInterneHighway.Visibility = Visibility.Collapsed;
            spInterneRural.Visibility = Visibility.Collapsed;
            spInterneStopStart.Visibility = Visibility.Collapsed;
            spInterneTransit.Visibility = Visibility.Collapsed;
            spRural.Visibility = Visibility.Collapsed;
            spIntercityA.Visibility = Visibility.Collapsed;
        }

        private void lblIntercityC_Tapped(object sender, TappedRoutedEventArgs e)
        {
            spHasseltStopStart.Visibility = Visibility.Collapsed;
            spHighway.Visibility = Visibility.Collapsed;
            spShakedown.Visibility = Visibility.Collapsed;
            spIntercityB.Visibility = Visibility.Collapsed;
            spIntercityC.Visibility = Visibility.Visible;
            spInterneHighway.Visibility = Visibility.Collapsed;
            spInterneRural.Visibility = Visibility.Collapsed;
            spInterneStopStart.Visibility = Visibility.Collapsed;
            spInterneTransit.Visibility = Visibility.Collapsed;
            spRural.Visibility = Visibility.Collapsed;
            spIntercityA.Visibility = Visibility.Collapsed;
        }

        private void lblRural_Tapped(object sender, TappedRoutedEventArgs e)
        {
            spHasseltStopStart.Visibility = Visibility.Collapsed;
            spHighway.Visibility = Visibility.Collapsed;
            spShakedown.Visibility = Visibility.Collapsed;
            spIntercityB.Visibility = Visibility.Collapsed;
            spIntercityC.Visibility = Visibility.Collapsed;
            spInterneHighway.Visibility = Visibility.Collapsed;
            spInterneRural.Visibility = Visibility.Collapsed;
            spInterneStopStart.Visibility = Visibility.Collapsed;
            spInterneTransit.Visibility = Visibility.Collapsed;
            spRural.Visibility = Visibility.Visible;
            spIntercityA.Visibility = Visibility.Collapsed;
        }

        private void lblHighway_Tapped(object sender, TappedRoutedEventArgs e)
        {
            spHasseltStopStart.Visibility = Visibility.Collapsed;
            spHighway.Visibility = Visibility.Visible;
            spShakedown.Visibility = Visibility.Collapsed;
            spIntercityB.Visibility = Visibility.Collapsed;
            spIntercityC.Visibility = Visibility.Collapsed;
            spInterneHighway.Visibility = Visibility.Collapsed;
            spInterneRural.Visibility = Visibility.Collapsed;
            spInterneStopStart.Visibility = Visibility.Collapsed;
            spInterneTransit.Visibility = Visibility.Collapsed;
            spRural.Visibility = Visibility.Collapsed;
            spIntercityA.Visibility = Visibility.Collapsed;
        }

        private void lblHasseltStopStart_Tapped(object sender, TappedRoutedEventArgs e)
        {
            spHasseltStopStart.Visibility = Visibility.Visible;
            spHighway.Visibility = Visibility.Collapsed;
            spShakedown.Visibility = Visibility.Collapsed;
            spIntercityB.Visibility = Visibility.Collapsed;
            spIntercityC.Visibility = Visibility.Collapsed;
            spInterneHighway.Visibility = Visibility.Collapsed;
            spInterneRural.Visibility = Visibility.Collapsed;
            spInterneStopStart.Visibility = Visibility.Collapsed;
            spInterneTransit.Visibility = Visibility.Collapsed;
            spRural.Visibility = Visibility.Collapsed;
            spIntercityA.Visibility = Visibility.Collapsed;
        }

        private void lblInternStopStart_Tapped(object sender, TappedRoutedEventArgs e)
        {
            spHasseltStopStart.Visibility = Visibility.Collapsed;
            spHighway.Visibility = Visibility.Collapsed;
            spShakedown.Visibility = Visibility.Collapsed;
            spIntercityB.Visibility = Visibility.Collapsed;
            spIntercityC.Visibility = Visibility.Collapsed;
            spInterneHighway.Visibility = Visibility.Collapsed;
            spInterneRural.Visibility = Visibility.Collapsed;
            spInterneStopStart.Visibility = Visibility.Visible;
            spInterneTransit.Visibility = Visibility.Collapsed;
            spRural.Visibility = Visibility.Collapsed;
            spIntercityA.Visibility = Visibility.Collapsed;
        }

        private void lblInternTransit_Tapped(object sender, TappedRoutedEventArgs e)
        {
            spHasseltStopStart.Visibility = Visibility.Collapsed;
            spHighway.Visibility = Visibility.Collapsed;
            spShakedown.Visibility = Visibility.Collapsed;
            spIntercityB.Visibility = Visibility.Collapsed;
            spIntercityC.Visibility = Visibility.Collapsed;
            spInterneHighway.Visibility = Visibility.Collapsed;
            spInterneRural.Visibility = Visibility.Collapsed;
            spInterneStopStart.Visibility = Visibility.Collapsed;
            spInterneTransit.Visibility = Visibility.Visible;
            spRural.Visibility = Visibility.Collapsed;
            spIntercityA.Visibility = Visibility.Collapsed;
        }

        private void lblInternHighway_Tapped(object sender, TappedRoutedEventArgs e)
        {
            spHasseltStopStart.Visibility = Visibility.Collapsed;
            spHighway.Visibility = Visibility.Collapsed;
            spShakedown.Visibility = Visibility.Collapsed;
            spIntercityB.Visibility = Visibility.Collapsed;
            spIntercityC.Visibility = Visibility.Collapsed;
            spInterneHighway.Visibility = Visibility.Visible;
            spInterneRural.Visibility = Visibility.Collapsed;
            spInterneStopStart.Visibility = Visibility.Collapsed;
            spInterneTransit.Visibility = Visibility.Collapsed;
            spRural.Visibility = Visibility.Collapsed;
            spIntercityA.Visibility = Visibility.Collapsed;
        }

        private void lblInternRural_Tapped(object sender, TappedRoutedEventArgs e)
        {
            spHasseltStopStart.Visibility = Visibility.Collapsed;
            spHighway.Visibility = Visibility.Collapsed;
            spShakedown.Visibility = Visibility.Collapsed;
            spIntercityB.Visibility = Visibility.Collapsed;
            spIntercityC.Visibility = Visibility.Collapsed;
            spInterneHighway.Visibility = Visibility.Collapsed;
            spInterneRural.Visibility = Visibility.Visible;
            spInterneStopStart.Visibility = Visibility.Collapsed;
            spInterneTransit.Visibility = Visibility.Collapsed;
            spRural.Visibility = Visibility.Collapsed;
            spIntercityA.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// This method handles the navigation of the back button to the Hoofdscherm screen.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Hoofdscherm));
        }

        private void cbbRoute_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string route = cbbRoute.SelectedValue.ToString();
            route = route.Replace(" ", "");
            route = route.Replace("/", "");
            route = "sp" + route;
            spHasseltStopStart.Visibility = Visibility.Collapsed;
            spHighway.Visibility = Visibility.Collapsed;
            spShakedown.Visibility = Visibility.Collapsed;
            spIntercityB.Visibility = Visibility.Collapsed;
            spIntercityC.Visibility = Visibility.Collapsed;
            spInterneHighway.Visibility = Visibility.Collapsed;
            spInterneRural.Visibility = Visibility.Collapsed;
            spInterneStopStart.Visibility = Visibility.Collapsed;
            spInterneTransit.Visibility = Visibility.Collapsed;
            spRural.Visibility = Visibility.Collapsed;
            spIntercityA.Visibility = Visibility.Collapsed;

            if (spHighway.Name.Equals(route))
            {
                spHighway.Visibility = Visibility.Visible;
            }
            if (spIntercityA.Name.Equals(route))
            {
                spIntercityA.Visibility = Visibility.Visible;
            }
            if (spIntercityB.Name.Equals(route))
            {
                spIntercityB.Visibility = Visibility.Visible;
            }
            if (spIntercityC.Name.Equals(route))
            {
                spIntercityC.Visibility = Visibility.Visible;
            }
            if (spInterneHighway.Name.Equals(route))
            {
                spInterneHighway.Visibility = Visibility.Visible;
            }
            if (spInterneRural.Name.Equals(route))
            {
                spInterneRural.Visibility = Visibility.Visible;
            }
            if (spInterneStopStart.Name.Equals(route))
            {
                spInterneStopStart.Visibility = Visibility.Visible;
            }
            if (spInterneTransit.Name.Equals(route))
            {
                spInterneTransit.Visibility = Visibility.Visible;
            }
            if (spRural.Name.Equals(route))
            {
                spRural.Visibility = Visibility.Visible;
            }
            if (spShakedown.Name.Equals(route))
            {
                spShakedown.Visibility = Visibility.Visible;
            }
            if (spHasseltStopStart.Name.Equals(route))
            {
                spHasseltStopStart.Visibility = Visibility.Visible;
            }
        }
    }
}