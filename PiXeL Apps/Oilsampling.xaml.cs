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
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace PiXeL_Apps
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class Oilsampling : Page
    {

        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        double oliepeil;

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


        public Oilsampling()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

            //Populate cbbOilUnit
            cbbOilUnit.Items.Add("CC");
            cbbOilUnit.Items.Add("Gram");
            cbbOilUnit.SelectedItem = "CC";

            //Populate cbbReasonOilSample
            cbbReasonOilSample.PlaceholderText = "Geef een reden op";
            cbbReasonOilSample.Items.Add("Voor top-up");
            cbbReasonOilSample.Items.Add("Na top-up");
            cbbReasonOilSample.Items.Add("Script");
            cbbReasonOilSample.Items.Add("Inspectie (Oliewissel)");
            cbbReasonOilSample.Items.Add("Andere");

            slOillevel.Value = OilOdoInput.GetOliepeil() / 100;
            if (!localSettings.Values.ContainsKey("Odometer"))
            {
                txtOdo.Text = "";
            }
            else
            {
                txtOdo.Text = localSettings.Values["Odometer"].ToString();
            }
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

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            string reason;
            string remarks;
            int odometer;
            int oiltaken;
            int oilfilled;
            double oillevel;
            string oilunit;

            if (await LocalDB.database.UpdateOliepeil(oliepeil.ToString("#.###")))
            {
                OilOdoInput.SetOliepeil(oliepeil);
                //lblError.Foreground = new SolidColorBrush(Colors.White);
                //lblError.Text = "De gegevens werden opgeslagen";
            }
            try
            {
                DateTime dateToday = DateTime.Today;

                if (txtOdo.Text != "")
                    odometer = Convert.ToInt32(txtOdo.Text);
                else
                    odometer = 0;
                
                oillevel = slOillevel.Value;

                if (txtOilTaken.Text != "")
                    oiltaken = Convert.ToInt32(txtOilTaken.Text);
                else
                    oiltaken = 0;

                if (txtOilFilled.Text != "")
                    oilfilled = Convert.ToInt32(txtOilFilled.Text);
                else
                    oilfilled = 0;

                if (cbbReasonOilSample.SelectedItem != null)
                    reason = cbbReasonOilSample.SelectedValue.ToString();
                else
                    reason = "";

                if (odometer == 0 || reason.Equals("") || (oiltaken == 0 && oilfilled == 0))
                {
                    lblError.Text = "Niet alle gegevens zijn (correct) ingevuld";
                }
                else //writing oilsample to LocalDB
                {
                    var car = LocalDB.database.GetToegewezenAuto();
                    int vehicle_id = car.Result.Id;

                    User user = LocalDB.database.GetIngelogdeGebruiker();
                    string user_id = user.Username;

                    remarks = txtRemark.Text;
                    oilunit = cbbOilUnit.SelectedValue.ToString();

                    Oilsample oilsample = new Oilsample(user_id, vehicle_id, dateToday, odometer, Math.Round(oillevel,3), oiltaken, oilfilled, oilunit, reason, remarks);

                    //bool passed = LocalDB.database.AddOilsample(oilsample).Result;

                    if (LocalDB.database.AddOilsample(oilsample).Result && await ToonOkAnnuleer("Het oliestaal werd succesvol opgeslagen", "Oliestaal opgeslagen"))
                    {
                        this.Frame.Navigate(typeof(Hoofdscherm));
                    }
                }

            }
            catch (Exception ex)
            {
                ToonOkAnnuleer("Er is iets misgelopen, probeer het opnieuw", "Oeps");
                paLogging.log.Info(ex.Message);
            } 
        }

        private void slOillevel_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            oliepeil = slOillevel.Value * 100;
        }

        public async Task<bool> ToonOkAnnuleer(string bericht, string titel, string okTekst = "OK")
        {
            MessageDialog okAnnuleer = new MessageDialog(bericht, titel);
            okAnnuleer.Commands.Add(new UICommand(okTekst));
            var antwoord = await okAnnuleer.ShowAsync();
            if (antwoord.Label.Equals(okTekst))
                return true;
            else
                return false;
        }
    }
}
