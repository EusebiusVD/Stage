using PiXeL_Apps.Classes;
using PiXeL_Apps.Common;
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
using Windows.Storage;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PiXeL_Apps.UserControls
{
    public sealed partial class VisueleAuto : UserControl
    {
        private float kilometerstand;
        private double oliepeil;
        private string afstandssoort, afkortingAfstand;
        ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
        StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

        /// <summary>
        /// In deze constructor wordt de de methode ToevoegenGegevensAfbeelding() en HaalBandenSpanningOp() opgeroepen
        /// </summary>
        public VisueleAuto()
        {
            this.InitializeComponent();
            object boolAfstandsaanduiding = LocalStorage.localStorage.LaadGegevens("afstandsaanduiding");
            if (boolAfstandsaanduiding != null)
            {
                if (Convert.ToBoolean(boolAfstandsaanduiding))
                {
                    afstandssoort = "Mijlstand: ";
                    afkortingAfstand = " mijl";
                }
                else
                {
                    afstandssoort = "Kilometerstand: ";
                    afkortingAfstand = " km";
                }
            }
            else
            {
                afstandssoort = "Kilometerstand: ";
                afkortingAfstand = " km";
            }

            ToevoegenGegevensAfbeelding();
            HaalBandenSpanningOp();
        }

        /// <summary>
        /// De bandenspanning wordt opgehaald uit het excel document en getoond in de virtuele wagenmap
        /// </summary>
        public async void HaalBandenSpanningOp()
        {
            var lijstGewichten = await LocalDB.database.GetGewichtenExcel();
            if (lijstGewichten != null)
            {
               String linksVoor= lijstGewichten.ElementAt(5);
               String rechtsVoor = lijstGewichten.ElementAt(6);
               String linksAchter = lijstGewichten.ElementAt(7);
               String rechtsAchter = lijstGewichten.ElementAt(8);
               lblBandenSpanningVoor.Text = "L: " + linksVoor + " bar, R: " + rechtsVoor + " bar";
               lblBandenSpanningAchter.Text = "L: " + linksAchter + " bar, R: " + rechtsAchter + " bar";
            }
            else
            {
                lblBandenSpanningAchter.Text = "N/A";
                lblBandenSpanningVoor.Text = "N/A";
            }
        }
        /// <summary>
        /// Het zetten van oliepeil en kilometerstand op de afbeelding
        /// </summary>
        private async void ToevoegenGegevensAfbeelding()
        {

            CompleteAuto caAuto = (CompleteAuto) await LocalDB.database.GetToegewezenAuto();
            
            kilometerstand = OilOdoInput.GetKilometerstand();
            oliepeil = OilOdoInput.GetOliepeil();
            if (!localSettings.Values.ContainsKey("Odometer"))
            {
                lblKilometerstand.Text = afstandssoort + kilometerstand.ToString() + afkortingAfstand;
            }
            else
            {
                lblKilometerstand.Text = afstandssoort + localSettings.Values["Odometer"].ToString() + afkortingAfstand;
            }
            if (oliepeil == 0.0)
            {
                lblOliepeil.Text = "Oliepeil: 0" + oliepeil.ToString("#.##") + " %";
            }
            else
            {
                lblOliepeil.Text = "Oliepeil: " + oliepeil.ToString("#.##") + " %";
            }
        }

        public static readonly DependencyProperty ExpanderContentProperty =
                    DependencyProperty.Register("ExpanderContent", typeof(object),
                    typeof(VisueleAuto), new PropertyMetadata(null));

        public object ExpanderContent
        {
            get { return (bool)GetValue(ExpanderContentProperty); }
            set { SetValue(ExpanderContentProperty, value); }
        }


        public static readonly DependencyProperty ExpanderHeaderProperty =
                    DependencyProperty.Register("ExpanderHeader", typeof(string),
                    typeof(VisueleAuto), new PropertyMetadata("Header Text"));

        public string ExpanderHeader
        {
            get { return (string)GetValue(ExpanderHeaderProperty); }
            set { SetValue(ExpanderHeaderProperty, value); }
        }
    }
}
