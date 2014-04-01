using PiXeL_Apps.Classes;
using PiXeL_Apps.Common;
using PiXeL_Apps.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using WinRTXamlToolkit.Controls.Extensions;


namespace PiXeL_Apps
{
    public sealed partial class Inspecties : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private GpsSupport gpsSupport = new GpsSupport();

        private static string geselecteerdScript;
        private static List<Inspectie> inspecties = new List<Inspectie>();
        private static List<Inspectie> gewijzigdeInspecties = new List<Inspectie>();
        private static List<object> geselecteerdeInspecties = new List<object>();

        private Point beginPunt; private UserControls.Menu ucMenu;
        private ScrollViewer scrollViewer; //ScrollViewer van gvwInspecties
        private Inspectie scrollenNaarInspectie;

        #region Initialisatie

        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// This constructor add the menu to the screen, registers GPS updates and the swype commands for the menu
        /// </summary>
        public Inspecties()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += NavigationHelper_LoadState;
            this.navigationHelper.SaveState += NavigationHelper_SaveState;
            this.Loaded += Inspecties_Loaded;
            GpsSupport.gpsSupport.LocatieUpdate += LocatieUpdate;

            //Dynamisch menu (usercontrol inladen)
            ucMenu = new UserControls.Menu(false);
            menuPanel.Children.Add(ucMenu);

            //Pagina grid linken aan twee events die lijsteren naar gestures
            paginaGrid.ManipulationDelta += PaginaGrid_ManipulationDelta;
            paginaGrid.ManipulationStarted += PaginaGrid_ManipulationStarted;
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
        /// This method will set the list of inspections as ItemSource for the grid as soon as the page is loaded.
        /// Inspections that are completed will be SelectedItems
        /// In deze methode zal zodra de pagina geladen is, de lijst met inspecties als ItemSource voor het grid instellen.
        /// Inspecties die eerder afgerond werden worden aan de SelectedItems van het grid toegevoegd zodat deze worden aangevinkt.
        /// Tenslotte wordt de eerste inspectie in de lijst die niet aangevinkt werd in beeld gescrolled. Indien een dringend 
        /// (volgens de gps) eerst komt wordt deze in beeld gebracht.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Inspecties_Loaded(object sender, RoutedEventArgs e)
        {
            gvwInspecties.HorizontalContentAlignment = Windows.UI.Xaml.HorizontalAlignment.Stretch;
            gvwInspecties.ItemsSource = inspecties;
            bool gescrolled = false; //Houd bij of naar het eerste onvoldooide activiteit gescrolled werd of niet.
            foreach (Inspectie inspectie in inspecties)
            {
                if (inspectie.Status)
                    gvwInspecties.SelectedItems.Add(inspectie);
                else if (!gescrolled)
                {
                    gescrolled = true;
                    scrollenNaarInspectie = inspectie;
                }
            }
            if (scrollenNaarInspectie != null)
                gvwInspecties.ScrollIntoView(scrollenNaarInspectie, ScrollIntoViewAlignment.Leading);

            //scrollViewer van gridview ophalen is niet mogelijk in standaard RT - wel met WinRTXamlToolkit
            scrollViewer = gvwInspecties.GetFirstDescendantOfType<ScrollViewer>();
        }

        #endregion

        #region GridView Events

        /// <summary>
        /// Bij het klikken op een item in het grid wordt het item aangepast in de lijst met gewijzigde inspecties.
        /// Bovendien worden alle items in de geselecteerde items van het grid geüpdatet zodat deze aan- of afgevinkt worden.
        /// Er wordt bovendien terug naar de laatst geklikte positie gescrollen (geklikte grid tegen te linkerkant)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gvwInspecties_ItemClick(object sender, ItemClickEventArgs e)
        {
            Inspectie geklikteInspectie = (Inspectie)e.ClickedItem;
            int inspectieIndex = inspecties.IndexOf(geklikteInspectie);
            bool eerderGewijzigd = gewijzigdeInspecties.Contains(geklikteInspectie);
            if (eerderGewijzigd)
                gewijzigdeInspecties.Remove(geklikteInspectie);
            geklikteInspectie.Status = !geklikteInspectie.Status;
            inspecties.RemoveAt(inspectieIndex);
            inspecties.Insert(inspectieIndex, geklikteInspectie);
            if (inspectieIndex + 1 % 9 == 0)
            {
                gvwInspecties.ScrollIntoView(geklikteInspectie, ScrollIntoViewAlignment.Leading);
            }

            try
            {
                gvwInspecties.SelectedItems.Clear();
                foreach (Inspectie inspectie in inspecties)
                {
                    if (inspectie.Status)
                        gvwInspecties.SelectedItems.Add(inspectie);
                }
            }
            catch (COMException)
            {
                paLogging.log.Error(String.Format("Inspectie '{0}' kon niet aan- of afgevinkt worden.", geklikteInspectie.Activity));
            }

            if (inspectieIndex % 9 == 0)
            {
                gvwInspecties.ScrollIntoView(geklikteInspectie, ScrollIntoViewAlignment.Leading);
            } //terug scrollen naar huidige positie

            if (!eerderGewijzigd)
                gewijzigdeInspecties.Add(geklikteInspectie);

        }

        #endregion

        #region Button Clicks

