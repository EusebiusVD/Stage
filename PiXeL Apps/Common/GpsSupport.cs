using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;

namespace PiXeL_Apps.Common
{
    /// <summary>
    /// Wordt gebruikt door de GpsSupport klasse om Observers te waarschuwen over een recente update.
    /// </summary>
    /// <param name="kilometersGereden">Het aantal gereden kilometers van het type int</param>
    public delegate void GeoLocator_PositionChanged(int kilometersGereden);

    /// <summary>
    /// GpsSupport is een centraal punt waar andere klassen zich kunnen inschrijven om updates te krijgen over de laatste gereden afstand.
    /// </summary>
    public class GpsSupport
    {
        public static GpsSupport gpsSupport = new GpsSupport();
        private Geolocator geoLocator = new Geolocator(); //Built-in library voor geolocatie
        private int metersGereden = 0;
        private int cycli = 0;
        private uint metersVoorLocatieUpdate = 1;
        private int tolerantieAankomend;
        private int tolerantieDringend;

        public event GeoLocator_PositionChanged LocatieUpdate;

        #region Initialisatie

        public GpsSupport()
        {
            object intTolerantieAankomend = LocalStorage.localStorage.LaadGegevens("tolerantieAankomend");
            object intTolerantieDringend = LocalStorage.localStorage.LaadGegevens("tolerantieDringend");

            if (intTolerantieAankomend == null || int.TryParse(intTolerantieAankomend.ToString(), out tolerantieAankomend))
            {
                tolerantieAankomend = 200;
            }

            if (intTolerantieDringend == null || int.TryParse(intTolerantieDringend.ToString(), out tolerantieDringend))
            {
                tolerantieDringend = 50;
            }
        }

        /// <summary>
        /// Nieuwe gps klasse met zelfgekozen meters voor locatieupdate
        /// </summary>
        public void SetUpdateInterval(uint metersVoorUpdate)
        {
            metersVoorLocatieUpdate = metersVoorUpdate;
            geoLocator.DesiredAccuracyInMeters = metersVoorUpdate;
        }

        /// <summary>
        /// Geolocator object initialiseren en het vooraf ingestelde updateparameters in meters implementeren
        /// </summary>
        public void InitialiseerGeoLocator(uint metersVoorUpdate)
        {
            geoLocator = new Geolocator();
            metersVoorLocatieUpdate = metersVoorUpdate;
            geoLocator.DesiredAccuracyInMeters = metersVoorUpdate;
            geoLocator.PositionChanged += Geolocator_PositionChanged;
        }

        /// <summary>
        /// Wordt opgeroepen wanneer de Geolocator een update doorvoert.
        /// De gereden meters worden hier geincrementeerd met de update ratio.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public virtual void Geolocator_PositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            metersGereden += Convert.ToInt32(metersVoorLocatieUpdate) * 1000;
            if (LocatieUpdate != null)
                LocatieUpdate(metersGereden / 1000);
        }

        #endregion

        #region Toegangsmethoden

        /// <summary>
        /// Geeft het aantal kilometers dat tot nu toe gereden werden terug.
        /// </summary>
        /// <returns>Een int instantie</returns>
        public int GetKilometersGereden()
        {
            return metersGereden / 1000;
        }

        /// <summary>
        /// Veranderd het aantal kilometers dat tot nu toe gereden werd.
        /// </summary>
        /// <returns></returns>
        public void SetKilometersGereden(int kilometersGereden)
        {
            this.metersGereden = kilometersGereden * 1000;
        }

        /// <summary>
        /// Haalt het aantal gereden cycli op
        /// </summary>
        /// <returns></returns>
        public int GetCycli()
        {
            return cycli;
        }

        /// <summary>
        /// Veranderd het aantal gereden cycli
        /// </summary>
        /// <returns></returns>
        public void SetCycli(int cycli)
        {
            this.cycli = cycli;
        }

        /// <summary>
        /// Ophalen van de dringende kilometer tollerantie
        /// </summary>
        public int GetTolerantieDringend()
        {
            return tolerantieDringend;
        }

        /// <summary>
        /// Het wijzigen van de dringende kilometer tollerantie
        /// </summary>
        /// <param name="tollerantie">De in te stellen tollerantie integer</param>
        public void SetTolerantieDringend(int tolerantie)
        {
            this.tolerantieDringend = tolerantie;
        }

        /// <summary>
        /// Ophalen van de aankomende kilometer tollerantie
        /// </summary>
        public int GetTolerantieAankomend()
        {
            return tolerantieAankomend;
        }

        /// <summary>
        /// Wijzigen van de aankomende kilometer tollerantie
        /// </summary>
        /// <param name="tollerantie">De in te stellen tollerantie integer</param>
        public void SetTolerantieAankomend(int tolerantie)
        {
            this.tolerantieAankomend = tolerantie;
        }

        #endregion
    }
}
