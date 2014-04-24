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
        bool durability = Convert.ToBoolean(Common.LocalStorage.localStorage.LaadGegevens("afdeling"));

        /// <summary>
        /// This constructor makes the menu.
        /// In deze constructor wordt het menu wordt aangemaakt.
        /// </summary>
        /// <param name="statisch">bool which indicates if the menu is static.</param>
        public Menu(bool statisch)
        {
            this.InitializeComponent();
            if (statisch)
            {
                btnHideShow.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                btnHideShow.IsEnabled = false;
                menuGrid.Margin = new Thickness(0);
            }
            btnRoutes.Visibility = Visibility.Collapsed;
            btnInspecties.Visibility = Visibility.Collapsed;

            if (!durability)
            {
                btnInspecties.Visibility = Visibility.Collapsed;
                btnRoutes.Visibility = Visibility.Visible;
            }
            else
            {
                btnInspecties.Visibility = Visibility.Visible;
                btnRoutes.Visibility = Visibility.Collapsed;
            }
            try
            {
                ToonGebruikerWagen();
            }
            catch (Exception) { }
        }
        /// <summary>
        /// Gets the username and carnumber to show in the menu.
        /// Haalt de gebruikernaam en autonummer op en toont deze op het menu.
        /// </summary>
        private async void ToonGebruikerWagen()
        {
            User gebruiker = LocalDB.database.GetIngelogdeGebruiker();
            CompleteAuto caAuto = await LocalDB.database.GetToegewezenAuto();

            lblGebruikerNmr.Text += gebruiker.Username;
            lblWagen.Text += caAuto.Number;
        }

        /// <summary>
        /// Starts a new storyboard which slides out the menu grid to a positive (visible) margin or a negative (invisible) margin.
        /// Begint een nieuw storyboard dat het menu grid uitschuift of inschuift naar een positieve (zichtbaar) of negatieve (onzichtbaar) margin.
        /// </summary>
        public void BeginMenuAnimatie()
        {
            Storyboard storyboard = new Storyboard();
            TranslateTransform beweegTransformatie = new TranslateTransform();
            menuGrid.RenderTransform = beweegTransformatie;
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
        /// If the menu is open it returns true, else it returns false.
        /// </summary>
        /// <returns>bool</returns>
        public bool IsMenuOpen()
        {
            if (btnHideShow.Content.Equals("<"))
                return true;
            else return false;
        }

        #region Button clicks

        /// <summary>
        /// This method logs of the user. The user goes back to the Inlogscherm screen.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnUitloggen_Click(object sender, RoutedEventArgs e)
        {
            ((Frame)Window.Current.Content).Navigate(typeof(Bijlagen));
        }

        /// <summary>
        /// This method navigates to the OverzichtOpmerkingen screen.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnOpmerkingen_Click(object sender, RoutedEventArgs e)
        {
            ((Frame)Window.Current.Content).Navigate(typeof(OverzichtOpmerkingen));
        }

        /// <summary>
        /// This method navigates to the Tabs screen.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnTabs_Click(object sender, RoutedEventArgs e)
        {
            if (durability)
            {
                ((Frame)Window.Current.Content).Navigate(typeof(TabsDURA));
            }
            else
            {
                ((Frame)Window.Current.Content).Navigate(typeof(TabsVOCF));
            }

        }

        /// <summary>
        /// This method navigates to the Syncronisatie screen.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSynchroniseren_Click(object sender, RoutedEventArgs e)
        {
            ((Frame)Window.Current.Content).Navigate(typeof(Synchroniseren));
        }

        /// <summary>
        /// This button starts the menu animation without a swype action, when the button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnHideShow_Click(object sender, RoutedEventArgs e)
        {
            BeginMenuAnimatie();
        }

        /// <summary>
        /// This method navigates to the Hoofdscherm screen;
        /// This is the main screen.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnHome_Click(object sender, RoutedEventArgs e)
        {
            ((Frame)Window.Current.Content).Navigate(typeof(Hoofdscherm));
        }

        /// <summary>
        /// This method navigates to the Oilsampling screen.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOilsampling_Click(object sender, RoutedEventArgs e)
        {
            ((Frame)Window.Current.Content).Navigate(typeof(Oilsampling));
        }

        /// <summary>
        /// This method navigates to the Feedback screen.
        /// This screen is made for us. Users can provide feedback during testing.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnFeedback_Click(object sender, RoutedEventArgs e)
        {
            ((Frame)Window.Current.Content).Navigate(typeof(Feedback));
        }

        /// <summary>
        /// This method navigates to the Inspecties screen.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnInspecties_Click(object sender, RoutedEventArgs e)
        {
            ((Frame)Window.Current.Content).Navigate(typeof(Inspecties));
        }
        #endregion

        private void btnRoutes_Click(object sender, RoutedEventArgs e)
        {
            ((Frame)Window.Current.Content).Navigate(typeof(Routes));
        }
    }
}