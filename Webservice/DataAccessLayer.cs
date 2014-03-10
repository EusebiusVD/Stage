using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Serialization;


namespace Webservice
{
    public class DataAccessLayer
    {
        private String applicatieNaam = "DAL_sqlite";
        private String projectString = System.Configuration.ConfigurationManager.AppSettings.Get("connectieString"); /*ConfigurationManager.ConnectionStrings["Project_Data"].ConnectionString;*/
        private OleDbConnection sqlConnectie;
        private OleDbDataAdapter sqlAdapter = new OleDbDataAdapter();
        private OleDbCommand sqlCommando;
        private OleDbTransaction sqlTransactie;
        private DataSet dataSet;
        private WordNaarPDF wordToPdf = new WordNaarPDF();
        private ExcelNaarPDF excelToPdf = new ExcelNaarPDF();

        /// <summary>
        /// In deze constructor wordt een nieuwe logging en sqlConnectie aangemaakt
        /// </summary>
        public DataAccessLayer(/*String connectiePad*/)
        {
            //DateTime dt = DateTime.UtcNow;
            //logging.log = new logging(String.Format("{0}{1:dd-MM-yyyy}{2}", ConfigurationManager.AppSettings["LoggingPath"], dt, ".log"));
            projectString = System.Configuration.ConfigurationManager.AppSettings.Get("connectieString");
            sqlConnectie = new OleDbConnection(projectString);
        }

        /// <summary>
        /// In deze methode wordt een nieuwe DataSet, OleDbCommand en OleDbDataAdapter aangemaakt
        /// Ook wordt de databank connectie geopend en de transactie uitgevoerd
        /// </summary>
        /// <param name="sqlQuery">sqlQuery is van het type String, dit is de query die meegegeven wordt</param>
        private void VoerNonQueryUit(string sqlQuery, string[] parameters)
        {
            dataSet = new DataSet();
            sqlCommando = new OleDbCommand(sqlQuery, sqlConnectie);


            sqlAdapter = new OleDbDataAdapter(sqlCommando);
            foreach (string parameter in parameters)
            {
                sqlAdapter.SelectCommand.Parameters.Add(new OleDbParameter("@zoek", parameter));
            }

            sqlConnectie.Open();
            sqlTransactie = sqlConnectie.BeginTransaction();
            sqlCommando.Transaction = sqlTransactie;
            sqlCommando.ExecuteNonQuery();

            sqlAdapter.SelectCommand = sqlCommando;
        }

        /// <summary>
        /// In deze methode wordt de sqlConnectie gesloten en de sqlAdapter verwijderd
        /// Als dit niet lukt dan wordt de exception in de logging weggeschreven
        /// </summary>
        /// <param name="context"></param>
        private void CloseConnection(string context)
        {
            try
            {
                sqlConnectie.Close();
                sqlAdapter.Dispose();
            }
            catch (OleDbException oe)
            {
                Logging.log.WriteLine(String.Format("{0} ({1})", applicatieNaam, context), oe.Message);
                dataSet = null;
            }
        }

        [Serializable]
        [XmlType(TypeName = "SerializableKeyValuePair")]
        public struct KeyValuePair<K, V>
        {
            public K Key
            { get; set; }

            public V Value
            { get; set; }
        }

        public void SetConnectieString(string pad)
        {
            projectString = "Provider=Microsoft.ACE.OLEDB.12.0;&#xD;&#xA;Data Source=" + pad + "Project_Data1.accdb;&#xD;&#xA;Persist Security Info=False;";

            //BRON: http://www.geekzone.co.nz/dmw/1755
            //Extra config bestand, rede = waarden in web.config bijwerken vereist server herstart.
            //waarde in extra bestand bijwerken en cache update is voldoende en heeft geen herstart nodig.
            XmlDocument xmldoc = new XmlDocument();
            string baseDir = System.Web.HttpRuntime.AppDomainAppPath;
            string configPath = Path.Combine(baseDir, "Settings.config");
            xmldoc.Load(Path.Combine(HttpRuntime.AppDomainAppPath, "Settings.config"));
            XmlNode node = xmldoc.SelectSingleNode("appSettings/add[@key = 'connectieString']");
            XmlNode valueAttribute = node.Attributes.GetNamedItem("value");
            if (valueAttribute != null)
            {
                valueAttribute.InnerXml = projectString;
                xmldoc.Save(configPath);
            }

            //update the cached setting
            System.Configuration.ConfigurationManager.AppSettings.Set("connectieString", projectString);
        
        }

