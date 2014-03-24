using PiXeL_Apps.Classes;
using PiXeL_Apps.Common;
using PiXeL_Apps.Logging;
using PiXeL_Apps.SQLite.Tables;
using PiXeL_Apps.WebserviceRef;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Connectivity;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;


namespace PiXeL_Apps
{

    public sealed partial class Administratie : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private String wagenNummer;
        private List<Vehicle> wagenOpties, wagenOptiesFilter;
        private int tolerantieAankomend, tolerantieDringend;
        private static List<string> filterOpmerking = new List<string>();
        ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
        StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Constructor; responsible for filling the combobox
        /// </summary>
        public Administratie()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
            //Event for looking at networkconnection
            NetworkInformation.NetworkStatusChanged += VeranderingConnectiviteit;
            VeranderToggle();

            object toegewezenWagen = LocalStorage.localStorage.LaadGegevens("toegewezenWagen");
            if (toegewezenWagen != null)
                pageTitle.Text = "Administratie: wagen " + toegewezenWagen;

            object intTolerantieAankomend = LocalStorage.localStorage.LaadGegevens("tolerantieAankomend");
            object intTolerantieDringend = LocalStorage.localStorage.LaadGegevens("tolerantieDringend");
            if (intTolerantieAankomend == null || !int.TryParse(intTolerantieAankomend.ToString(), out tolerantieAankomend))
                tolerantieAankomend = 200;
            if (intTolerantieDringend == null || !int.TryParse(intTolerantieDringend.ToString(), out tolerantieDringend))
                tolerantieDringend = 50;

            txtTolerantieAankomend.Text = tolerantieAankomend.ToString();
            txtTolerantieDringend.Text = tolerantieDringend.ToString();

            try
            {
                string pad = Common.LocalStorage.localStorage.LaadGegevens("padMapDatabank").ToString();
                if (pad.Length > 33)
                    lblDatabankPad.Text = pad.Substring(0, 33) + "...";
                else
                    lblDatabankPad.Text = pad;
            }
            catch (Exception)
            {
                lblDatabankPad.Text = @"Er is nog geen databank geselecteerd";
                lblDatabankPad.Foreground = new SolidColorBrush(Colors.Yellow);
            }

            object boolRapporteerDubbel = LocalStorage.localStorage.LaadGegevens("rapporteerDubbeleOpmerking");
            if (boolRapporteerDubbel != null)
            {
                var controle = Windows.Storage.ApplicationData.Current.LocalSettings.Values;
                swtOpmerking.IsOn = Convert.ToBoolean(boolRapporteerDubbel);
            }
            else
            {
                Common.LocalStorage.localStorage.SlaGegevensOp("rapporteerDubbeleOpmerking", true);
                swtOpmerking.IsOn = true;
            }

            object boolAfstandsaanduiding = LocalStorage.localStorage.LaadGegevens("afstandsaanduiding");
            if (boolAfstandsaanduiding != null)
                swtAfstandsaanduiding.IsOn = Convert.ToBoolean(boolAfstandsaanduiding);
            else
                Common.LocalStorage.localStorage.SlaGegevensOp("afstandsaanduiding", false);

            PopulateCombobox();

            //object vl = Common.LocalStorage.localStorage.LaadGegevens("bandVoorLinks");
            //object vr = Common.LocalStorage.localStorage.LaadGegevens("bandVoorRechts");
            //object al = Common.LocalStorage.localStorage.LaadGegevens("bandAchterLinks");
            //object ar = Common.LocalStorage.localStorage.LaadGegevens("bandAchterRechts");

