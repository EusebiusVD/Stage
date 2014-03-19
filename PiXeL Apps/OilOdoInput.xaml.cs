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
    public sealed partial class OilOdoInput : Page
    {
        private static float kilometerstandWagen = 0;
        private static double oliepeil = 0.0;
        public OilOdoInput()
        {
            this.InitializeComponent();
            slOliepeil.Value = GetOliepeil()/100;
        }
        /// <summary>
        /// Writes the mileage and oil level reservoir
        /// The user control "Visuelewagen" is created and made visible on the homescreen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnOpslaan_Click(object sender, RoutedEventArgs e)
        {
            if (!txtKilometerstand.Text.Equals("") && slOliepeil.Value.CompareTo(GetOliepeil()) == 0.0)
            {
                saveOdo();
            }
            else if (txtKilometerstand.Text.Equals("") && slOliepeil.Value.CompareTo(GetOliepeil()) != 0.0)
            {
                saveOilLevel();
            }
            else
            {
                saveOdoAndOil();
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
        }

        /// <summary>
        /// Passes the oil reservoir level to other screens
        /// </summary>
        /// <returns>Double</returns>
        public static double GetOliepeil()
        {
            return oliepeil;
        }

        /// <summary>
        /// Sets the oil reservoir level
        /// </summary>
        /// <returns>Double</returns>
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

        /// <summary>
        /// Handles the click on btnSkip
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSkip_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Hoofdscherm));
        }
        /// <summary>
        /// Saves just the odometer when the oil level hasn't changed
        /// </summary>
        private async void saveOdo()
        {
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
                            await LocalDB.database.Setkilometer(kilometerstand.ToString());
                            this.Frame.Navigate(typeof(Hoofdscherm));
                        }
                        catch (Exception)
                        {
                            lblError.Text = "Er trad een fout op tijdens het opslaan van de gegevens, gelieve nog eens te proberen.";
                            lblError.Text += "\nU kan dit ook overslaan door op de knop de klikken";
                        }

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
        /// saves the oil level when you didn't enter a mileage.
        /// </summary>
        private async void saveOilLevel()
        {
            try
            {
                {
                    await LocalDB.database.UpdateOliepeil(oliepeil.ToString("#.###"));
                    this.Frame.Navigate(typeof(Hoofdscherm));
                }
            }
            catch (Exception)
            {
                lblError.Foreground = new SolidColorBrush(Colors.Yellow);
                lblError.Text = "Er is iets fout gelopen bij de opslag, gelieve dit later opnieuw te proberen";
            }
        }

        /// <summary>
        /// Saves both milage and oil level
        /// </summary>
        private async void saveOdoAndOil()
        {
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
                            this.Frame.Navigate(typeof(Hoofdscherm));
                        }
                        catch (Exception)
                        {
                            lblError.Text = "Er trad een fout op tijdens het opslaan van de gegevens, gelieve nog eens te proberen.";
                            lblError.Text += "\nU kan dit ook overslaan door op de knop de klikken";
                        }

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
    }
}