        /// <summary>
        /// Deze methode zorgt ervoor dat er naar het opmerkingenscherm genavigeerd kan worden
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnOpmerkingen_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(ProblemenTest));
        }

        /// <summary>
        /// Deze methode zorgt ervoor dat er naar het "overzicht opmerkingen" (OverzichtOpmerkingen) scherm genavigeerd kan worden
        /// Ook wordt de GegevensOpslaan() methode geladen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnVerwerk_Click(object sender, RoutedEventArgs e)
        {
            GegevensOpslaan();
            this.Frame.Navigate(typeof(OverzichtOpmerkingen), new List<object>(gvwInspecties.Items));
        }

        /// <summary>
        /// Deze methode zorgt ervoor dat er naar het hoofdscherm genavigeerd kan worden en roept de methode Gegevensopslaan() op
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            GegevensOpslaan();
            this.Frame.Navigate(typeof(Hoofdscherm));
        }

        /// <summary>
        /// Bij muisklik op de terug knop controleren of de gebruiker de gegevens wil opslaan
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            GegevensOpslaan();
        }

        #endregion

        #region Dialogen

        /// <summary>
        /// Toont een messagedialog met een aangepast bericht, titel, en knopteksten (ok en annuleer)
        /// </summary>
        /// <param name="bericht">Bericht van het type string, Inhoud van het bericht</param>
        /// <param name="titel">Titel van het type string, Titel van het bericht</param>
        /// <param name="okTekst">Tekst van het type string, Tekst voor OK knop</param>
        /// <param name="annuleerTekst">Tekst van het type string, Tekst voor annuleer knop</param>
        /// <returns></returns>
        public async Task<bool> ToonOkAnnuleer(string bericht, string titel, string okTekst = "OK", string annuleerTekst = "Annuleer")
        {
            MessageDialog okAnnuleer = new MessageDialog(bericht, titel);
            okAnnuleer.Commands.Add(new UICommand(okTekst));
            okAnnuleer.Commands.Add(new UICommand(annuleerTekst));
            var antwoord = await okAnnuleer.ShowAsync();
            if (antwoord.Label.Equals(okTekst))
                return true;
            else
                return false;
        }

        #endregion

        #region Toegangsmethoden

        /// <summary>
        /// Deze methode wordt opgeroepen bij het verlaten van de pagina.
        /// Als de testrijder ervoor kiest om zijn gegevens op te slaan zodat deze na een herstart gelezen kunnen worden, worden alle
        /// gewijzigde items naar de SQLite database weg geschreven.
        /// </summary>
        private async void GegevensOpslaan()
        {
            if (gewijzigdeInspecties.Count > 0 && await ToonOkAnnuleer("Wilt u uw laatste wijzigingen opslaan voor u verder gaat?", "Opslaan bevestigen", "OK", "Annuleer"))
            {
                await LocalDB.database.UpdateInspecties(gewijzigdeInspecties, inspecties);
                gewijzigdeInspecties = new List<Inspectie>();
            }
        }

        /// <summary>
        /// Deze methode wordt op voorhand (inlogscherm) opgeroepen nadat de gebruiker zijn correcte inloggegevens heeft ingevuld.
        /// Op deze manier kunnen alle inspecties op de achtergrond reeds geladen worden voor de pagina geladen is.
        /// </summary>
        public static async void HaalInspectiesOp(List<Inspectie> tePlaatsenInspecties)
        {
            if (tePlaatsenInspecties.Count == 0)
            {
                if (inspecties.Count == 0) //Kan enkel als er geen inspecties aanwezig zijn.
                {
                    CompleteAuto completeAuto = await LocalDB.database.GetToegewezenAuto();
                    geselecteerdScript = completeAuto != null ? completeAuto.Number : String.Empty;
                    if (!geselecteerdScript.Equals(String.Empty))
                    {
                        inspecties = await LocalDB.database.GetInspecties();
                        if (inspecties != null && inspecties.Count > 0)
                            inspecties.Sort((x, y) => x.Kilometerstand.CompareTo(y.Kilometerstand)); //Vergelijk de kilometers in de inspectie-instanties om ze te sorteren.
                        else
                            inspecties = new List<Inspectie>();
                    }
                }
            }
            else
                inspecties = tePlaatsenInspecties;
        }

        /// <summary>
        /// Methode die luistert naar updates van de Geolocator.
        /// De ItemTemplateSelector wordt opnieuw aan het gridview toegewezen wat ervoor zorgt dat elk item opnieuw een DataTemplate
        /// toegewezen krijgt naargelang de gereden kilometers.
        /// </summary>
        /// <param name="kilometersGereden">kilometers is van het type int</param>
        private void LocatieUpdate(int kilometersGereden)
        {
            this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                foreach (Inspectie inspectie in inspecties)
                {
                    if (kilometersGereden > inspectie.Kilometerstand - 501 && kilometersGereden < inspectie.Kilometerstand)
                    {
                        scrollenNaarInspectie = inspectie;
                        break;
                    }
                }
                DataTemplateSelector selector = gvwInspecties.ItemTemplateSelector;
                gvwInspecties.ItemTemplateSelector = null;
                gvwInspecties.ItemTemplateSelector = selector;
                gvwInspecties.ScrollIntoView(scrollenNaarInspectie);
            }).AsTask().Wait();
                }

        #endregion

        #region NavigationHelper

        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
            {
            }

        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion
    }
}