        /// <summary>
        /// In deze methode worden de automerken opgehaald uit de Access databank en doorgegeven d.m.v. een lijst
        /// Indien dit niet lukt dan wordt de exception in de logging weggeschreven, de transactie wordt teruggedraaid en afgesloten
        /// </summary>
        /// <returns></returns>
        public List<List<KeyValuePair<string, object>>> GetBrand(int brandId)
        {
            List<List<KeyValuePair<string, object>>> brands = new List<List<KeyValuePair<string, object>>>();
            try
            {
                VoerNonQueryUit("Select * from DSS_BRAND where BRAND_ID = ?", new string[] { brandId.ToString() });
                sqlAdapter.Fill(dataSet);
                List<KeyValuePair<string, object>> brandDictionary;
                foreach (DataRow row in dataSet.Tables[0].Rows)
                {
                    brandDictionary = new List<KeyValuePair<string, object>>();
                    KeyValuePair<string, object> keyValue = new KeyValuePair<string, object>();
                    keyValue.Key = "BRAND_ID";
                    keyValue.Value = row[0];
                    brandDictionary.Add(keyValue);

                    keyValue.Key = "BRAND";
                    keyValue.Value = row[1];
                    brandDictionary.Add(keyValue);

                    keyValue.Key = "TYPE";
                    keyValue.Value = row[2];
                    brandDictionary.Add(keyValue);
                    brands.Add(brandDictionary);
                }

                sqlTransactie.Commit();
            }
            catch (Exception e)
            {
                Logging.log.WriteLine(applicatieNaam, e.Message);
                if (sqlTransactie != null)
                {
                    sqlTransactie.Rollback();
                    sqlTransactie.Dispose();
                    throw (e);
                }
            }
            finally
            {
                CloseConnection("merken");
            }

            return brands;
        }

        /// <summary>
        /// In deze methode worden de defectcodes opgehaald uit de Access databank en doorgegeven d.m.v. een lijst
        /// Indien dit niet lukt dan wordt de exception in de logging weggeschreven, de transactie wordt teruggedraaid en afgesloten
        /// </summary>
        /// <returns></returns>
        public List<List<KeyValuePair<string, object>>> GetDefectcode()
        {
            List<List<KeyValuePair<string, object>>> defectcodes = new List<List<KeyValuePair<string, object>>>();
            try
            {
                VoerNonQueryUit("Select * from DSS_DEFECTCODES", new string[0]);
                sqlAdapter.Fill(dataSet);
                List<KeyValuePair<string, object>> defectcodeDictionary;
                foreach (DataRow row in dataSet.Tables[0].Rows)
                {
                    defectcodeDictionary = new List<KeyValuePair<string, object>>();
                    KeyValuePair<string, object> keyValue = new KeyValuePair<string, object>();
                    keyValue.Key = "DEFECT_ID";
                    keyValue.Value = row[0];
                    defectcodeDictionary.Add(keyValue);

                    keyValue.Key = "CODE";
                    keyValue.Value = row[1];
                    defectcodeDictionary.Add(keyValue);

                    keyValue.Key = "BESCHRIJVING";
                    keyValue.Value = row[2];
                    defectcodeDictionary.Add(keyValue);

                    keyValue.Key = "TYPE_DEFECT";
                    keyValue.Value = row[3];
                    defectcodeDictionary.Add(keyValue);
                    defectcodes.Add(defectcodeDictionary);
                }

                sqlTransactie.Commit();
            }
            catch (Exception e)
            {
                Logging.log.WriteLine(applicatieNaam, e.Message);
                if (sqlTransactie != null)
                {
                    sqlTransactie.Rollback();
                    sqlTransactie.Dispose();
                    throw (e);
                }
            }
            finally
            {
                CloseConnection("defectcodes");
            }

            return defectcodes;
        }

