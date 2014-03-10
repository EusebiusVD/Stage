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
        /// Deze constructor vult een GridView op.
        /// Daarnaast laadt deze constructor een User Control in en toont deze op het scherm.
        /// Wanneer de gebruiker naar dit scherm gaat vanuit het inlogscherm, krijgt de gebruiker een pop-up
        /// te zien waar hij/zij het oliepeil en kilometerstand moet ingeven.
        /// </summary>
        public Hoofdscherm()
        {
            this.InitializeComponent();
            VulInspectieGridView();

            UserControls.Menu ucMenu = new UserControls.Menu(true);
            menuPanel.Children.Add(ucMenu);

            //Het aanmaken v de usercontrol+toevoegen an het scherm
            if (teller != 0)
            {
                UserControls.VisueleAuto ucWagen = new UserControls.VisueleAuto();
                grUserControl.Children.Add(ucWagen);
            }

            object boolAfstandsaanduiding = LocalStorage.localStorage.LaadGegevens("afstandsaanduiding");
            if (boolAfstandsaanduiding != null)
            {
                if (Convert.ToBoolean(boolAfstandsaanduiding))
                {
                    lblKilometerstand.Text = "Mijlstand:";
                    txtKilometerstand.PlaceholderText = "Mijlstand...";
                }
            }
        }

        /// <summary>
        /// Deze methode vraagt de meegegeven waarden van het inlogscherm op en toont daarna,
        /// naargelang de meegegeven waarden, een pop-up.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode != NavigationMode.Back && e.Parameter != null)
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
            }
        }

        #region GridView Events

        /// <summary>
        /// Zorgt ervoor dat alle inspecties uit de database opgehaald worden en gebonden worden aan het gridview.
        /// </summary>
        private async void VulInspectieGridView()
        {
            gvwScripten.HorizontalContentAlignment = Windows.UI.Xaml.HorizontalAlignment.Stretch;
            gvwScripten.Items.Add(await LocalDB.database.GetToegewezenAuto());
            gvwScripten.SelectedIndex = 0;
        }

        /// <summary>
        /// De geselecteerde index in een statische variabele bijhouden indien deze veranderd wordt door de gebruiker.
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
        /// Deze methode navigeert naar het inspecties scherm waar de testrijder kan zien welke inspecties
        /// nodig zijn op welke intervallen.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnStartRit_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(VerschillendeInspecties), geselecteerdeIndex + 1);
        }
        /// <summary>
        /// In deze methode worden de kilometerstand en het oliepeil weggeschreven
        /// De usercontrol van Visuelewagen aangemaakt + toevoegen aan het scherm en het hoofdscherm visible gezet
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BtnOpslaan_Click(object sender, RoutedEventArgs e)
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
        /// Via de knop Annuleren wordt er genavigeerd naar het inlogscherm
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnAnnuleren_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Inlogscherm));
        }

        /// <summary>
        /// Deze methode wordt telkens opgeroepen wanneer de slider verschoven wordt
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SlOliepeil_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            oliepeil = slOliepeil.Value * 100;
        }
        #endregion

        #region DoorgevenWagendetails
        /// <summary>
        /// Via deze methode kan het oliepeil doorgegeven worden aan andere pagina's
        /// </summary>
        /// <returns>Een int instantie</returns>
        public static double GetOliepeil()
        {
            return oliepeil;
        }

        public static void SetOliepeil(double oliepeilNieuw)
        {
            oliepeil = oliepeilNieuw;
        }

        /// <summary>
        /// Via deze methode kan de kilometerstand doorgegeven worden aan andere pagina's
        /// </summary>
        /// <returns>Een float instantie</returns>
        public static float GetKilometerstand()
        {
            return kilometerstandWagen;
        }

        #endregion
    }
}