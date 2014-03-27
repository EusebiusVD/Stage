using PiXeL_Apps.Classes;
using PiXeL_Apps.Common;
using PiXeL_Apps.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.UI;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace PiXeL_Apps
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class ProblemenTest : Page
    {
        #region Initialisatie

        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        private static List<String> DefectCategorieën = new List<String>(); //Categorieën voor alle defectcodes
        private static List<String> ObjectCategorieën = new List<String>(); // Categorieën voor alle objectcodes
        private static List<DefectCodes> defectCodes = new List<DefectCodes>(); //Alle defectcode objecten
        private List<string> categorieDefectCodes = new List<string>(); //Indien een categorie geselecteerd wordt, is dit de lijst van defectinstanties waarin gezocht word.
        private static List<ObjectCodes> objectCodes = new List<ObjectCodes>(); //Alle objectcode objecten
        private List<string> categorieObjectCodes = new List<string>(); //Indien een categorie geselecteerd wordt, is dit de lijst van objectinstanties waarin gezocht word.
        private static List<string> defectCodeOmschrijvingen; //Lijst van strings met alle omschrijvingen van alle defectcodes
        private static List<string> objectCodeOmschrijvingen; //Lijst van strings met alle omschrijvingen van alle objectcodes
        private List<string> defectCodeOmschrijvingenResultaat = new List<string>(); //Na het zoeken naar defectcodes wordt het resultaat bijgehouden in deze lijst als suggesties
        private List<string> objectCodeOmschrijvingenResultaat = new List<string>(); //Na het zoeken naar objectcodes wordt het resultaat bijgehouden in deze lijst als suggesties
        //private StorageFile[] Photos = new StorageFile[];
        //private StorageFile[] Videos;
        private static int countPhoto = 0;
        private static int countVideo = 0;
        private List<StorageFile> Photos = new List<StorageFile>();
        private List<StorageFile> Videos = new List<StorageFile>();


        private int indexDefectCode = -1;
        public static DefectCodes geselecteerdeDefectCode; //Na het zoeken/selecteren naar een defectcode wordt de geselecteerde hier bijgehouden
        private int indexObjectCode = -1;
        public static ObjectCodes geselecteerdeObjectCode; //Na het zoeken/selecteren naar een objectcode wordt de geselecteerde hier bijgehouden
        private static Comment nieuwCommentaar = new Comment(); //Het uiteindelijke resultaat (wordt weggeschreven naar de database en naar het overzicht
        private static bool updateComment = false; //Lokale variabele om te kijken of uit het overzicht een comment doorgegeven werd om bij te werken (anders -> nieuwe comment)
        private Comment opmerking; //Lokale variabele die de doorgegeven comment van het overzicht zal bijhouden (indien nodig)

        private Point beginPunt; //Menu animatie (swype beginpunt)
        private UserControls.Menu ucMenu; //In te laden menu

        private bool objectSuggesties; //Is op het knopje voor objectsuggesties gedrukt of niet?
        private List<string> filterOpmerking = new List<string>();//Geeft aan of een opmerking al bestaat of niet?
        private static List<string> chauffeurs = new List<string>();
        public static List<Comment> opmerkingen = new List<Comment>();
        private static StorageFolder photoFolder;
        private static StorageFolder videoFolder;
        int photoCounter = 1;
        int videoCounter = 1;

        /// <summary>
        /// In deze constructor wordt de combobox opgevuld
        ///  Ook wordt de usercontrol ingeladen voor het dynamisch menu
        /// </summary>
        public ProblemenTest()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
            nieuwCommentaar.ObjectCodeId = -1;
            nieuwCommentaar.DefectCodeId = -1;
            this.Loaded += ProblemenTest_Loaded;
            //Comboboxen initialiseren en declareren
            cbbDefectCategorie.ItemsSource = DefectCategorieën;
            if (cbbDefectCategorie.Items.Count > 0)
                cbbDefectCategorie.SelectedIndex = 0;
            cbbObjectCategorie.ItemsSource = ObjectCategorieën;
            if (cbbObjectCategorie.Items.Count > 0)
                cbbObjectCategorie.SelectedIndex = 0;

            VoorstelPopup.Opened += VoorstelPopup_Opened;
            VoorstelPopup.Closed += VoorstelPopup_Closed;
            gvwVoorstellen.SelectionChanged += GvwVoorstellen_SelectionChanged;

            //Dynamisch menu (usercontrol) inladen
            ucMenu = new UserControls.Menu(false);
            menuPanel.Children.Add(ucMenu);

            //Pagina grid linken aan twee events die lijsteren naar gestures
            paginaGrid.ManipulationDelta += PaginaGrid_ManipulationDelta;
            paginaGrid.ManipulationStarted += PaginaGrid_ManipulationStarted;

            //Populate cbbPosition
            cbbPosition.PlaceholderText = "Selecteer een positie";
            cbbPosition.Items.Add("Left Front");
            cbbPosition.Items.Add("Front");
            cbbPosition.Items.Add("Right Front");
            cbbPosition.Items.Add("Left");
            cbbPosition.Items.Add("Right");
            cbbPosition.Items.Add("Left Rear");
            cbbPosition.Items.Add("Rear");
            cbbPosition.Items.Add("Right Rear");
            cbbPosition.Items.Add("Outside");
            cbbPosition.Items.Add("Inside");
            cbbPosition.Items.Add("Underside");

            //Populate cbbRating
            cbbRating.PlaceholderText = "Selecteer een rating";
            cbbRating.Items.Add("0  Breakdown");
            cbbRating.Items.Add("1  Not Acceptable - Condition considered a production reject");
            cbbRating.Items.Add("2  Not Acceptable - Condition noted by all customers");
            cbbRating.Items.Add("3  Poor - Condition noted by all customers");
            cbbRating.Items.Add("4  Customer Complaint - Objectionable to the average customer");
            cbbRating.Items.Add("5  Borderline - Improvement desired by the average customer");
            cbbRating.Items.Add("6  Acceptable - Medium condition noted by a critical customer");
            cbbRating.Items.Add("7  Fair - Slight condition noted by a critical customer");
            cbbRating.Items.Add("8  Good - Very slight condition noted by only the trained observer");
            cbbRating.Items.Add("9  Very Good - Trace condition noted by only the trained observer");
            cbbRating.Items.Add("10  Excellent - Condition not perceptible to even a trained observer");
        }

        /// <summary>
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
        /// Via deze methode 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProblemenTest_Loaded(object sender, RoutedEventArgs e)
        {
            if (opmerking != null)
            {
                IEnumerable<ObjectCodes> objCode = from objectcode in objectCodes
                                              where objectcode.Code.Equals(opmerking.ObjectCode)
                                              select objectcode;
                if (objCode.Count() > 0)
                {
                    txtZoekObjectCode.Text = objCode.First().Code;
                    zoekCode(false, objCode.First().Code);
                }

                IEnumerable<DefectCodes> defCode = from defectcode in defectCodes
                                              where defectcode.Code.Equals(opmerking.DefectCode)
                                              select defectcode;
                if (defCode.Count() > 0)
                {
                    txtZoekDefectCode.Text = defCode.First().Code;
                    zoekCode(true, defCode.First().Code);
                }

                txtProblem.Text = opmerking.Omschrijving;
                cbbPosition.SelectedItem = opmerking.Position;
                cbbRating.SelectedItem = opmerking.Rating;
            }
        }


        #endregion

        #region Code-gerelateerd

        /// <summary>
        /// Alle object en defectcodes worden opgehaald door uit de database en gebonden aan hun text/comboboxen
        /// </summary>
        public static async void HaalCodesOp()
        {
            DefectCategorieën = await LocalDB.database.GetDefectCategorieën();
            ObjectCategorieën = await LocalDB.database.GetObjectCategorieën();
            objectCodes = await LocalDB.database.GetObjectCodes();
            defectCodes = await LocalDB.database.GetDefectCodes();
            await HaalCodeOmschrijvingenOp();
        }

        /// <summary>
        /// Zet de lijst met object- en defectcodes om in een lijst met menselijk leesbare omschrijvingen
        /// </summary>
        public static async Task HaalCodeOmschrijvingenOp()
        {
            objectCodeOmschrijvingen = new List<string>();
            foreach (ObjectCodes objectCode in objectCodes)
            {
                objectCodeOmschrijvingen.Add(String.Format("{0} - {1}", objectCode.Code, objectCode.Beschrijving));
            }

            defectCodeOmschrijvingen = new List<string>();
            foreach (DefectCodes defectCode in defectCodes)
            {
                defectCodeOmschrijvingen.Add(String.Format("{0} - {1}", defectCode.Code, defectCode.Beschrijving));
            }
        }

        /// <summary>
        /// Methode om te voorspellen of het meest recent ingetypte zoekparameter overeenstemt met een van de items in de lijst.
        /// </summary>
        /// <param name="defect">Defect of Objectcode boolean</param>
        /// <param name="zoekParameter">Gezochte string</param>
        /// <returns></returns>
        private int zoekCode(bool defect, string zoekParameter)
        {
            int index = -1;
            int parameterLengte = zoekParameter.Length; //Lengte ingegeven zoek parameter
            
            if (parameterLengte == 0)
            {
                VoorstelPopup.IsOpen = false;
                return -2;
            }

            try
            {
                int zoekCode;
                if (int.TryParse(zoekParameter, out zoekCode))
                {
                    switch (defect)
                    {
                        case true: //Gezocht naar een defectcode
                            objectSuggesties = false;

                            IEnumerable<string> relevanteDefectCodes =
                                from omschrijving in categorieDefectCodes
                                where (omschrijving.Length >= parameterLengte &&
                                omschrijving.Substring(0, parameterLengte).ToLower().Equals(zoekParameter.ToLower()))
                                select omschrijving;

                            int aantalDefectCodes = relevanteDefectCodes.Count();
                            if (aantalDefectCodes > 0)
                            {
                                gvwVoorstellen.ItemsSource = relevanteDefectCodes;
                                index = defectCodeOmschrijvingen.IndexOf(relevanteDefectCodes.First());
                                VoorstelPopup.IsOpen = aantalDefectCodes < 2 ? false : true;
                            }
                            else
                            {
                                gvwVoorstellen.ItemsSource = categorieDefectCodes;
                                VoorstelPopup.IsOpen = false;
                            }
                            break;
                        case false: //Gezocht naar een objectcode
                            objectSuggesties = true;
                            IEnumerable<string> relevanteObjectCodes =
                                from omschrijving in categorieObjectCodes
                                where (omschrijving.Length >= parameterLengte && omschrijving.Substring(0, parameterLengte).ToLower().Equals(zoekParameter.ToLower()))
                                select omschrijving;

                            int aantalObjectCodes = relevanteObjectCodes.Count();
                            if (aantalObjectCodes > 0)
                            {
                                gvwVoorstellen.ItemsSource = relevanteObjectCodes;
                                index = objectCodeOmschrijvingen.IndexOf(relevanteObjectCodes.First());
                                VoorstelPopup.IsOpen = aantalObjectCodes < 2 ? false : true;
                            }
                            else
                            {
                                gvwVoorstellen.ItemsSource = categorieObjectCodes;
                                VoorstelPopup.IsOpen = false;
                            }
                            break;
                    }
                }
                else
                {
                    switch (defect)
                    {
                        case true:
                            objectSuggesties = false;
                            IEnumerable<string> relevanteDefectCodes =
                                from omschrijving in categorieDefectCodes
                                where omschrijving.Length >= parameterLengte &&
                                      omschrijving.ToLower().Contains(zoekParameter.ToLower())
                                select omschrijving;

                            int aantalDefectCodes = relevanteDefectCodes.Count();
                            if (aantalDefectCodes > 0)
                            {
                                gvwVoorstellen.ItemsSource = relevanteDefectCodes;
                                index = defectCodeOmschrijvingen.IndexOf(relevanteDefectCodes.First());
                                VoorstelPopup.IsOpen = aantalDefectCodes < 2 ? false : true;
                            }
                            else
                            {
                                gvwVoorstellen.ItemsSource = categorieDefectCodes;
                                VoorstelPopup.IsOpen = false;
                            }
                            break;
                        case false:
                            objectSuggesties = true;
                            IEnumerable<string> relevanteObjectCodes =
                                from omschrijving in categorieObjectCodes
                                where omschrijving.Length >= parameterLengte &&
                                      omschrijving.ToLower().Contains(zoekParameter.ToLower())
                                select omschrijving;

                            int aantalObjectCodes = relevanteObjectCodes.Count();
                            if (aantalObjectCodes > 0)
                            {
                                gvwVoorstellen.ItemsSource = relevanteObjectCodes;
                                index = objectCodeOmschrijvingen.IndexOf(relevanteObjectCodes.First());
                                VoorstelPopup.IsOpen = aantalObjectCodes < 2 ? false : true;
                            }
                            else
                            {
                                gvwVoorstellen.ItemsSource = categorieObjectCodes;
                                VoorstelPopup.IsOpen = false;
                            }
                            break;
                    }
                }
            }
            catch (Exception)
            {
                paLogging.log.Error(String.Format("Er ging iets fout bij het opzoeken van een object of defectcode. Inhoud: {0}", zoekParameter));
            }
            return index;
        }

        #endregion

        #region AutoCompleteBoxen Events

        /// Met dit Click event wordt als de tekst in het zoekveld voor objecten veranderd, wordt gekeken of de reeds ingegeve tekst overeenkomt
        /// met een reeds bestaande objectcode. Deze wordt getoond.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TxtZoekObjectCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            VerwerkGezochtResultaat(false, zoekCode(false, txtZoekObjectCode.Text.Trim()));
        }

        /// <summary>
        /// Met dit Click event wordt als de tekst in het zoekveld voor defecten veranderd, wordt gekeken of de reeds ingegeve tekst overeenkomt
        /// met een reeds bestaande defectcode. Deze wordt getoond.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TxtZoekDefectCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            VerwerkGezochtResultaat(true, zoekCode(true, txtZoekDefectCode.Text.Trim()));
        }

        /// <summary>
        /// Automatiseer het zoeken van de object of defectcode indien een andere categorie geselecteerd werd.
        /// </summary>
        /// <param name="defect"></param>
        /// <param name="index"></param>
        private void VerwerkGezochtResultaat(bool defect, int index)
        {
            if (defect)
            {
                if (index == -1)
                {
                    lblDefectCodeStatus.Text = "Geen objectcode gevonden.";
                    lblDefectCodeStatus.Foreground = (SolidColorBrush)Application.Current.Resources["DefaultTextErrorColor"];
                }
                else if (index == -2)
                {
                    lblDefectCodeStatus.Foreground = (SolidColorBrush)Application.Current.Resources["DefaultTextColor"];
                    lblDefectCodeStatus.Text = String.Empty;
                }
                else
                {
                    lblDefectCodeStatus.Foreground = (SolidColorBrush)Application.Current.Resources["DefaultTextColor"];
                    lblDefectCodeStatus.Text = String.Format("Geselecteerd: {0}", defectCodeOmschrijvingen[index]);
                    nieuwCommentaar.DefectCodeId = index + 1;
                    nieuwCommentaar.DefectCode = defectCodes.ElementAt(index).Code;
                }
            }
            else
            {
                if (index == -1)
                {
                    lblObjectCodeStatus.Text = "Geen objectcode gevonden.";
                    lblObjectCodeStatus.Foreground = (SolidColorBrush)Application.Current.Resources["DefaultTextErrorColor"];
                }
                else if (index == -2)
                {
                    lblObjectCodeStatus.Foreground = (SolidColorBrush)Application.Current.Resources["DefaultTextColor"];
                    lblObjectCodeStatus.Text = String.Empty;
                }
                else
                {
                    lblObjectCodeStatus.Foreground = (SolidColorBrush)Application.Current.Resources["DefaultTextColor"];
                    lblObjectCodeStatus.Text = String.Format("Geselecteerd: {0}", objectCodeOmschrijvingen[index]);
                    nieuwCommentaar.ObjectCodeId = index + 1;
                    nieuwCommentaar.ObjectCode = objectCodes.ElementAt(index).Code;
                }
            }
        }

        /// <summary>
        /// Als een nieuwe defecten categorie geselecteerd wordt, word de lijst met mogelijke defectcodes opnieuw aangepast.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CbbDefectCategorie_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbbDefectCategorie.SelectedIndex != 0)
            {
                String categorie = (String)cbbDefectCategorie.SelectedItem;
                IEnumerable<string> defCode = from defectcode in defectCodes
                                              where defectcode.Type_Defect.ToLower().Equals(categorie)
                                              select String.Format("{0} - {1}", defectcode.Code, defectcode.Beschrijving);
                if (defCode.Count() > 0)
                {
                    gvwVoorstellen.ItemsSource = defCode;
                    categorieDefectCodes = defCode.ToList<string>();
                    VerwerkGezochtResultaat(true, zoekCode(true, txtZoekDefectCode.Text.Trim()));
                }
            }
            else
            {
                categorieDefectCodes = defectCodeOmschrijvingen;
            }
        }

        /// <summary>
        /// Als een nieuwe objecten categorie geselecteerd wordt, word de lijst met mogelijke objectcodes opnieuw aangepast.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CbbObjectCategorie_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbbObjectCategorie.SelectedIndex != 0)
            {
                String categorie = (String)cbbObjectCategorie.SelectedItem;
                IEnumerable<string> objCode = from objectCode in objectCodes
                                              where objectCode.Type.ToLower().Equals(categorie)
                                              select String.Format("{0} - {1}", objectCode.Code, objectCode.Beschrijving);
                if (objCode.Count() > 0)
                {
                    gvwVoorstellen.ItemsSource = objCode;
                    categorieObjectCodes = objCode.ToList<string>();
                    VerwerkGezochtResultaat(false, zoekCode(false, txtZoekObjectCode.Text.Trim()));
                }
            }
            else
            {
                categorieObjectCodes = objectCodeOmschrijvingen;
            }
        }

        /// <summary>
        /// Via deze methode wordt de voorstelPopup geopend
        /// Deze popup toont alle opmerkingen die overeenkomen met de ingegeven zoekparameter
        /// </summary>
        /// <param name="sender"></param>
        void VoorstelPopup_Opened(object sender, object e)
        {
            if (gvwVoorstellen.ItemsSource != null || gvwVoorstellen.Items.Count > 1)
            {
                lblObjectCodeStatus.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                lblDefectCodeStatus.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Als een item in uit de voorstellen geselecteerd wordt, zal dit item geselecteerde worden.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GvwVoorstellen_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (gvwVoorstellen.SelectedItems.Count > 0)
            {
                string geselecteerdItem = gvwVoorstellen.SelectedItems.First().ToString();
                if (!objectSuggesties)
                {
                    indexDefectCode = defectCodeOmschrijvingen.IndexOf(geselecteerdItem);
                    txtZoekDefectCode.Text = geselecteerdItem;

                    lblDefectCodeStatus.Foreground = (SolidColorBrush)Application.Current.Resources["DefaultTextColor"];
                    lblDefectCodeStatus.Text = String.Format("Geselecteerd: {0}", geselecteerdItem);
                    lblDefectCodeStatus.UpdateLayout();
                    nieuwCommentaar.DefectCodeId = indexDefectCode + 1;
                    btnZoekDefect.Content = "\xE1A3";
                }
                else
                {
                    indexObjectCode = objectCodeOmschrijvingen.IndexOf(geselecteerdItem);
                    txtZoekObjectCode.Text = geselecteerdItem;

                    lblObjectCodeStatus.Foreground = (SolidColorBrush)Application.Current.Resources["DefaultTextColor"];
                    lblObjectCodeStatus.Text = String.Format("Geselecteerd: {0}", geselecteerdItem);
                    lblObjectCodeStatus.UpdateLayout();
                    nieuwCommentaar.ObjectCodeId = indexObjectCode + 1;
                    btnZoekObject.Content = "\xE1A3";
                }
            }
            VoorstelPopup.IsOpen = false;
        }

        /// <summary>
        /// via deze methode wordt de voorstelPopup gesloten
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void VoorstelPopup_Closed(object sender, object e)
        {
            lblObjectCodeStatus.Visibility = Windows.UI.Xaml.Visibility.Visible;
            lblDefectCodeStatus.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }

        #endregion

        #region Button clicks

        /// <summary>
        /// Dit Click event zorgt ervoor dat wanneer op de Opslaan knop gedrukt wordt, er gecontroleerd word of
        /// een object- en defectode aangevuld werden. Indien alles weggeschreven werd krijgt de gebruiker een gepaste boodschap.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BtnOpslaan_Click(object sender, RoutedEventArgs e)
        {
            if (nieuwCommentaar.DefectCodeId < 0 || nieuwCommentaar.ObjectCodeId < 0 || cbbPosition.SelectedItem == null || cbbRating.SelectedItem == null)
                lblError.Text = "U moet een object- en defectcode, een positie en een rating ingeven";
            else
            {
                //  filterOpmerking = (List<String>)Common.LocalStorage.localStorage.LaadGegevens("rapporteerDubbeleOpmerking");
                if (updateComment) //Update a comment
                {
                    User gebruiker = LocalDB.database.GetIngelogdeGebruiker();
                    //ophalen chauffeur van opm met ID ...

                    nieuwCommentaar.Omschrijving = txtProblem.Text;
                    nieuwCommentaar.Id = opmerking.Id;
                    nieuwCommentaar.Vehicle_Id = opmerking.Vehicle_Id;
                    nieuwCommentaar.Chauffeur = gebruiker.Username;
                    nieuwCommentaar.Datum = DateTime.Now;
                    nieuwCommentaar.Position = opmerking.Position;
                    nieuwCommentaar.Rating = opmerking.Rating;
                    CompleteAuto ca = await LocalDB.database.GetToegewezenAuto();
                    
                    for (int i = 0; i < Photos.Count(); i++)
                    {
                        if (i == 0)
                        {
                            await Photos[i].RenameAsync(ca.Number + "_" + opmerking.Id + "_" + Photos[i].DateCreated.ToString("ddMMyyyy-HHmmssff") + "_" + gebruiker.Username + ".png");
                        }
                        else
                        {
                            await Photos[i].RenameAsync(ca.Number + "_" + opmerking.Id + "_" + Photos[i].DateCreated.ToString("ddMMyyyy-HHmmssff") + "_" + gebruiker.Username + "(" + i + ").png");
                        }
                        await movePhotoVideo(Photos[i], photoFolder);
                    }

                    for (int i = 0; i < Videos.Count(); i++)
                    {
                        if (i == 0)
                        {
                            await Videos[i].RenameAsync(ca.Number + "_" + opmerking.Id + "_" + Videos[i].DateCreated.ToString("ddMMyyyy-HHmmssff") + "_" + gebruiker.Username + ".mp4");
                        }
                        else
                        {
                            await Videos[i].RenameAsync(ca.Number + "_" + opmerking.Id + "_" + Videos[i].DateCreated.ToString("ddMMyyyy-HHmmssff") + "_" + gebruiker.Username + "(" + i + ").mp4");
                        }
                        await movePhotoVideo(Videos[i], videoFolder);
                    }
                    await CommentaarAanpassen(nieuwCommentaar);
                    this.Frame.Navigate(typeof(OverzichtOpmerkingen));
                }
                else //Create a new rijbericht
                {
                    User gebruiker = LocalDB.database.GetIngelogdeGebruiker();
                    nieuwCommentaar.Omschrijving = txtProblem.Text;
                    nieuwCommentaar.Chauffeur = gebruiker.Username;
                    nieuwCommentaar.Datum = DateTime.Now;
                    nieuwCommentaar.Position = cbbPosition.SelectedItem.ToString();
                    nieuwCommentaar.Rating = cbbRating.SelectedItem.ToString();

                    CompleteAuto ca = await LocalDB.database.GetToegewezenAuto();

                    object settingRapporteerDubbele = LocalStorage.localStorage.LaadGegevens("rapporteerDubbeleOpmerking");
                    bool rapporteerDubbele;
                    if (settingRapporteerDubbele != null &&
                        bool.TryParse(settingRapporteerDubbele.ToString(), out rapporteerDubbele) &&
                        rapporteerDubbele)
                    {
                        await ControleerOpmerking();
                    }
                    else
                    {
                        await CommentaarToevoegen(nieuwCommentaar);

                        var commentList = await LocalDB.database.GetComments();
                        Comment LastComment = commentList[0];
                        for (int i = 0; i < Photos.Count(); i++)
                        {
                            if (i == 0)
                            {
                                await Photos[i].RenameAsync(ca.Number + "_" + LastComment.Id + "_" + Photos[i].DateCreated.ToString("ddMMyyyy-HHmmssff") + "_" + gebruiker.Username + ".png");
                            }
                            else
                            {
                                await Photos[i].RenameAsync(ca.Number + "_" + LastComment.Id + "_" + Photos[i].DateCreated.ToString("ddMMyyyy-HHmmssff") + "_" + gebruiker.Username + "(" + i + ").png");
                            }                        
                            await movePhotoVideo(Photos[i], photoFolder);
                        }

                        for (int i = 0; i < Videos.Count(); i++)
                        {
                            if (i == 0)
                            {
                                await Videos[i].RenameAsync(ca.Number + "_" + LastComment.Id + "_" + Videos[i].DateCreated.ToString("ddMMyyyy-HHmmssff") + "_" + gebruiker.Username + ".mp4");
                            }
                            else
                            {
                                await Videos[i].RenameAsync(ca.Number + "_" + LastComment.Id + "_" + Videos[i].DateCreated.ToString("ddMMyyyy-HHmmssff") + "_" + gebruiker.Username + "(" + i + ").mp4");
                            }
                            await movePhotoVideo(Videos[i], videoFolder);
                        }
                        this.Frame.Navigate(typeof(OverzichtOpmerkingen));
                    }
                }
                updateComment = false;

                this.Frame.Navigate(typeof(OverzichtOpmerkingen));
            }
        }

        /// <summary>
        /// Het aanmaken van een nieuwe opmerking
        /// </summary>
        /// <param name="commentaar">De opmerking die aangemaakt moet worden van het type comment</param>
        private async Task<bool> CommentaarToevoegen(Comment commentaar)
        {
            Comment toegevoegdeCommentaar = await LocalDB.database.AddComment(commentaar);
            if (toegevoegdeCommentaar != null)
            {
                OverzichtOpmerkingen.AddComment(toegevoegdeCommentaar);
                return true;
            }
            else
            {
                lblError.Foreground = (SolidColorBrush)Application.Current.Resources["DefaultTextErrorColor"];
                lblError.Text = "Er heeft zich een fout voorgedaan. Probeer later opnieuw of neem contact op met een beheerder.";
            }
            return false;
        }

        /// <summary>
        /// Het updaten van een opmerking
        /// </summary>
        /// <param name="commentaar">De opmerking die geupdate moet worden van het type comment</param>
        private async Task CommentaarAanpassen(Comment commentaar)
        {
            if (await LocalDB.database.UpdateComment(commentaar))
            {
                //  String boodschap = "Uw probleem werd aangepast";
                OverzichtOpmerkingen.UpdateComment(commentaar);
            }
            else
            {
                lblError.Foreground = (SolidColorBrush)Application.Current.Resources["DefaultTextErrorColor"];
                lblError.Text = "Er heeft zich een fout voorgedaan. Probeer later opnieuw of neem contact op met een beheerder.";
            }
        }

        /// <summary>
        /// In deze methode wordt het uitzicht van de zoekObjectButton gezet naar gelang of de VoorstelPopup openstaat of niet
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnZoekObject_Click(object sender, RoutedEventArgs e)
        {
            objectSuggesties = true;
            if (VoorstelPopup.IsOpen)
            {
                btnZoekObject.Content = "\xE1A3";
                btnZoekDefect.Content = "\xE1A3";
                VoorstelPopup.IsOpen = false;
            }
            else
            {
                gvwVoorstellen.ItemsSource = categorieObjectCodes;
                btnZoekObject.Content = "\xE19F";
                VoorstelPopup.IsOpen = true;
            }

        }

        /// <summary>
        /// In deze methode wordt het uitzicht van de zoekDefectButton gezet naar gelang of de VoorstelPopup openstaat of niet
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnZoekDefect_Click(object sender, RoutedEventArgs e)
        {
            objectSuggesties = false;
            if (VoorstelPopup.IsOpen)
            {
                btnZoekDefect.Content = "\xE1A3";
                btnZoekObject.Content = "\xE1A3";
                VoorstelPopup.IsOpen = false;
            }
            else
            {
                gvwVoorstellen.ItemsSource = categorieDefectCodes;
                btnZoekDefect.Content = "\xE19F";
                VoorstelPopup.IsOpen = true;
            }
        }

        /// <summary>
        /// Deze methode kan een opmerking verwijderd worden
        /// Dit gebeurd enkel als Ja gekozen wordt in de messagedialog
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BtnVerwijderen_Click(object sender, RoutedEventArgs e)
        {

            bool? result = null;
            MessageDialog okAnnuleer = new MessageDialog("Weet u zeker dat u dit rijbericht wil verwijderen?", "Rijbericht Verwijderen");
            okAnnuleer.Commands.Add(new UICommand("Ja", new UICommandInvokedHandler((cmd) => result = true)));
            okAnnuleer.Commands.Add(new UICommand("Nee"));
            await okAnnuleer.ShowAsync();
            if (result == true)
            {
                if (await LocalDB.database.DeleteComment(opmerking))
                {
                    OverzichtOpmerkingen.DeleteComment();
                    this.Frame.Navigate(typeof(OverzichtOpmerkingen));
                }
                else
                {
                    lblError.Foreground = (SolidColorBrush)Application.Current.Resources["DefaultTextErrorColor"];
                    lblError.Text = "Er heeft zich een fout voorgedaan. Probeer later opnieuw of neem contact op met een beheerder.";
                }
            }
        }

        /// <summary>
        /// Controleert of de opmerking niet al werd gemaakt. En indien deze al gemaakt werd of dit door een andere chauffeur wordt gedaan of door dezelfde chauffeur
        /// </summary>
        /// <param name="nieuwCommentaar">Opmerking die gecontroleerd moet worden van het type Comment</param>
        /// <returns>Task</returns>
        private async Task ControleerOpmerking()
        {
            List<Comment> opmerkingen = await OverzichtOpmerkingen.GetOverzichtComments();

            IEnumerable<Comment> dubbeleOpmerkingen =
                from evtOpmerking in opmerkingen
                where (evtOpmerking.ObjectCode.Equals(nieuwCommentaar.ObjectCode) &&
                       evtOpmerking.DefectCode.Equals(nieuwCommentaar.DefectCode) &&
                       evtOpmerking.Position.Equals(nieuwCommentaar.Position) &&
                       evtOpmerking.Duplicate == 0)
                select evtOpmerking;

            List<Comment> dubbeleOpmerkingenlijst = dubbeleOpmerkingen.ToList<Comment>();
            bool doorgaanMetBewerken = false;

            if (dubbeleOpmerkingenlijst.Count == 0)
            {
                doorgaanMetBewerken = !await CommentaarToevoegen(nieuwCommentaar);
            }
            else
            {
                foreach (Comment dubbele in dubbeleOpmerkingenlijst)
                {
                    //Comment dubbele = dubbeleOpmerkingenlijst.Last<Comment>();
                    string bericht;
                    if (dubbele.Chauffeur == nieuwCommentaar.Chauffeur)
                        bericht = String.Format("U heeft eerder al een rijbericht aangemaakt voor dit onderdeel. Wilt u deze rijberichten samenvoegen?\nObjectcode: {0}, Defectcode: {1}\nOmschrijving: {2}.",
                                                                            dubbele.ObjectCode, dubbele.DefectCode, dubbele.Omschrijving);
                    else
                        bericht = String.Format("Chauffeur {0} heeft eerder een rijbericht gemaakt voor dit onderdeel. Wilt u deze rijberichten samenvoegen?\nObjectcode: {1}, Defectcode: {2}\nOmschrijving: {3}.",
                                                                            dubbele.Chauffeur, dubbele.ObjectCode, dubbele.DefectCode, dubbele.Omschrijving);

                    //Melding weergeven indien er dubbele commentaren gevonden zijn
                    MessageDialog okAnnuleer = new MessageDialog(bericht, "Dubbele rijberichten");
                    okAnnuleer.Commands.Add(new UICommand("Samenvoegen"));
                    okAnnuleer.Commands.Add(new UICommand("Opslaan als nieuw rijbericht"));
                    //okAnnuleer.Commands.Add(new UICommand("Annuleren"));
                    var resultaat = await okAnnuleer.ShowAsync();
                    if (resultaat.Label.Equals("Samenvoegen"))
                    {
                        nieuwCommentaar.Duplicate += 1;
                        nieuwCommentaar.OriginalId = dubbele.Id;
                        await CommentaarToevoegen(nieuwCommentaar);
                        //dubbele.Omschrijving += String.Format("{0}Toevoeging chauffeur {1}: {2}.", Environment.NewLine, nieuwCommentaar.Chauffeur, nieuwCommentaar.Omschrijving);
                        //await CommentaarAanpassen(dubbele);
                    }
                    else if (resultaat.Label.Equals("Opslaan als nieuw rijbericht"))
                    {
                        nieuwCommentaar.Duplicate = 0;
                        nieuwCommentaar.OriginalId = 0;
                        await CommentaarToevoegen(nieuwCommentaar);
                    }
                }
            }
            if (!doorgaanMetBewerken)
                this.Frame.Navigate(typeof(OverzichtOpmerkingen));
        }

        /// <summary>
        /// This method handles photo's in the "Rijberichten" screen. Photo's will be PNG format and the highest available resolution.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BtnMaakFoto_Click(object sender, RoutedEventArgs e)
        {
            CameraCaptureUI fotoScherm = new CameraCaptureUI();
            fotoScherm.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Png; //.png is veel lichter
            fotoScherm.PhotoSettings.MaxResolution = CameraCaptureUIMaxPhotoResolution.HighestAvailable;
            fotoScherm.PhotoSettings.AllowCropping = false;

            var foto = await fotoScherm.CaptureFileAsync(CameraCaptureUIMode.Photo); //Enkel een foto maken
            if (foto != null)
            {
                Photos.Add(foto);
                countPhoto++;
                /*int messageID;
                if (OverzichtOpmerkingen.getSelectedIndex() == 0)
                {
                    messageID = OverzichtOpmerkingen.getSelectedIndex();
                }
                else
                {
                    messageID = await LocalDB.database.getAantalCommentaren();
                }
                CompleteAuto ca = await LocalDB.database.GetToegewezenAuto();
                User gebruiker = LocalDB.database.GetIngelogdeGebruiker();
                // if-structure doesn't work entirely
                if (photoFolder.GetFileAsync(ca.Number + "_" + messageID + "_" + foto.DateCreated.ToString("ddMMyyyy-HH:mm:ss.ff") + "_" + gebruiker.Username + ".png").Equals(true))
                {
                    await foto.RenameAsync(ca.Number + "_" + messageID + "_" + foto.DateCreated.ToString("ddMMyyyy-HH:mm:ss.ff") + "_" + gebruiker.Username + "(" + photoCounter + ").png");
                    photoCounter++;
                    var fileStream = await foto.OpenAsync(Windows.Storage.FileAccessMode.Read);
                    AsyncStatus renamed = foto.RenameAsync(ca.Number + "_" + messageID + "_" + foto.DateCreated.ToString("ddMMyyyy-HH:mm:ss.ff") + "_" + gebruiker.Username + "(" + photoCounter + ").png").Status;
                    
                        try
                        {
                            renamed = foto.RenameAsync(ca.Number + "_" + messageID + "_" + foto.DateCreated.ToString("ddMMyyyy-HH:mm:ss.ff") + "_" + gebruiker.Username + "(" + photoCounter + ").png").Status;
                        }
                        catch(Exception ex)
                        {
                            lblError.Foreground = (SolidColorBrush)Application.Current.Resources["DefaultTextErrorColor"];
                            lblError.Text = "Er is een proleem opgetreden bij het opslaan van de foto, probeer het opnieuw";
                            btnMaakFoto.IsEnabled = true;
                            goto BREAK;
                        }                        

                    BREAK:
                    bool completed = await movePhotoVideo(foto, photoFolder);
                    if (completed)
                    {
                        lblError.Foreground = new SolidColorBrush(Colors.White);
                        lblError.Text = "De foto is succesvol opgeslagen!";
                        btnMaakFoto.IsEnabled = true;
                    }
                    else
                    {
                        lblError.Foreground = (SolidColorBrush)Application.Current.Resources["DefaultTextErrorColor"];
                        lblError.Text = "Er is een proleem opgetreden bij het opslaan van de foto, probeer het opnieuw";
                        btnMaakFoto.IsEnabled = true;
                    }
                }
                else
                {
                    await foto.RenameAsync(ca.Number + "_" + messageID + "_" + foto.DateCreated.ToString("ddMMyyyy-HHmmssff") + "_" + gebruiker.Username + ".png");
                    photoCounter = 0;
                    var fileStream = await foto.OpenAsync(Windows.Storage.FileAccessMode.Read);
                    AsyncStatus renamed = foto.RenameAsync(ca.Number + "_" + messageID + "_" + foto.DateCreated.ToString("ddMMyyyy-HHmmssff") + "_" + gebruiker.Username + ".png").Status;
                    while (renamed.Equals("Started"))
                    {
                        try
                        {
                            renamed = foto.RenameAsync(ca.Number + "_" + messageID + "_" + foto.DateCreated.ToString("ddMMyyyy-HHmmssff") + "_" + gebruiker.Username + ".png").Status;
                        }
                        catch(Exception ex)
                        {
                            lblError.Foreground = (SolidColorBrush)Application.Current.Resources["DefaultTextErrorColor"];
                            lblError.Text = "Er is een proleem opgetreden bij het opslaan van de foto, probeer het opnieuw";
                            btnMaakFoto.IsEnabled = true;
                            goto BREAK;
                        }
                    }
                    BREAK:
                    try
                    {
                        bool completed = await movePhotoVideo(foto, photoFolder);
                            if (completed)
                            {   
                                lblError.Foreground = new SolidColorBrush(Colors.White);
                                lblError.Text = "De foto is succesvol opgeslagen!";
                                btnMaakFoto.IsEnabled = true;
                            }
                            else
                            {
                                lblError.Foreground = (SolidColorBrush)Application.Current.Resources["DefaultTextErrorColor"];
                                lblError.Text = "Er is een proleem opgetreden bij het opslaan van de foto, probeer het opnieuw";
                                btnMaakFoto.IsEnabled = true;
                            }
                    }
                    catch
                    {
                        lblError.Foreground = (SolidColorBrush)Application.Current.Resources["DefaultTextErrorColor"];
                        lblError.Text = "Er is een proleem opgetreden bij het opslaan van de foto, probeer het opnieuw";
                        btnMaakFoto.IsEnabled = true;
                    }                                              
                }*/
            }
        }

        /// <summary>
        /// This method handles videos in the "Rijberichten" screen. Videos will be MP4 format and the highest available resolution. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BtnMaakVideo_Click(object sender, RoutedEventArgs e)
        {
            var videoScherm = new CameraCaptureUI();
            videoScherm.VideoSettings.Format = CameraCaptureUIVideoFormat.Mp4;
            videoScherm.VideoSettings.MaxResolution = CameraCaptureUIMaxVideoResolution.HighestAvailable;

            var video = await videoScherm.CaptureFileAsync(CameraCaptureUIMode.Video); //Video opnemen
            if (video != null)
            {
                Videos.Add(video);
                countVideo++;
                /* int messageID;
                 if (OverzichtOpmerkingen.getSelectedIndex() == 0)
                 {
                     messageID = OverzichtOpmerkingen.getSelectedIndex();
                 }
                 else
                 {
                     messageID = await LocalDB.database.getAantalCommentaren();
                 }
                 User gebruiker = LocalDB.database.GetIngelogdeGebruiker();
                 CompleteAuto ca = await LocalDB.database.GetToegewezenAuto();
                 // if-structuur werkt niet, als er te snel foto's genomen worden doet hij het nog altijd
                 if (videoFolder.GetFileAsync(ca.Number + "_" + messageID + "_" + video.DateCreated.ToString("ddMMyyyy-HHmmss") + "_" + gebruiker.Username + ".mp4").Equals(true))
                 {
                     await video.RenameAsync(ca.Number + "_" + messageID + "_" + video.DateCreated.ToString("ddMMyyyy-HHmmss") + "_" + gebruiker.Username + "(" + videoCounter + ").mp4");
                     videoCounter++;
                     var fileStream = await video.OpenAsync(Windows.Storage.FileAccessMode.Read);
                     AsyncStatus renamed = video.RenameAsync(ca.Number + "_" + messageID + "_" + video.DateCreated.ToString("ddMMyyyy-HHmmss") + "_" + gebruiker.Username + "(" + videoCounter + ").mp4").Status;
                     while (renamed.Equals("Started"))
                     {
                         renamed = video.RenameAsync(ca.Number + "_" + messageID + "_" + video.DateCreated.ToString("ddMMyyyy-HHmmss") + "_" + gebruiker.Username + "(" + videoCounter + ").mp4").Status;
                     }
                     btnMaakVideo.IsEnabled = true;
                     bool completed = await movePhotoVideo(video, videoFolder);
                     if (completed)
                     {
                         lblError.Foreground = new SolidColorBrush(Colors.White);
                         lblError.Text = "De video is succesvol opgeslagen!";
                     }
                     else
                     {
                         lblError.Foreground = (SolidColorBrush)Application.Current.Resources["DefaultTextErrorColor"];
                         lblError.Text = "Er is een proleem opgetreden bij het opslaan van de video, probeer het opnieuw";
                     }
                 }
                 else
                 {
                     await video.RenameAsync(ca.Number + "_" + messageID + "_" + video.DateCreated.ToString("ddMMyyyy-HHmmss") + "_" + gebruiker.Username + ".mp4");
                     videoCounter = 0;
                     var fileStream = await video.OpenAsync(Windows.Storage.FileAccessMode.Read);
                     AsyncStatus renamed = video.RenameAsync(ca.Number + "_" + messageID + "_" + video.DateCreated.ToString("ddMMyyyy-HHmmss") + "_" + gebruiker.Username + ".png").Status;
                     while (renamed.Equals("Started"))
                     {
                         renamed = video.RenameAsync(ca.Number + "_" + messageID + "_" + video.DateCreated.ToString("ddMMyyyy-HHmmss") + "_" + gebruiker.Username + ".png").Status;
                     }
                     btnMaakVideo.IsEnabled = true;
                     bool completed = await movePhotoVideo(video, videoFolder);
                     if (completed)
                     {
                         lblError.Foreground = new SolidColorBrush(Colors.White);
                         lblError.Text = "De video is succesvol opgeslagen!";
                     }
                     else
                     {
                         lblError.Foreground = (SolidColorBrush)Application.Current.Resources["DefaultTextErrorColor"];
                         lblError.Text = "Er is een proleem opgetreden bij het opslaan van de video, probeer het opnieuw";
                     }
                 }*/
            }
        }

        #endregion

        #region Toegangsmethodes

        /// <summary>
        /// Via deze methode kan de code van een objectcode opgehaald worden vanaf een andere pagina in de applicatie
        /// </summary>
        /// <param name="objectId">objectId is van het type int en bevat het Id van een objectcode</param>
        /// <returns></returns>
        /// <summary>
        /// Via deze methode kan de code van een objectcode opgehaald worden vanaf een andere pagina in de applicatie
        /// </summary>
        /// <param name="objectId">objectId is van het type int en bevat het Id van een objectcode</param>
        /// <returns></returns>
        public static ObjectCodes GetObjectCode(string objectCode)
        {
            IEnumerable<ObjectCodes> objCode = from objectcode in objectCodes
                                               where objectcode.Code == objectCode
                                               select objectcode;
            if (objCode.Count() > 0)
                return objCode.First();
            return null;
        }

        /// <summary>
        /// Via deze methode kan de code van een defectcode opgehaald worden vanaf een andere pagina in de applicatie
        /// </summary>
        /// <param name="defectId">defectId is van het type int en bevat het Id van een defectId </param>
        /// <returns></returns>
        public static DefectCodes GetDefectCode(string defectCode)
        {
            IEnumerable<DefectCodes> defCode = from defectcode in defectCodes
                                               where defectcode.Code == defectCode
                                               select defectcode;
            if (defCode.Count() > 0)
                return defCode.First();
            return null;
        }
        /// <summary>
        /// De lijst van objectCodes wordt opgevuld met de lijst van ObjectoCodes die aan deze methode werd meegegeven
        /// </summary>
        /// <param name="inTeVoeren"></param>
        public static void SetObjectCodes(List<ObjectCodes> inTeVoeren)
        {
            objectCodes = inTeVoeren;
        }
        /// <summary>
        /// De lijst van defectcodes wordt opgevuld met de lijst van DefectCodes die aan deze methode werd meegegeven
        /// Ook wordt de methode HaalCodeOmschrijvingenOp() aangeroepen
        /// </summary>
        /// <param name="inTeVoeren"></param>
        public static async void SetDefectCodes(List<DefectCodes> inTeVoeren)
        {
            defectCodes = inTeVoeren;
            await HaalCodeOmschrijvingenOp();
        }

        /// <summary>
        /// Via deze methode kan er gecontroleerd worden of een opmerking al eens werd ingegeven door dezelfde/andere chauffeur
        /// </summary>
        /// <param name="opmerkingInvoer">De nieuwe opmerking van het type Comment</param>
        /// <returns>Een boolean</returns>


        #endregion

        #region NavigationHelper registration

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

        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        /// <summary>
        /// In deze methode worden de tekstvelden opgevuld met waarden uit de gekregen parameter 
        /// </summary>
        /// <param name="e"></param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            await GetphotoFolder();
            await GetVideoFolder();
            navigationHelper.OnNavigatedTo(e);
            if (e.Parameter != null)
            {
                opmerking = e.Parameter as Comment;
                if (opmerking != null)
                    updateComment = true;
            }
            else
            {
                updateComment = false;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        /// <summary>
        /// Returns true if the file is moved, false if it isn't or an error has occured. With this bool we change the message 
        /// displayed in "lblError".
        /// </summary>
        /// <param name="file"></param>
        /// <param name="folder"></param>
        /// <returns></returns>
        private async Task<bool> movePhotoVideo(StorageFile file, StorageFolder folder)
        {
            await file.MoveAsync(folder);
            string status = file.MoveAsync(folder).Status.ToString();
            while (status.Equals("Started"))
            {
                try
                {
                    status = file.MoveAsync(folder).Status.ToString();
                }
                catch(Exception e)
                {
                    goto BREAK;
                }
            }
            BREAK:
            if (status.Equals("Completed") || status.Equals("Error"))
            {
                return true;
            }
            else
            {
                return false;
            }            
        }

        /// <summary>
        /// Checks if the folder "Photos" already exists in the AppData folder, and if it doesn't exsists creates it.
        /// </summary>
        /// <returns></returns>
        private async Task GetphotoFolder()
        {
            try
            {
                photoFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync(LocalDB.database.GetAutoId() + @"\Photos");
            }
            catch
            { /*Can't create folder here because await is not allowed in the catch clause.*/ }

            if (photoFolder == null) //Dus als doelfolder == null -> hier aanmaken
                photoFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(LocalDB.database.GetAutoId() + @"\Photos");
        }

        /// <summary>
        /// Checks if the folder "Videos" already exists in the AppData folder, and if it doesn't exsists creates it.
        /// </summary>
        /// <returns></returns>
        private async Task GetVideoFolder()
        {
            try
            {
                videoFolder = await ApplicationData.Current.LocalFolder.GetFolderAsync(LocalDB.database.GetAutoId() + @"\Videos");
            }
            catch
            { /*Can't create folder here because await is not allowed in the catch clause.*/ }

            if (videoFolder == null) //Dus als doelfolder == null -> hier aanmaken
                videoFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(LocalDB.database.GetAutoId() + @"\Videos");
        }
    }
}