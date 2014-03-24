using PiXeL_Apps.Classes;
using PiXeL_Apps.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PiXeL_Apps.UserControls
{
    public sealed partial class Menu : UserControl
    {
        private Point beginPunt;

        /// <summary>
        /// In deze constructor wordt het menu wordt aangemaakt 
        /// </summary>
        /// <param name="statisch">boolean die aangeeft of het menu statisch is of niet</param>
        public Menu(bool statisch)
        {
            this.InitializeComponent();
            if (statisch)
            {
                btnHideShow.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                btnHideShow.IsEnabled = false;
                menu.Margin = new Thickness(0);
            }

            try
            {
                ToonGebruikerWagen();
            }
            catch (Exception) { }
        }
        /// <summary>
        /// Haalt de gebruikernaam en autonumer op en toont deze op het menu
        /// </summary>
        private async void ToonGebruikerWagen()
        {
            User gebruiker = LocalDB.database.GetIngelogdeGebruiker();
            CompleteAuto caAuto = await LocalDB.database.GetToegewezenAuto();

            lblGebruikerNmr.Text += gebruiker.Username;
            lblWagen.Text += caAuto.Number;
        }

        /// <summary>
        /// Begint een nieuw storyboard dat het menu grid uitschuift of inschuift naar een positieve (zichtbaar) of negatieve (onzichtbaar) margin.
        /// </summary>
        public void BeginMenuAnimatie()
        {
            Storyboard storyboard = new Storyboard();
            TranslateTransform beweegTransformatie = new TranslateTransform();
            menu.RenderTransform = beweegTransformatie;
            DoubleAnimation verplaatsMenuAnimatie = new DoubleAnimation();
            verplaatsMenuAnimatie.Duration = new Duration(TimeSpan.FromSeconds(0.15));
            if (btnHideShow.Content.Equals(">"))
            {
                verplaatsMenuAnimatie.To = 303;
                verplaatsMenuAnimatie.From = 0;
                btnHideShow.Content = "<";
            }
            else
            {
                verplaatsMenuAnimatie.To = 0;
                verplaatsMenuAnimatie.From = 303;
                btnHideShow.Content = ">";
            }

            storyboard.Children.Add(verplaatsMenuAnimatie);
            Storyboard.SetTarget(verplaatsMenuAnimatie, beweegTransformatie);
            Storyboard.SetTargetProperty(verplaatsMenuAnimatie, "X");
            storyboard.Begin();
        }
        /// <summary>
        /// Als het menu open is dan wordt true teruggegeven anders false
        /// </summary>
        /// <returns>Een boolean</returns>
        public bool IsMenuOpen()
        {
            if (btnHideShow.Content.Equals("<"))
                return true;
            else return false;
        }

        #region Button clicks

        /// <summary>
        /// Deze methode logt de gebruiker uit. Vervolgens gaat de gebruiker naar het inlogscherm
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnUitloggen_Click(object sender, RoutedEventArgs e)
        {
            ((Frame)Window.Current.Content).Navigate(typeof(Bijlagen));
        }

        /// <summary>
        /// Deze methode toont het scherm met alle opmerkingen.
        /// De gebruiker kan bovendien een opmerking aanmaken.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnOpmerkingen_Click(object sender, RoutedEventArgs e)
        {
            ((Frame)Window.Current.Content).Navigate(typeof(OverzichtOpmerkingen));
        }

        /// <summary>
        /// Deze methode navigeert naar naar het tabscherm
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnTabs_Click(object sender, RoutedEventArgs e)
        {
            ((Frame)Window.Current.Content).Navigate(typeof(Tabs));
        }

        /// <summary>
        /// Deze methode navigeert naar naar het synchronisatiescherm
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSynchroniseren_Click(object sender, RoutedEventArgs e)
        {
            ((Frame)Window.Current.Content).Navigate(typeof(Synchroniseren));
        }

        /// <summary>
        /// Deze knop start de animatie zonder activering door een swype gesture.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnHideShow_Click(object sender, RoutedEventArgs e)
        {
            BeginMenuAnimatie();
        }

        /// <summary>
        /// Deze methode navigeert naar naar het hoofdscherm
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnHome_Click(object sender, RoutedEventArgs e)
        {
            ((Frame)Window.Current.Content).Navigate(typeof(Hoofdscherm));
        }

        #endregion

        private void btnOilsampling_Click(object sender, RoutedEventArgs e)
        {
            ((Frame)Window.Current.Content).Navigate(typeof(Oilsampling));
        }


    }
}