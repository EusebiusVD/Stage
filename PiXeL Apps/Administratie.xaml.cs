﻿using PiXeL_Apps.Classes;
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

        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Deze constructor vult o.a. de combobox op op het scherm
        /// </summary>
        public Administratie()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
            //Event voor het kijken naar internetconnectie
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
                lblDatabankPad.Text = @"C:\PiXel-Apps";
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
        /// Deze methode zorgt dat er een lijst is van wagens in de databank. Daarna wordt deze lijst in de combobox gestoken.
        /// </summary>
        private async void PopulateCombobox()
        {
            wagenOpties = await LocalDB.database.GetAutos(true);
            cbbSelecteerWagen.ItemsSource = wagenOpties;
        }

        /// <summary>
        /// Deze methode wordt opgeroepen na het intypen van een letter in de combobox. Deze methode zorgt er dan voor dat de items met
        /// de ingegeven letters getoond wordt.
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
                //Geen internet -> USB
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
        /// Deze methode zorgt ervoor dat een tablet toegewezen kan worden aan een wagen.
        /// Deze methode gaat via de lokale databank de wagen toekennen aan de tablet.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BtnWijzigAuto_Click(object sender, RoutedEventArgs e)
        {
            try
            {
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
                    btnWijzigAuto.IsEnabled = true;
                    prSynchroniseren.IsActive = false;
                    BtnSynchroniseren_Click(sender, e);
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
        /// Deze methode zorgt ervoor dat de databank uitgelezen en lokaal opgeslagen wordt.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BtnSynchroniseren_Click(object sender, RoutedEventArgs e)
        {
            var gebruikersTest = await LocalDB.database.ControleerGebruikers();
            if (gebruikersTest)
            {
                //Bestand bestaat
                btnSynchroniseren.IsEnabled = false;
                prSynchroniseren.IsActive = true;
                lblFeedback.Foreground = new SolidColorBrush(Colors.White);
                lblFeedback.Text += "\nEven geduld. De gegevens worden op dit moment binnengehaald...";
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
        /// Deze methode zorgt ervoor dat de gebruiker een path kan instellen waarheen de bestanden gestuurd worden
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

        private void SwtOpmerking_Toggled(object sender, RoutedEventArgs e)
        {
            //True = Ja
            //False = Nee
            Common.LocalStorage.localStorage.SlaGegevensOp("rapporteerDubbeleOpmerking", swtOpmerking.IsOn);
        }

        private void SwtAfstandsaanduiding_Toggled(object sender, RoutedEventArgs e)
        {
            //True = Mijl
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