        /// <summary>
        /// In deze methode worden de eenheden opgehaald uit de Access databank en doorgegeven d.m.v. een lijst
        /// Indien dit niet lukt dan wordt de exception in de logging weggeschreven, de transactie wordt teruggedraaid en afgesloten
        /// </summary>
        /// <returns></returns>
        public List<List<KeyValuePair<string, object>>> GetEenheid(int eenheidId)
        {
            List<List<KeyValuePair<string, object>>> eenheden = new List<List<KeyValuePair<string, object>>>();
            try
            {
                VoerNonQueryUit("Select * from DSS_EENHEID where EENHEID_ID = ?", new string[] { eenheidId.ToString() });
                sqlAdapter.Fill(dataSet);
                List<KeyValuePair<string, object>> eenheidDictionary;
                foreach (DataRow row in dataSet.Tables[0].Rows)
                {
                    eenheidDictionary = new List<KeyValuePair<string, object>>();
                    KeyValuePair<string, object> keyValue = new KeyValuePair<string, object>();
                    keyValue.Key = "EENHEID_ID";
                    keyValue.Value = row[0];
                    eenheidDictionary.Add(keyValue);

                    keyValue.Key = "EENHEID";
                    keyValue.Value = row[1];
                    eenheidDictionary.Add(keyValue);
                    eenheden.Add(eenheidDictionary);
                }

                sqlTransactie.Commit();
            }
            catch (Exception e)
            {
                Logging.log.WriteLine(applicatieNaam, e.Message);
                if (sqlTransactie != null)
                {
                    sqlTransactie.Rollback();
                    sqlTransactie.Dispose();
                    throw (e);
                }
            }
            finally
            {
                CloseConnection("eenheden");
            }

            return eenheden;
        }

        /// <summary>
        /// In deze methode worden de motors opgehaald uit de Access databank en doorgegeven d.m.v. een lijst
        /// Indien dit niet lukt dan wordt de exception in de logging weggeschreven, de transactie wordt teruggedraaid en afgesloten
        /// </summary>
        /// <returns></returns>
        public List<List<KeyValuePair<string, object>>> GetEngine(int engineId)
        {
            List<List<KeyValuePair<string, object>>> engines = new List<List<KeyValuePair<string, object>>>();
            try
            {
                VoerNonQueryUit("Select * from DSS_ENGINE where ENGINE_ID = ?", new string[] { engineId.ToString() });
                sqlAdapter.Fill(dataSet);
                List<KeyValuePair<string, object>> engineDictionary;
                foreach (DataRow row in dataSet.Tables[0].Rows)
                {
                    engineDictionary = new List<KeyValuePair<string, object>>();
                    KeyValuePair<string, object> keyValue = new KeyValuePair<string, object>();
                    keyValue.Key = "ENGINE_ID";
                    keyValue.Value = row[0];
                    engineDictionary.Add(keyValue);

                    keyValue.Key = "ENGINE_CODE";
                    keyValue.Value = row[1];
                    engineDictionary.Add(keyValue);

                    keyValue.Key = "CAPACITY";
                    keyValue.Value = row[2];
                    engineDictionary.Add(keyValue);

                    keyValue.Key = "FUEL_ID";
                    keyValue.Value = row[3];
                    engineDictionary.Add(keyValue);
                    engines.Add(engineDictionary);
                }

                sqlTransactie.Commit();
            }
            catch (Exception e)
            {
                Logging.log.WriteLine(applicatieNaam, e.Message);
                if (sqlTransactie != null)
                {
                    sqlTransactie.Rollback();
                    sqlTransactie.Dispose();
                    throw (e);
                }
            }
            finally
            {
                CloseConnection("motoren");
            }

            return engines;
        }