            //if(vl != null && vr != null && al != null && ar != null)
            //{
            //    //Alles wordt mee opgeslagen
            //    txtbandenspanningvoorlinks.Text = vl.ToString();
            //    txtbandenspanningvoorrechts.Text = vr.ToString();
            //    txtbandenspanningachterlinks.Text = al.ToString();
            //    txtbandenspanningachterrechts.Text = ar.ToString();
            //}
        }

        #region Populate Combobox

        /// <summary>
        /// Gets the list of vehicles from the database and puts them in the "cbbSelecteerWagen".
        /// </summary>
        private async void PopulateCombobox()
        {
            wagenOpties = await LocalDB.database.GetAutos(true);
            cbbSelecteerWagen.ItemsSource = wagenOpties;
            string a;
        }

        /// <summary>
        /// Adapts the list in the combobox when a letter is typed. Unfortunally there are no free toolkits that supports editable 
        /// comboboxes at the moment. This method is here so it can be implemented easilly.
        /// </summary>
        private void PopulateComboboxFilter()
        {
           cbbSelecteerWagen.ItemsSource = wagenOptiesFilter;
        }
        async void VeranderingConnectiviteit(object sender)
        {
            VeranderToggle();
        }

        private async void VeranderToggle()
        {
            if (Common.InternetControle.ControleerInternet())
            {
                //Internet
                //UI Thread
                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => swtManierOpslag.IsOn = true);
                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => swtManierOpslag.IsEnabled = true);
            }
            else
            {
                //no network -> USB
                //UI Thread
                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => swtManierOpslag.IsEnabled = false);
                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => swtManierOpslag.IsOn = false);
            }
        }

        #endregion

        #region NavigationHelper registration

        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
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

        #region Button
        /// <summary>
        /// This method handles the assignment of the tablet to a vehicle.
        /// This happens via the local sqlite database
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BtnWijzigAuto_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                localSettings.Values.Remove("Oillevel");
                localSettings.Values.Remove("Odometer");
                Vehicle v = (Vehicle)cbbSelecteerWagen.SelectedItem;
                btnWijzigAuto.IsEnabled = false;
                prSynchroniseren.IsActive = true;
                lblFeedback.Text = "De tablet wordt toegewezen aan wagen " + v.Number;
                await LocalDB.database.CreateDatabase(false);

                if (InternetControle.ControleerInternet())
                    await PdfSupport.pdfSupport.maakPDFBestanden(true);
                else
                {
                    MessageDialog okAnnuleer = new MessageDialog("U heeft geen internet, gelieve naar een Wi-Fi zone te gaan.\nStart de applicatie opnieuw op om bijlagen op te halen..", "Wi-Fi vereist");
                    okAnnuleer.Commands.Add(new UICommand("Begrepen"));
                }
                string feedback = await LocalDB.database.SetToegewezenAuto(v.Number);

                if (feedback != null && !feedback.Equals(""))
                {
                    wagenNummer = v.Number;
                    Common.LocalStorage.localStorage.SlaGegevensOp("toegewezenWagen", wagenNummer);
                    lblFeedback.Text = "Tablet is met succes toegewezen aan wagen " + wagenNummer;
                    pageTitle.Text = "Administratie: wagen " + wagenNummer;

                    lblFeedback.Foreground = new SolidColorBrush(Colors.White);
                    lblFeedback.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    var gebruikersTest = await LocalDB.database.ControleerGebruikers();
                    if (gebruikersTest)
                    {
                        //File exists
                        btnSynchroniseren.IsEnabled = false;
                        prSynchroniseren.IsActive = true;
                        lblFeedback.Foreground = new SolidColorBrush(Colors.White);
                        lblFeedback.Text += "\nEven geduld. De gegevens worden op dit moment binnengehaald...";
                        lblFeedback.Visibility = Windows.UI.Xaml.Visibility.Visible;

                        await LocalDB.database.CreateDatabase(false);

                        btnSynchroniseren.IsEnabled = true;
                        prSynchroniseren.IsActive = false;
                        lblFeedback.Foreground = new SolidColorBrush(Colors.White);
                        lblFeedback.Text = "Gegevens zijn met succes binnengehaald!";
                    }
                    else
                    {
                        lblFeedback.Foreground = new SolidColorBrush(Colors.Yellow);
                        lblFeedback.Text = "Er is iets fout gelopen. Controleer of u met het juiste netwerk verbonden bent";
                        prSynchroniseren.IsActive = false;
                    }
                    btnWijzigAuto.IsEnabled = true;
                    prSynchroniseren.IsActive = false;
                    //BtnSynchroniseren_Click(sender, e);
                }
            }
            catch (NullReferenceException)
            {
                lblFeedback.Text = "Gelieve eerst een auto te kiezen...";
                lblFeedback.Foreground = new SolidColorBrush(Colors.Yellow);
                lblFeedback.Visibility = Windows.UI.Xaml.Visibility.Visible;
                prSynchroniseren.IsActive = false;
            }
        }

        /// <summary>
        /// This method reads the database specified and saves it locally
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BtnSynchroniseren_Click(object sender, RoutedEventArgs e)
        {
            var gebruikersTest = await LocalDB.database.ControleerGebruikers();
            if (gebruikersTest)
            {
                //File exists
                btnSynchroniseren.IsEnabled = false;
                prSynchroniseren.IsActive = true;
                lblFeedback.Foreground = new SolidColorBrush(Colors.White);
                lblFeedback.Text = "Even geduld. De gegevens worden op dit moment binnengehaald...";
                lblFeedback.Visibility = Windows.UI.Xaml.Visibility.Visible;

                await LocalDB.database.CreateDatabase(true);

                btnSynchroniseren.IsEnabled = true;
                prSynchroniseren.IsActive = false;
                lblFeedback.Foreground = new SolidColorBrush(Colors.White);
                lblFeedback.Text = "Gegevens zijn met succes binnengehaald!";
            }
            else
            {
                lblFeedback.Foreground = new SolidColorBrush(Colors.Yellow);
                lblFeedback.Text = "Er is iets fout gelopen. Controleer of u met het juiste netwerk verbonden bent";
                prSynchroniseren.IsActive = false;
            }
        }

        /// <summary>
        /// Changes the path to the MS Access database
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BtnDatabankKiezer_Click(object sender, RoutedEventArgs e)
        {
            var nieuwPad = new FolderPicker();
            nieuwPad.CommitButtonText = "Kies";
            nieuwPad.SuggestedStartLocation = PickerLocationId.HomeGroup;
            nieuwPad.ViewMode = PickerViewMode.List;
            nieuwPad.FileTypeFilter.Add(".accdb"); //Nodig, je moet er minstens één hebben.
            nieuwPad.FileTypeFilter.Add(".mdb");

            var folder = await nieuwPad.PickSingleFolderAsync();
            if (folder == null) return;

            string pad = folder.Path + "\\";
            LocalDB.database.SetConnectieString(pad);
            Common.LocalStorage.localStorage.SlaGegevensOp("padMapDatabank", pad);

            if (pad.Length > 33)
                lblDatabankPad.Text = pad.Substring(0, 33) + "...";
            else
                lblDatabankPad.Text = pad;
        }
        /// <summary>
        /// Sets the tolerance for oncomming inspections
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnTolerantieAankomend_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(txtTolerantieAankomend.Text, out tolerantieAankomend))
            {
                if (tolerantieAankomend > tolerantieDringend)
                {
                    Common.LocalStorage.localStorage.SlaGegevensOp("tolerantieAankomend", tolerantieAankomend);
                    GpsSupport.gpsSupport.SetTolerantieAankomend(tolerantieAankomend);
                    lblFeedback.Text = "De tolerantie voor aankomende kilometers is ingesteld";
                    lblFeedback.Foreground = new SolidColorBrush(Colors.White);
                }
                else
                {
                    lblFeedback.Text = "Gelieve een positieve tolerantie in te geven die groter is dan dringend...";
                    lblFeedback.Foreground = new SolidColorBrush(Colors.Yellow);
                }
            }
            else
            {
                lblFeedback.Text = "Gelieve een geldige aankomende tolerantie in te geven...";
                lblFeedback.Foreground = new SolidColorBrush(Colors.Yellow);
            }
        }

        /// <summary>
        /// Sets the tolerance for urgent inspections
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnTolerantieDringend_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(txtTolerantieDringend.Text, out tolerantieDringend))
            {
                if (tolerantieDringend > 0 && tolerantieDringend < tolerantieAankomend)
                {
                    Common.LocalStorage.localStorage.SlaGegevensOp("tolerantieDringend", tolerantieDringend);
                    GpsSupport.gpsSupport.SetTolerantieDringend(tolerantieDringend);
                    lblFeedback.Text = "De tolerantie voor dringende kilometers is ingesteld";
                    lblFeedback.Foreground = new SolidColorBrush(Colors.White);
                }
                else
                {
                    lblFeedback.Text = "Gelieve een positieve tolerantie in te geven dat kleiner is dan aankomend...";
                    lblFeedback.Foreground = new SolidColorBrush(Colors.Yellow);
                }
            }
            else
            {
                lblFeedback.Text = "Gelieve een geldige dringende tolerantie in te geven...";
                lblFeedback.Foreground = new SolidColorBrush(Colors.Yellow);
            }
        }
        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            ((Frame)Window.Current.Content).Navigate(typeof(Bijlagen));
        }

        /// <summary>
        /// Method that handles the synchronisation of the rijberichten
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnSyncRijberichten_Click(object sender, RoutedEventArgs e)
        {
            btnSyncRijberichten.IsEnabled = false;
            prSynchroniseren.IsActive = true;
            lblFeedback.Text = "Even geduld. Gegevens worden op dit moment verstuurd...";

            if (!swtManierOpslag.IsOn)
            {
                try
                {
                    if (await LocalDB.database.SynchroniseerNaarUSB())
                    {
                        lblFeedback.Text = "De gegevens zijn met succes verstuurd naar de USB!";
                    }
                    else
                    {
                        lblFeedback.Text = "Er is iets fout gelopen. Controleer of u een opslagmedia ingestoken heeft...";
                    }
                }
                catch (Exception)
                {
                    lblFeedback.Text = "Er is iets fout gelopen. Controleer of u een opslagmedia ingestoken heeft...";
                }
            }
            else
            {
                List<String> lijstGegevens = new List<String>();
                List<Comment> opmerkingen = await OverzichtOpmerkingen.GetOverzichtComments();

                List<string> lijstStringGegevens = new List<string>();
                foreach (Comment opmerking in opmerkingen)
                {
                    lijstStringGegevens.Add("Chauffeur: " + opmerking.Chauffeur +
                        ", Aangemaakt op: " + opmerking.Datum.ToString("dd/MM/yyyy HH:mm") +
                        ", Objectcode: " + opmerking.ObjectCode +
                        ", Defectcode: " + opmerking.DefectCode +
                        ", Omschrijving: " + opmerking.Omschrijving);
                }

                ArrayOfString aosGegevens = new ArrayOfString(); //Een ArrayOfString is nodig voor de web service
                aosGegevens.AddRange(lijstStringGegevens);

                var avAuto = await LocalDB.database.GetToegewezenAuto();

                string datum = String.Format("Wagen {0} - {1}-{2}-{3} {4}h{5}m", avAuto.Number, DateTime.Now.Day, DateTime.Now.Month, DateTime.Now.Year,
                                                                                DateTime.Now.Hour, DateTime.Now.Minute);

                string bestandsNaam = String.Format("{0} opmerkingen.csv", datum);

                if (await LocalDB.database.SetMaakSVCvoorWebSync(aosGegevens, bestandsNaam))
                {
                    lblFeedback.Text = "De gegevens zijn met succes verstuurd naar de server!";
                }
                else
                {
                    lblFeedback.Text = "Er is iets fout gelopen. Controleer of u met het juiste netwerk verbonden bent...";
                }
            }

            btnSyncRijberichten.IsEnabled = true;
            prSynchroniseren.IsActive = false;
        }

        #endregion

        /// <summary>
        /// Lets you choose to rapport and concat double rijberichten
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SwtOpmerking_Toggled(object sender, RoutedEventArgs e)
        {
            //True = yes
            //False = no
            Common.LocalStorage.localStorage.SlaGegevensOp("rapporteerDubbeleOpmerking", swtOpmerking.IsOn);
        }

        /// <summary>
        /// Defines the odometer in the car
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SwtAfstandsaanduiding_Toggled(object sender, RoutedEventArgs e)
        {
            //True = Mile
            //False = Kilometers
            Common.LocalStorage.localStorage.SlaGegevensOp("afstandsaanduiding", swtAfstandsaanduiding.IsOn);
        }

    

          //private void btnBandenspanningOpslaan_Click(object sender, RoutedEventArgs e)
        //{
        //    Common.LocalStorage.localStorage.SlaGegevensOp("bandVoorLinks", txtbandenspanningvoorlinks.Text);
        //    Common.LocalStorage.localStorage.SlaGegevensOp("bandVoorRechts", txtbandenspanningvoorrechts.Text);
        //    Common.LocalStorage.localStorage.SlaGegevensOp("bandAchterLinks", txtbandenspanningachterlinks.Text);
        //    Common.LocalStorage.localStorage.SlaGegevensOp("bandAchterRechts", txtbandenspanningachterrechts.Text);
        //}
    }
}