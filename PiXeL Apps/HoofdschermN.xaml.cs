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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PiXeL_Apps
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Hoofdscherm : Page
    {
        private static int geselecteerdeIndex = 0;

        /// <summary>
        /// Deze constructor vult een GridView op.
        /// Daarnaast laad deze constructor een User Control in en toont deze op het scherm.
        /// </summary>
        public Hoofdscherm()
        {
            this.InitializeComponent();
            VulInspectieGridView();

            UserControls.VisueleAuto ucWagen = new UserControls.VisueleAuto();
            grUserControl.Children.Add(ucWagen);
            OverzichtOpmerkingen.HaalCommentsOp();
            ProblemenTest.HaalCodesOp();
        }

        #region GridView Events

        /// <summary>
        /// Zorgt ervoor dat alle inspecties uit de database opgehaald worden en gebonden worden aan het gridview.
        /// </summary>
        private void VulInspectieGridView()
        {
            gvwScripten.HorizontalContentAlignment = Windows.UI.Xaml.HorizontalAlignment.Stretch;
            gvwScripten.Items.Add(LocalDB.database.GetToegewezenAuto());
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
        /// Deze methode logt de gebruiker uit. Vervolgens gaat de gebruiker naar het inlogscherm
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnUitloggen_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Inlogscherm));
        }

        /// <summary>
        /// Deze methode zorgt ervoor dat de gebruiker belangrijke informatie kan zien
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnInformatie_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(BelangrijkeInformaties));
        }

        /// <summary>
        /// Deze methode toont het scherm met alle opmerkingen.
        /// De gebruiker kan bovendien een opmerking aanmaken.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnOpmerkingen_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(OverzichtOpmerkingen));
        }

        /// <summary>
        /// Deze methode navigeert naar naar het metingenscherm
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnMetingen_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Tabs));
        }

        /// <summary>
        /// Deze methode navigeert naar naar het inspecties scherm waar de testrijder kan zien welke inspecites
        /// nodig zijn op welke intervallen.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnStartRit_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(VerschillendeInspecties), geselecteerdeIndex + 1);
        }

        #endregion

        private void BtnSynchroniseren_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}