        /// <summary>
        /// In deze methode worden de brandstoffen opgehaald uit de Access databank en doorgegeven d.m.v. een lijst
        /// Indien dit niet lukt dan wordt de exception in de logging weggeschreven, de transactie wordt teruggedraaid en afgesloten
        /// </summary>
        /// <returns></returns>
        public List<List<KeyValuePair<string, object>>> GetFuel(int fuelId)
        {
            List<List<KeyValuePair<string, object>>> fuels = new List<List<KeyValuePair<string, object>>>();
            try
            {
                VoerNonQueryUit("Select * from DSS_FUEL where FUEL_ID = ?", new string[] { fuelId.ToString() });
                sqlAdapter.Fill(dataSet);
                List<KeyValuePair<string, object>> fuelDictionary;
                foreach (DataRow row in dataSet.Tables[0].Rows)
                {
                    fuelDictionary = new List<KeyValuePair<string, object>>();
                    KeyValuePair<string, object> keyValue = new KeyValuePair<string, object>();
                    keyValue.Key = "FUEL_ID";
                    keyValue.Value = row[0];
                    fuelDictionary.Add(keyValue);

                    keyValue.Key = "FUEL";
                    keyValue.Value = row[1];
                    fuelDictionary.Add(keyValue);
                    fuels.Add(fuelDictionary);
                }

                sqlTransactie.Commit();
            }
            catch (Exception e)
            {
                Logging.log.WriteLine(applicatieNaam, e.Message);
                if (sqlTransactie != null)
                {
                    sqlTransactie.Rollback();
                    sqlTransactie.Dispose();
                    throw (e);
                }
            }
            finally
            {
                CloseConnection("brandstoffen");
            }

            return fuels;
        }

        /// <summary>
        /// In deze methode worden de scripts opgehaald uit de Access databank en doorgegeven d.m.v. een lijst
        /// Indien dit niet lukt dan wordt de exception in de logging weggeschreven, de transactie wordt teruggedraaid en afgesloten
        /// </summary>
        /// <param name="vehicle_Id">Vehicle_Id is van het type String, is het wagennummer</param>
        /// <returns></returns>
        public List<List<KeyValuePair<string, object>>> GetScript(string vehicle_Id)
        {
            List<List<KeyValuePair<string, object>>> scripts = new List<List<KeyValuePair<string, object>>>();
            try
            {
                VoerNonQueryUit("Select * from DSS_SCRIPT where VEHICLE_ID = ?", new string[] { vehicle_Id });
                sqlAdapter.Fill(dataSet);
                List<KeyValuePair<string, object>> scriptDictionary;
                foreach (DataRow row in dataSet.Tables[0].Rows)
                {
                    scriptDictionary = new List<KeyValuePair<string, object>>();
                    KeyValuePair<string, object> keyValue = new KeyValuePair<string, object>();
                    keyValue.Key = "SCRIPT_ID";
                    keyValue.Value = row[0];
                    scriptDictionary.Add(keyValue);

                    keyValue.Key = "VEHICLE_ID";
                    keyValue.Value = row[1];
                    scriptDictionary.Add(keyValue);

                    keyValue.Key = "ODO";
                    keyValue.Value = row[2];
                    scriptDictionary.Add(keyValue);

                    keyValue.Key = "CYCLI";
                    keyValue.Value = row[3];
                    scriptDictionary.Add(keyValue);

                    keyValue.Key = "ACTIVITY";
                    keyValue.Value = row[4];
                    scriptDictionary.Add(keyValue);

                    keyValue.Key = "EENHEID";
                    keyValue.Value = row[5];
                    scriptDictionary.Add(keyValue);
                    scripts.Add(scriptDictionary);
                }

                sqlTransactie.Commit();
            }
            catch (Exception e)
            {
                Logging.log.WriteLine(applicatieNaam, e.Message);
                if (sqlTransactie != null)
                {
                    sqlTransactie.Rollback();
                    sqlTransactie.Dispose();
                    throw (e);
                }
            }
            finally
            {
                CloseConnection("scripten");
            }
            return scripts;
        }

        /// <summary>
        /// In deze methode worden de transmissies opgehaald uit de Access databank en doorgegeven d.m.v. een lijst
        /// Indien dit niet lukt dan wordt de exception in de logging weggeschreven, de transactie wordt teruggedraaid en afgesloten
        /// </summary>
        /// <returns></returns>
        public List<List<KeyValuePair<string, object>>> GetTransmission(int transmissionId)
        {
            List<List<KeyValuePair<string, object>>> transmission = new List<List<KeyValuePair<string, object>>>();
            try
            {
                VoerNonQueryUit("Select * from DSS_TRANSMISSION where TRANSMISSION_ID = ?", new string[] { transmissionId.ToString() });
                sqlAdapter.Fill(dataSet);
                List<KeyValuePair<string, object>> transmissionDictionary;
                foreach (DataRow row in dataSet.Tables[0].Rows)
                {
                    transmissionDictionary = new List<KeyValuePair<string, object>>();
                    KeyValuePair<string, object> keyValue = new KeyValuePair<string, object>();
                    keyValue.Key = "TRANSMISSION_ID";
                    keyValue.Value = row[0];
                    transmissionDictionary.Add(keyValue);

                    keyValue.Key = "TRANSMISSION";
                    keyValue.Value = row[1];
                    transmissionDictionary.Add(keyValue);

                    keyValue.Key = "GEARS";
                    keyValue.Value = row[2];
                    transmissionDictionary.Add(keyValue);
                    transmission.Add(transmissionDictionary);
                }

                sqlTransactie.Commit();
            }
            catch (Exception e)
            {
                Logging.log.WriteLine(applicatieNaam, e.Message);
                if (sqlTransactie != null)
                {
                    sqlTransactie.Rollback();
                    sqlTransactie.Dispose();
                    throw (e);
                }
            }
            finally
            {
                CloseConnection("transmissies");
            }
            return transmission;
        }

