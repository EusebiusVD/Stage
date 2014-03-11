using PiXeL_Apps.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236
namespace PiXeL_Apps.UserControls
{
    public sealed partial class Wagenmap : UserControl
    {
        /// <summary>
        /// Deze constructor roept de methode 'ToonWagenmap()' op.
        /// </summary>
        public Wagenmap()
        {
            this.InitializeComponent();
            ToonWagenmap();
        }

        /// <summary>
        /// Deze methode haalt de volledige auto op en toont deze vervolgens op het scherm.
        /// </summary>
        private async void ToonWagenmap()
        {
            CompleteAuto caAuto = await LocalDB.database.GetToegewezenAuto();

            List<string> lijstWagenmap = new List<string>();
            if (caAuto.Merk_Naam != null)
            {
                lijstWagenmap.Add("Wagen: " + caAuto.Merk_Naam);
                lijstWagenmap.Add("Merk: " + caAuto.Merk_Naam);
                lijstWagenmap.Add("Type: " + caAuto.Merk_Type);
                lijstWagenmap.Add("Vermogen: " + caAuto.Motor_Capaciteit);
                lijstWagenmap.Add("Aantal schakelingen: " + caAuto.Transmissie_Versnellingen);
                lijstWagenmap.Add("Motor: " + caAuto.Motor_Code);
                lijstWagenmap.Add("Brandstof: " + caAuto.Motor_Brandstof_Naam);
            }
            else
            {
                lijstWagenmap.Add("Deze tablet is nog niet toegewezen aan een wagen...");
                lblWagenmap.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }

            lvWagenmap.ItemsSource = lijstWagenmap;
            lvWagenmap.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }
    }
}