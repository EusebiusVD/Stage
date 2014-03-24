using PiXeL_Apps.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace PiXeL_Apps.Common
{
    /// <summary>
    /// Hulp bij het opslaan van instellingen en andere data in AppSettings
    /// </summary>
    class LocalStorage
    {
        public static LocalStorage localStorage = new LocalStorage();
        private ApplicationDataContainer localSettings;
        private ApplicationDataContainer container;

        public LocalStorage()
        {
            localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
        }

        /// <summary>
        /// Slaat bepaalde gegevens op in de AppSettings met een sleutel als referentie
        /// </summary>
        /// <param name="sleutel">Referentie</param>
        /// <param name="waarde">Object met waarde</param>
        public void SlaGegevensOp(string sleutel, object waarde)
        {
            List<string> objecten;
            object waarden;
            try
            {
                objecten = (List<string>)waarde;
                waarden = new ApplicationDataCompositeValue();
                for (int teller = 0; teller < objecten.Count; teller++)
                {
                    ((ApplicationDataCompositeValue)waarden)[teller.ToString()] = objecten[teller].ToString();
                }
            }
            catch (InvalidCastException)
            {
                waarden = waarde.ToString();
            }

            try
            {
                if (!localSettings.Containers.ContainsKey("AppSettings"))
                {
                    container = localSettings.CreateContainer("AppSettings", Windows.Storage.ApplicationDataCreateDisposition.Always);
                }

                if (localSettings.Containers.ContainsKey("AppSettings"))
                {
                    if (localSettings.Containers["AppSettings"].Values.ContainsKey(sleutel))
                        localSettings.Containers["AppSettings"].Values[sleutel] = waarden;
                    else
                        localSettings.Containers["AppSettings"].Values.Add(sleutel, waarden);
                }
            }
            catch (Exception ex)
            {
                paLogging.log.Error(String.Format("Gegevens voor {0} konden niet opgeslaan worden.\n{1}", sleutel, ex.Message));
            }
        }

        /// <summary>
        /// Haalt een bepaald gegeven op uit de AppSettings met de sleutel als referentie
        /// </summary>
        /// <param name="sleutel">Referentie</param>
        /// <returns>Object met waarde of NULL</returns>
        public object LaadGegevens(string sleutel)
        {
            object waarde = null;
            try
            {
                if (localSettings.Containers.ContainsKey("AppSettings"))
                {
                    if (localSettings.Containers["AppSettings"].Values.ContainsKey(sleutel))
                        waarde = localSettings.Containers["AppSettings"].Values[sleutel];
                }
            }
            catch (Exception ex)
            {
                paLogging.log.Error(String.Format("Gegevens voor {0} konden niet opgehaald worden.\n{1}", sleutel, ex.Message));
            }
            return waarde;
        }

        public bool checkKey(string key)
        {
            return localSettings.Containers.ContainsKey(key);
        }
    }
}