        /// <summary>
        /// In deze methode worden de gebruikers opgehaald uit de Access databank en doorgegeven d.m.v. een lijst
        /// Indien dit niet lukt dan wordt de exception in de logging weggeschreven, de transactie wordt teruggedraaid en afgesloten
        /// </summary>
        /// <returns></returns>
        public List<List<KeyValuePair<string, object>>> GetUser()
        {
            List<List<KeyValuePair<string, object>>> users = new List<List<KeyValuePair<string, object>>>();
            try
            {
                VoerNonQueryUit("Select * from DSS_USERS", new string[0]);
                sqlAdapter.Fill(dataSet);
                List<KeyValuePair<string, object>> userDictionary;
                foreach (DataRow row in dataSet.Tables[0].Rows)
                {
                    userDictionary = new List<KeyValuePair<string, object>>();
                    KeyValuePair<string, object> keyValue = new KeyValuePair<string, object>();
                    keyValue.Key = "ID";
                    keyValue.Value = row[0];
                    userDictionary.Add(keyValue);

                    keyValue.Key = "USER_NAME";
                    keyValue.Value = row[1];
                    userDictionary.Add(keyValue);

                    keyValue.Key = "PASSWORD";
                    keyValue.Value = row[2];
                    userDictionary.Add(keyValue);

                    keyValue.Key = "ADMIN";
                    keyValue.Value = row[3];
                    userDictionary.Add(keyValue);

                    users.Add(userDictionary); //Aangemaakte userdictionary aan de lijst toevoegen
                }

                sqlTransactie.Commit();
            }
            catch (Exception e)
            {
                Logging.log.WriteLine(applicatieNaam, e.Message);
                if (sqlTransactie != null)
                {
                    sqlTransactie.Rollback();
                    sqlTransactie.Dispose();
                    throw (e);
                }
            }
            finally
            {
                CloseConnection("gebruikers");
            }
            return users;
        }

        /// <summary>
        /// In deze methode worden de autos opgehaald uit de Access databank en doorgegeven d.m.v. een lijst
        /// Indien dit niet lukt dan wordt de exception in de logging weggeschreven, de transactie wordt teruggedraaid en afgesloten
        /// </summary>
        /// <returns></returns>
        public List<List<KeyValuePair<string, object>>> GetVehicles()
        {
            List<List<KeyValuePair<string, object>>> vehicles = new List<List<KeyValuePair<string, object>>>();
            try
            {
                VoerNonQueryUit("Select * from DSS_VEHICLE", new string[0]);
                sqlAdapter.Fill(dataSet);
                List<KeyValuePair<string, object>> vehicleDictionary;
                foreach (DataRow row in dataSet.Tables[0].Rows)
                {
                    vehicleDictionary = new List<KeyValuePair<string, object>>();
                    KeyValuePair<string, object> keyValue = new KeyValuePair<string, object>();
                    keyValue.Key = "VEHICLE_ID";
                    keyValue.Value = row[0];
                    vehicleDictionary.Add(keyValue);

                    keyValue.Key = "VEHICLE_NUMBER";
                    keyValue.Value = row[1];
                    vehicleDictionary.Add(keyValue);

                    keyValue.Key = "BRAND_ID";
                    keyValue.Value = (row[2] == DBNull.Value) ? 0 : row[2];
                    vehicleDictionary.Add(keyValue);

                    keyValue.Key = "ENGINE_ID";
                    keyValue.Value = (row[3] == DBNull.Value) ? 0 : row[3];
                    vehicleDictionary.Add(keyValue);

                    keyValue.Key = "TRANMISSION";
                    keyValue.Value = (row[4] == DBNull.Value) ? 0 : row[4];
                    vehicleDictionary.Add(keyValue);

                    keyValue.Key = "EENHEID_ID";
                    keyValue.Value = (row[5] == DBNull.Value) ? 0 : row[5];
                    vehicleDictionary.Add(keyValue);

                    keyValue.Key = "ACTIVE";
                    keyValue.Value = row[6];
                    vehicleDictionary.Add(keyValue);
                    vehicles.Add(vehicleDictionary);
                }

                sqlTransactie.Commit();
            }
            catch (Exception e)
            {
                Logging.log.WriteLine(applicatieNaam, e.Message);
                if (sqlTransactie != null)
                {
                    sqlTransactie.Rollback();
                    sqlTransactie.Dispose();
                    throw (e);
                }
            }
            finally
            {
                CloseConnection("autos");
            }
            return vehicles;
        }

