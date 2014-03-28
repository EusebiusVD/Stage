using PiXeL_Apps.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using PiXeL_Apps.Classes;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PiXeL_Apps
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Hoofdscherm : Page
    {
        private static int geselecteerdeIndex = 0;
        private static float kilometerstandWagen = 0;
        private static int teller = 0;
        private static List<DefectCodes> defectCodes = new List<DefectCodes>();
        private static List<ObjectCodes> objectCodes = new List<ObjectCodes>();
        private static double oliepeil = 0.0;

        /// <summary>
        /// Constructor; Fills the GridView
        /// It also loads a User control and shows this on the screen
        /// When a user goes to this screen directly from the login screen, the user will get a popup where
        /// they have to fill in the oil reservoir level and mileage of the car
        /// </summary>
        public Hoofdscherm()
        {
            this.InitializeComponent();
            VulInspectieGridView();
            UserControls.Menu ucMenu = new UserControls.Menu(true);
            menuPanel.Children.Add(ucMenu);

            //Het aanmaken v de usercontrol+toevoegen an het scherm
            //if (teller != 0)
            //{
                UserControls.VisueleAuto ucWagen = new UserControls.VisueleAuto();
                grUserControl.Children.Add(ucWagen);
            //}

            object boolAfstandsaanduiding = LocalStorage.localStorage.LaadGegevens("afstandsaanduiding");
           /* if (boolAfstandsaanduiding != null)
            {
                if (Convert.ToBoolean(boolAfstandsaanduiding))
                {
                    lblKilometerstand.Text = "Mijlstand:";
                    txtKilometerstand.PlaceholderText = "Mijlstand...";
                }
            }*/
        }

        /// <summary>
        /// Asks the given values from the login screen and shows, depending on the values, a popup
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            /*if (e.NavigationMode != NavigationMode.Back && e.Parameter != null)
            {
                List<object> lijstGegevens = (List<object>)e.Parameter;

                if (Convert.ToBoolean(lijstGegevens[1])) //Vraag de boolean op
                {
                    grHoofdscherm.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    invoerPopup.IsOpen = true;
                }
                else
                {
                    grHoofdscherm.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    invoerPopup.IsOpen = false;
                }
            }
            else
            {
                grHoofdscherm.Visibility = Windows.UI.Xaml.Visibility.Visible;
                invoerPopup.IsOpen = false;
            }*/
        }

        #region GridView Events

        /// <summary>
        /// Gets all inspections from the database and and bind them to the gridview
        /// </summary>
        private async void VulInspectieGridView()
        {
            gvwScripten.HorizontalContentAlignment = Windows.UI.Xaml.HorizontalAlignment.Stretch;
            gvwScripten.Items.Add(await LocalDB.database.GetToegewezenAuto());
            gvwScripten.SelectedIndex = 0;
        }
        /// <summary>
        /// Holds the selected index in a static variable when changed by user
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GvwScripten_SelectieVeranderd(object sender, SelectionChangedEventArgs e)
        {
            geselecteerdeIndex = gvwScripten.SelectedIndex;
        }

        #endregion

        #region Button Clicks

        /// <summary>
        /// Navigates to the inspetion screen where the user can see which inspections are needed at 
        /// which milage/cylci
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnStartRit_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Inspecties), geselecteerdeIndex + 1);
        }
        /// <summary>
        /// Writes the mileage and oil level reservoir
        /// The user control "Visuelewagen" is created and made visible on the homescreen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /*private async void BtnOpslaan_Click(object sender, RoutedEventArgs e)
        {
            //int oliepeil;
            float kilometerstand;

            if (float.TryParse(txtKilometerstand.Text, out kilometerstand))
            {
                if (kilometerstand < 2147483647) //2147483647 = max. waarde Int32
                {
                    if (kilometerstand >= 0)
                    {
                        kilometerstandWagen = kilometerstand;

                        try
                        {
                            await LocalDB.database.SetkilometerstandEnOliepeil(oliepeil.ToString(), kilometerstand.ToString());
                        }
                        catch (Exception)
                        {
                            lblError.Text = "Er trad een fout op tijdens het opslaan van de gegevens, gelieve nog eens te proberen";
                        }

                        //Het aanmaken v de usercontrol+toevoegen aan het scherm
                        UserControls.VisueleAuto ucWagen = new UserControls.VisueleAuto();
                        grUserControl.Children.Add(ucWagen);
                        teller++;

                        //Gegevens wegschrijven
                        invoerPopup.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                        grHoofdscherm.Visibility = Windows.UI.Xaml.Visibility.Visible;
                        GpsSupport.gpsSupport.SetKilometersGereden(Convert.ToInt32(kilometerstand));
                    }
                    else
                    {
                        lblError.Text = "Uw ingevoerde afstand moet boven 0 zijn...";
                        lblError.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    }
                }
                else
                {
                    lblError.Text = "Uw ingevoerde afstand is veel te hoog...";
                    lblError.Visibility = Windows.UI.Xaml.Visibility.Visible;
                }
            }
            else
            {
                lblError.Text = "U hebt (een) verkeerde waarde(n) ingegeven...";
                lblError.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
        }
        /// <summary>
        /// Handles the click of btnAnnuleren. The user will be redirected to the login screen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnAnnuleren_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Inlogscherm));
        }

        /// <summary>
        /// Method that is called when SlOliepeil is slided
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SlOliepeil_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            oliepeil = slOliepeil.Value * 100;
        }*/
        #endregion

        #region DoorgevenWagendetails
        /// <summary>
        /// Passes the oil reservoir level to other screens
        /// </summary>
        /// <returns>Double</returns>
        public static double GetOliepeil()
        {
            return oliepeil;
        }

        public static void SetOliepeil(double oliepeilNieuw)
        {
            oliepeil = oliepeilNieuw;
        }

        /// <summary>
        /// Passes mileage to other screens
        /// </summary>
        /// <returns>Float</returns>
        public static float GetKilometerstand()
        {
            return kilometerstandWagen;
        }

        #endregion
    }
}