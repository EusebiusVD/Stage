using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Xml.Serialization;

namespace Webservice
{
    /// <summary>
    /// Alle methodes voor het ophalen van gegevens die de applicatie nodig heeft worden hier opgereoepen uit de DataAccessLayer
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    //[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class PixelAppsWebservice : System.Web.Services.WebService
    {
        private static DataAccessLayer dataAccessLayer;

        /// <summary>
        /// In deze constructor wordt gecontroleerd of de dataAccessLayer aangemaakt is.
        /// Als deze niet aanwezig is dan wordt deze aangemaakt
        /// </summary>
        public PixelAppsWebservice(/*String connectiePad*/)
        {
            if (dataAccessLayer == null)
                dataAccessLayer = new DataAccessLayer();
        }

        [WebMethod(Description = "Geeft een lijst met lijsten van keypairs terug. Elke lijst met keypairs omschrijft een MERK object", EnableSession = true)]
        public List<List<DataAccessLayer.KeyValuePair<string, object>>> SQLite_GetBrands(int brandId)
        {
            return dataAccessLayer.GetBrand(brandId);
        }
        [WebMethod(Description = "Geeft een lijst met lijsten van keypairs terug. Elke lijst met keypairs omschrijft een DEFECTCODE object", EnableSession = true)]
        public List<List<DataAccessLayer.KeyValuePair<string, object>>> SQLite_GetDefectcodes()
        {
            return dataAccessLayer.GetDefectcode();
        }
        [WebMethod(Description = "Geeft een lijst met lijsten van keypairs terug. Elke lijst met keypairs omschrijft een EENHEID object", EnableSession = true)]
        public List<List<DataAccessLayer.KeyValuePair<string, object>>> SQLite_GetEenheiden(int eenheidId)
        {
            return dataAccessLayer.GetEenheid(eenheidId);
        }
        [WebMethod(Description = "Geeft een lijst met lijsten van keypairs terug. Elke lijst met keypairs omschrijft een MOTOR object", EnableSession = true)]
        public List<List<DataAccessLayer.KeyValuePair<string, object>>> SQLite_GetEngines(int engineId)
        {
            return dataAccessLayer.GetEngine(engineId);
        }
        [WebMethod(Description = "Geeft een lijst met lijsten van keypairs terug. Elke lijst met keypairs omschrijft een BRANDSTOF object", EnableSession = true)]
        public List<List<DataAccessLayer.KeyValuePair<string, object>>> SQLite_GetFuels(int fuelId)
        {
            return dataAccessLayer.GetFuel(fuelId);
        }
        [WebMethod(Description = "Geeft een lijst met lijsten van keypairs terug. Elke lijst met keypairs omschrijft een SCRIPT object", EnableSession = true)]
        public List<List<DataAccessLayer.KeyValuePair<string, object>>> SQLite_GetScripts(string Vehicle_Id)
        {
            return dataAccessLayer.GetScript(Vehicle_Id);
        }
        [WebMethod(Description = "Geeft een lijst met lijsten van keypairs terug. Elke lijst met keypairs omschrijft een GEBRUIKER object", EnableSession = true)]
        public List<List<DataAccessLayer.KeyValuePair<string, object>>> SQLite_GetUsers()
        {
            return dataAccessLayer.GetUser();
        }
        [WebMethod(Description = "Geeft een lijst met lijsten van keypairs terug. Elke lijst met keypairs omschrijft een AUTO object", EnableSession = true)]
        public List<List<DataAccessLayer.KeyValuePair<string, object>>> SQLite_GetVehicles()
        {
            return dataAccessLayer.GetVehicles();
        }
        [WebMethod(Description = "Geeft een lijst met lijsten van keypairs terug. Elke lijst met keypairs omschrijft een OBJECTCODE object", EnableSession = true)]
        public List<List<DataAccessLayer.KeyValuePair<string, object>>> SQLite_GetObjectCodes()
        {
            return dataAccessLayer.GetObjectCodes();
        }
        [WebMethod(Description = "Geeft een lijst met lijsten van keypairs terug. Elke lijst met keypairs omschrijft een COMMENTAAR object", EnableSession = true)]
        public List<List<DataAccessLayer.KeyValuePair<string, object>>> SQLite_GetComments()
        {
            return dataAccessLayer.GetComments();
        }
        [WebMethod(Description = "Geeft een lijst met lijsten van keypairs terug. Elke lijst met keypairs omschrijft een TRANSMISSIE object", EnableSession = true)]
        public List<List<DataAccessLayer.KeyValuePair<string, object>>> SQLite_GetTransmissions(int transmissionId)
        {
            return dataAccessLayer.GetTransmission(transmissionId);
        }
        [WebMethod(Description = "Maakt een CSV bestand aan op basis van een meegegeven lijst", EnableSession = true)]
        public Boolean MaakCSV(List<String> lijstGegevens, String bestandsnaam)
        {
            return CSVGenerator.SchrijfCSV(lijstGegevens, bestandsnaam);
        }
        [WebMethod(Description = "Maakt een CSV bestand aan op basis van een meegegeven lijst en schrijft het bestand naar een USB medium", EnableSession = true)]
        public Boolean MaakCSVUSB(List<String> lijstGegevens, String bestandsnaam)
        {
            return CSVGenerator.SchrijfCSVUSB(lijstGegevens, bestandsnaam);
        }
        [WebMethod(Description = "Haalt bestanden op van een USB apparaat", EnableSession = true)]
        public Boolean SynchroniseerVanUSB(String wagennummer)
        {
            return LeesUSB.KopieerBestanden(wagennummer);
        }
        [WebMethod(Description = "Geeft een lijst van byte arrays. Elke array is een pdf bestand.", EnableSession = true)]
        public List<byte[]> haalPdfBestandenOp(string vehicle_id, string[] pdfNamen)
        {
            return dataAccessLayer.GetPDFBestanden(vehicle_id, pdfNamen.ToList<string>());
        }
        [WebMethod(Description = "Geeft een lijst van strings. Elke string omschrijft de titel van een gevonden document", EnableSession = true)]
        public List<string> haalPdfNamenOp(string vehicle_id)
        {
            return dataAccessLayer.GetPDFBestandsNamen(vehicle_id);
        }
        [WebMethod(Description = "Geeft een lijst van wijzigingsdata voor de bijlagebestanden. Controleer of een nieuwe versie opgehaald moet worden.", EnableSession = true)]
        public List<string> haalPdfWijzigingsdataOp(string vehicle_id)
        {
            return dataAccessLayer.GetPDFData(vehicle_id);
        }
        [WebMethod(Description = "Geeft de inhoud van een Excel bestand", EnableSession = true)]
        public List<string> LeesExcel()
        {
            return ExcelNaarPDF.GetGewichten();
        }
        [WebMethod(Description = "Veranderd de connectiepad", EnableSession = true)]
        public void ZetConnectieString(String pad)
        {
            dataAccessLayer.SetConnectieString(pad);
        }
    }
}