        /// <summary>
        /// In deze methode worden de objectcodes opgehaald uit de Access databank en doorgegeven d.m.v. een lijst
        /// Indien dit niet lukt dan wordt de exception in de logging weggeschreven, de transactie wordt teruggedraaid en afgesloten
        /// </summary>
        /// <returns></returns>
        public List<List<KeyValuePair<string, object>>> GetObjectCodes()
        {
            List<List<KeyValuePair<string, object>>> objectCodes = new List<List<KeyValuePair<string, object>>>();
            try
            {
                VoerNonQueryUit("Select * from DSS_OBJECTCODES", new string[0]);
                sqlAdapter.Fill(dataSet);
                List<KeyValuePair<string, object>> objectCodesDictionary;
                foreach (DataRow row in dataSet.Tables[0].Rows)
                {
                    objectCodesDictionary = new List<KeyValuePair<string, object>>();
                    KeyValuePair<string, object> keyValue = new KeyValuePair<string, object>();
                    keyValue.Key = "OBJECTCODES_ID";
                    keyValue.Value = row[0];
                    objectCodesDictionary.Add(keyValue);

                    keyValue.Key = "CODE";
                    keyValue.Value = row[1];
                    objectCodesDictionary.Add(keyValue);

                    keyValue.Key = "TYPE";
                    keyValue.Value = row[2];
                    objectCodesDictionary.Add(keyValue);

                    keyValue.Key = "REFERENTIE";
                    keyValue.Value = row[3];
                    objectCodesDictionary.Add(keyValue);

                    keyValue.Key = "BESCHRIJVING";
                    keyValue.Value = row[4];
                    objectCodesDictionary.Add(keyValue);

                    objectCodes.Add(objectCodesDictionary);
                }

                sqlTransactie.Commit();
            }
            catch (Exception e)
            {
                Logging.log.WriteLine(applicatieNaam, e.Message);
                if (sqlTransactie != null)
                {
                    sqlTransactie.Rollback();
                    sqlTransactie.Dispose();
                    throw (e);
                }
            }
            finally
            {
                CloseConnection("objectcodes");
            }
            return objectCodes;
        }

        /// <summary>
        /// In deze methode worden de opmerkingen opgehaald uit de Access databank en doorgegeven d.m.v. een lijst
        /// Indien dit niet lukt dan wordt de exception in de logging weggeschreven, de transactie wordt teruggedraaid en afgesloten
        /// </summary>
        /// <returns></returns>
        public List<List<KeyValuePair<string, object>>> GetComments()
        {
            List<List<KeyValuePair<string, object>>> commentaren = new List<List<KeyValuePair<string, object>>>();
            try
            {
                VoerNonQueryUit("Select * from DSS_TEST_COMMENT", new string[0]);
                sqlAdapter.Fill(dataSet);
                List<KeyValuePair<string, object>> commentarenDictionary;
                foreach (DataRow row in dataSet.Tables[0].Rows)
                {
                    commentarenDictionary = new List<KeyValuePair<string, object>>();
                    KeyValuePair<string, object> keyValue = new KeyValuePair<string, object>();
                    keyValue.Key = "ID";
                    keyValue.Value = row[0];
                    commentarenDictionary.Add(keyValue);

                    keyValue.Key = "VEHICLE_ID";
                    keyValue.Value = row[1];
                    commentarenDictionary.Add(keyValue);

                    keyValue.Key = "DateTime";
                    keyValue.Value = row[2];
                    commentarenDictionary.Add(keyValue);

                    keyValue.Key = "COMMENT";
                    keyValue.Value = row[3];
                    commentarenDictionary.Add(keyValue);

                    keyValue.Key = "LOCATION_AUDIO";
                    keyValue.Value = row[4];
                    commentarenDictionary.Add(keyValue);

                    commentaren.Add(commentarenDictionary);
                }

                sqlTransactie.Commit();
            }
            catch (Exception e)
            {
                Logging.log.WriteLine(applicatieNaam, e.Message);
                if (sqlTransactie != null)
                {
                    sqlTransactie.Rollback();
                    sqlTransactie.Dispose();
                    throw (e);
                }
            }
            finally
            {
                CloseConnection("commentaren");
            }
            return commentaren;
        }

