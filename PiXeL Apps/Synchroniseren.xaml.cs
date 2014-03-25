using PiXeL_Apps.Classes;
using PiXeL_Apps.Common;
using PiXeL_Apps.WebserviceRef;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Connectivity;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.Storage.Pickers;
using System.Runtime.InteropServices;
using Windows.UI;

namespace PiXeL_Apps
{
    public sealed partial class Synchroniseren : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        public Synchroniseren()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;

            //Event voor het kijken naar internetconnectie
            NetworkInformation.NetworkStatusChanged += VeranderingConnectiviteit;
            VeranderToggle();
        }

        /// <summary>
        /// Deze methode veranderd de ToggleButton naargelang er internet is.
        /// Bij geen internet wordt de standeerdkeuze op USB gezet.
        /// Bij internet wordt dit op internet gezet.
        /// </summary>
        /// <param name="sender"></param>
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

        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
        }

        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
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
        /// Deze methode geeft een lijst van opmerkingen door naar de web service.
        /// De web service gebruikt deze gegevens om een .CSV-bestand te maken
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BtnSynchroniseren_Click(object sender, RoutedEventArgs e)
        {
            btnSynchroniseren.IsEnabled = false;
            prSynchroniseren.IsActive = true;
            lblBoodschap.Text = "Even geduld. Gegevens worden op dit moment verstuurd...";
            lblBoodschap.Visibility = Windows.UI.Xaml.Visibility.Visible;

            if (!swtManierOpslag.IsOn)
            {
                try
                {
                    await CopyFiles.copyPhotosToUSB();
                    await CopyFiles.copyVideosToUSB();
                    if (await LocalDB.database.SynchroniseerNaarUSB())
                    {
                        lblBoodschap.Foreground = new SolidColorBrush(Colors.White);
                        lblBoodschap.Text = "De gegevens zijn met succes verstuurd naar de USB!";
                    }
                    else
                    {
                        lblBoodschap.Foreground = new SolidColorBrush(Colors.Yellow);
                        lblBoodschap.Text = "Er is iets fout gelopen. Controleer of u een opslagmedium ingestoken heeft...";
                    }
                }
                catch (Exception)
                {
                    lblBoodschap.Foreground = new SolidColorBrush(Colors.Yellow);
                    lblBoodschap.Text = "Er is iets fout gelopen. Controleer of u een opslagmedium ingestoken heeft...";
                }
            }
            else
            {
                await CopyFiles.copyPhotosViaNetwork();
                await CopyFiles.copyVideosViaNetwork();

                List<String> lijstGegevens = new List<String>();
                List<Comment> opmerkingen = await OverzichtOpmerkingen.GetOverzichtComments();

                List<string> lijstStringGegevens = new List<string>();
                foreach (Comment opmerking in opmerkingen)
                {
                    lijstStringGegevens.Add("Chauffeur: " + opmerking.Chauffeur +
                        ", OpmperkingID: " + opmerking.Id.ToString() +
                        ", Aangemaakt op: " + opmerking.Datum.ToString("dd/MM/yyyy HH:mm") +
                        ", Objectcode: " + opmerking.ObjectCode +
                        ", Defectcode: " + opmerking.DefectCode +
                        ", Positie: " + opmerking.Position +
                        ", Rating: " + opmerking.Rating +
                        ", Omschrijving: " + opmerking.Omschrijving +
                        ", Duplicate: " + opmerking.Duplicate +
                        ", Origineel: " + opmerking.OriginalId);
                }

                ArrayOfString aosGegevens = new ArrayOfString(); //Een ArrayOfString is nodig voor de web service
                aosGegevens.AddRange(lijstStringGegevens);

                var avAuto = await LocalDB.database.GetToegewezenAuto();

                string datum = String.Format("Wagen {0} - {1}-{2}-{3} {4}h{5}m", avAuto.Number, DateTime.Now.Day, DateTime.Now.Month, DateTime.Now.Year,
                                                                                DateTime.Now.Hour, DateTime.Now.Minute);

                string bestandsNaam = String.Format("{0} opmerkingen.csv", datum);

                //await CopyFiles.copyVideosViaNetwork();
                lblBoodschap.Text = "Videos uploaden";

                if (await LocalDB.database.SetMaakSVCvoorWebSync(aosGegevens, bestandsNaam))
                {
                    lblBoodschap.Text = "De gegevens zijn met succes verstuurd naar de server!";
                }
                else
                {
                    lblBoodschap.Text = "Er is iets fout gelopen. Controleer of u met het juiste netwerk verbonden bent...";
                }

            }

            btnSynchroniseren.IsEnabled = true;
            prSynchroniseren.IsActive = false;
        }
        /// <summary>
        /// Does nothing, error when releasing app if this method doesn't exists
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BtnOpslaan_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}