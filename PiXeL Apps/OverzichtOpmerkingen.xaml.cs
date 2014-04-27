using PiXeL_Apps.Classes;
using PiXeL_Apps.Common;
using PiXeL_Apps.SQLite.Tables;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace PiXeL_Apps
{

    public sealed partial class OverzichtOpmerkingen : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private static List<Comment> opmerkingen = new List<Comment>();
        private static List<DefectCodes> defectCodes = new List<DefectCodes>();
        private static List<ObjectCodes> objectCodes = new List<ObjectCodes>();
        private static List<Comment> Opmerking = new List<Comment>();
        private static int geselecteerdeIndex = 0;
        private UserControls.Menu ucMenu;
        private Point beginPunt;

        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }
        /// <summary>
        /// In this constructor the method VulGridview is called.
        /// The usercontrol for the dynamic menu is also called.
        /// In deze constructor wordt de methode VulGridview opgeroepen
        /// Ook wordt de usercontrol ingeladen voor het dynamisch menu
        /// </summary>
        public OverzichtOpmerkingen()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
            VulGridview();

            //Dynamisch menu (usercontrol inladen)
            ucMenu = new UserControls.Menu(false);
            menuPanel.Children.Add(ucMenu);

            //Pagina grid linken aan twee events die lijsteren naar gestures
            paginaGrid.ManipulationDelta += PaginaGrid_ManipulationDelta;
            paginaGrid.ManipulationStarted += PaginaGrid_ManipulationStarted;
        }

        /// <summary>
        /// Is called when a swype gesture starts.
        ///The difference between ManipulationStarting and this method is that this function needs
        /// the absolute starting point (X & Y).
        /// Wordt opgeroepen wanneer een swype gesture begint.
        /// In tegenstelling tot manipulationstarting wordt aan deze functie het absolute beginpunt (voor X en Y) meegegeven.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PaginaGrid_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            beginPunt = e.Position;
        }

        /// <summary>
        /// Is called during swype.
        /// There is a control if the swype has ended (e.IsEnertial). If is has ended, the end point is calculated and checked if the distance on the
        /// X-axis is long enough to start the menu animation.
        /// Word opgeroepen tijdens het swypen.
        /// er wordt gecontroleerd of de swype beëindigd is of niet (e.IsInertial). Indien wel, dan wordt het eindpunt bepaald
        /// en wordt gekeken of de swype afstand op de X-as ver genoeg was om de menuanimatie te starten.
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
        /// In this method gvwOpmerkingen is filled with Rijberichten.
        /// If there are any, the first one is selected.
        /// In deze methode wordt de gridview opgevuld met opmerkingen
        /// Als er opmerkingen aanwezig zijn dan wordt de eerste opmerking geselecteerd
        /// </summary>
        private async void VulGridview()
        {
            gvwOpmerkingen.HorizontalContentAlignment = Windows.UI.Xaml.HorizontalAlignment.Stretch;
            gvwOpmerkingen.DataContext = opmerkingen;
            gvwOpmerkingen.CanReorderItems = false;
            gvwOpmerkingen.CanDragItems = false;
            if (opmerkingen.Count != 0)
            {
                if (gvwOpmerkingen.Items.Count < geselecteerdeIndex)
                    gvwOpmerkingen.SelectedIndex = geselecteerdeIndex;
            }
            else
            {
                btnAanpassen.IsEnabled = false;
            }
        }

        /// <summary>
        /// This method handles the passing of Rijberichten to other screens.
        /// Via deze methode kunnen we indien er opmerkingen aanwezig zijn, deze aan een andere pagina in de app doorgeven 
        /// </summary>
        /// <returns></returns>
        public static async Task<List<Comment>> GetOverzichtComments()
        {
            if (opmerkingen.Count == 0)
                await HaalCommentsOp();

            return opmerkingen;
        }

        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        /// <summary>
        /// This method handles loading the rijberichten on another screen. 
        /// via deze methode kunnen de comments op een andere pagina worden opgehaald (BijlagenScherm), zodat deze op tijd klaar geladen zijn  
        /// Dit gebeurd enkel als de lijst opmerkingen leeg is en een voertuig geselecteerd is
        /// </summary>
        public static async Task<List<Comment>> HaalCommentsOp()
        {
            if (opmerkingen.Count == 0) //Als de lijst leeg is, probeer ze opnieuw op te halen.
            {
                opmerkingen = await LocalDB.database.GetComments();
                /*foreach (Comment opmerking in opmerkingen)
                {
                    if (opmerking.Datum < DateTime.Now.AddHours(-24) || !opmerking.Chauffeur.Equals(LocalDB.database.GetIngelogdeGebruiker().Username.ToString()))
                    {
                        opmerkingen.Remove(opmerking);
                    }
                }*/
                opmerkingen.Reverse(); //Volgorde omdraaien (laatste eerst)
            }

            /*foreach (Comment opmerking in opmerkingen)
            {
                if (opmerking.Datum < DateTime.Now.AddHours(-24) || !opmerking.Chauffeur.Equals(LocalDB.database.GetIngelogdeGebruiker().Username.ToString()))
                {
                    opmerkingen.Remove(opmerking);
                }
            }*/
            return opmerkingen;
        }

        /// <summary>
        /// This method adds a rijbericht to the list from another page.
        /// Via deze methode kan er een opmerking toegevoegd worden aan de lijst vanaf een andere pagina
        /// </summary>
        /// <param name="comment">comment is van het type Comment en bevat een nieuwe opmerking</param>
        public static void AddComment(Comment comment)
        {
            DefectCodes defectcode = ProblemenTest.GetDefectCode(comment.DefectCode);
            ObjectCodes objectcode = ProblemenTest.GetObjectCode(comment.ObjectCode);
            comment.ObjectCode = objectcode.Code;
            comment.DefectCode = defectcode.Code;
            opmerkingen.Insert(0, comment);
        }

        /// <summary>
        /// This method handles editing a rijbericht.
        /// Via deze methode kan een opmerking aangepast worden vanaf een andere pagina
        /// </summary>
        /// <param name="comment">comment is van het type Comment en bevat een nieuwe opmerking</param>
        public static void UpdateComment(Comment comment)
        {
            opmerkingen.RemoveAt(geselecteerdeIndex);
            opmerkingen.Insert(geselecteerdeIndex, comment);
        }

        /// <summary>
        /// This method deletes a rijbericht.
        /// Via deze methode kan een opmerking verwijderd worden vanaf een andere pagina
        /// </summary>
        public static void DeleteComment()
        {
            opmerkingen.RemoveAt(geselecteerdeIndex);
        }
        
        /// <summary>
        /// This method gets a list of defectcodes.
        /// Via deze methode kan een lijst met defectcodes opgehaald worden vanaf een andere pagina 
        /// </summary>
        /// <param name="defectcodes">defectcodes is van het type DefectCodes en bevat een lijst van defectcodes </param>
        public static void HaalDefectCodeOp(List<DefectCodes> defectcodes)
        {
            defectCodes = defectcodes;
        }
        /// <summary>
        /// This method gets a list of objectcodes
        /// Via deze methode kan een lijst met objectcodes opgehaald worden vanaf een andere pagina 
        /// </summary>
        /// <param name="objectcodes">objectcodes is van het type ObjectCodes en bevat een lijst van objectcodes</param>
        public static void HaalObjectCodeOp(List<ObjectCodes> objectcodes)
        {
            objectCodes = objectcodes;
        }

        #region NavigationHelper registration

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
        /// This method handles the changing of the selectedindex.
        /// Via deze methode wordt de geselecteerdeIndex gewijzigd
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GvwOpmerkingen_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            geselecteerdeIndex = gvwOpmerkingen.SelectedIndex;
        }

        /// <summary>
        /// This method navigates to the ProblemenTest screen when btnAanpassen is clicked.
        /// The rijbericht is passed as a parameter, so users can edit it.
        /// Via deze methode wordt er genavigeerd naar het AanpassenOpmerkingen scherm
        /// Er wordt een opmerking meegegeven
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnAanpassen_Click(object sender, RoutedEventArgs e)
        {
            Comment opmerking = (Comment)gvwOpmerkingen.SelectedItem;
            this.Frame.Navigate(typeof(ProblemenTest), opmerking);
        }
        
        /// <summary>
        /// This method handles navigating to the ProblemenTest screen.
        /// Via deze methode wordt er genavigeerd naar het problemen scherm
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnNieuw_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(ProblemenTest), null);
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

        /// <summary>
        /// This method gets the selected index to pass to other screens.
        /// </summary>
        /// <returns></returns>
        public static int getSelectedIndex()
        {
            return geselecteerdeIndex;
        }
    }
}