        /// <summary>
        /// Bron : http://forums.asp.net/t/1576851.aspx
        /// Converteert een lijst word bestanden naar pdf bestanden zoals opgeslagen op de server.
        /// De pdf bestanden worden dan een voor een geconverteerd naar een binary (byte) array en in een List<byte[]> geplaatst.
        /// Elke byte[] is een pdf bestand.
        /// </summary>
        /// <returns>List van byte arrays met pdf bestanden</returns>
        public List<byte[]> GetPDFBestanden(string vehicle_id, List<string> pdfNamen)
        {
            //Bron : http://forums.asp.net/t/1576851.aspx
            wordToPdf.ConverteerWordNaarPDF(vehicle_id, pdfNamen);
            excelToPdf.ConverteerExcelNaarPDF(vehicle_id, pdfNamen);

            //Lijst van PDF documenten opvragen en als returnwaarde meegeven.
            DirectoryInfo dirInfo = new DirectoryInfo(HttpContext.Current.Server.MapPath(String.Format("~/bestanden/{0}", vehicle_id)));
            IEnumerable<FileInfo> pdfBestandsInfo = new List<FileInfo>();
            if (dirInfo.Exists)
                pdfBestandsInfo =
                    from bestand in dirInfo.GetFiles("*.pdf")
                    where pdfNamen.Contains(bestand.Name.Replace(bestand.Extension, ""))
                    select bestand;

            List<byte[]> pdfBestandenLijst = new List<byte[]>();
            foreach (FileInfo pdfBestand in pdfBestandsInfo)
            {
                    BinaryReader binReader = new BinaryReader(File.Open(pdfBestand.FullName, FileMode.Open, FileAccess.Read));
                    binReader.BaseStream.Position = 0;
                    byte[] binFile = binReader.ReadBytes(Convert.ToInt32(binReader.BaseStream.Length));
                    binReader.Close();
                    pdfBestandenLijst.Add(binFile);
            }

            return pdfBestandenLijst;
        }

        /// <summary>
        /// Methode die alle bestandsnamen ophaalt van word en excel documenten
        /// </summary>
        /// <returns>Lijst van bestandsnamen als string</returns>
        internal List<string> GetPDFBestandsNamen(string vehicle_id)
        {
            IEnumerable<string> bestandsNamen = new List<string>();
            DirectoryInfo dirInfo = new DirectoryInfo(HttpContext.Current.Server.MapPath(String.Format("~/bestanden/{0}", vehicle_id)));
            if (dirInfo.Exists)
                bestandsNamen =
                    from bestand in dirInfo.GetFiles()
                    where !bestand.Name.Contains("~$") &&
                          (bestand.Name.Contains(".doc") || bestand.Name.Contains(".xls"))
                    select bestand.Name;


            //Geef bestandsnamen terug (Kan lege lijst zijn.
            return bestandsNamen.ToList<string>();
        }

        /// <summary>
        /// Methode die alle wijzigingsdata ophaalt van word en excel documenten
        /// </summary>
        /// <returns></returns>
        public List<string> GetPDFData(string vehicle_id)
        {
            return wordToPdf.HaalBestandsDataOp(vehicle_id).Concat(excelToPdf.HaalBestandsDataOp(vehicle_id)).ToList();
        }

    }
}
