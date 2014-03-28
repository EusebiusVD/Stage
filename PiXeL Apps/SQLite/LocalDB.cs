using PiXeL_Apps.Classes;
using PiXeL_Apps.Common;
using PiXeL_Apps.Logging;
using PiXeL_Apps.SQLite;
using PiXeL_Apps.SQLite.Tables;
using PiXeL_Apps.WebserviceRef;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Portable;
using Windows.Devices.Usb;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Search;
using Windows.UI.Popups;

namespace PiXeL_Apps
{
    public class LocalDB
    {
        public static LocalDB database = new LocalDB();
        private string path = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "LocalDBSQLite.sqlite");
        private static SQLiteAsyncConnection db;

        private List<Vehicle> autos = new List<Vehicle>();
        private List<Inspectie> inspecties = new List<Inspectie>();
        private List<Script> scripten = new List<Script>();
        private List<string> categoriëën = new List<string>();
        private List<Comment> commentaren = new List<Comment>();
        private static PixelAppsWebserviceSoapClient webService;

        private CompleteAuto completeAuto = null;

        private static int aantalCommentaren;
        private static User gebruiker;

        #region Initialisatie

        /// <summary>
        /// In deze constructor wordt een nieuwe verbinding gemaakt met de LocalDB
        /// </summary>
        public LocalDB()
        {
            if (webService == null)
                webService = new WebserviceRef.PixelAppsWebserviceSoapClient();
            if (db == null)
                db = new SQLiteAsyncConnection(path);
        }

        /// <summary>
        /// Bij het aanmaken van de lokale database wordt de eerder toegewezen auto opgehaald indien deze bestaat.
        /// </summary>
        public async Task HaalToegewezenAutoOp()
        {
            ASSIGNEDVEHICLE auto = new ASSIGNEDVEHICLE();
            try
            {
                var toegewezen = await db.QueryAsync<ASSIGNEDVEHICLE>("SELECT * FROM DSS_ASSIGNEDVEHICLE");
                auto = toegewezen.Any() ? toegewezen.First() : new ASSIGNEDVEHICLE
                {
                    Id = 0,
                    Number = String.Empty,
                    Brand_Id = 0,
                    Engine_Id = 0,
                    Kilometerstand = String.Empty,
                    Oliepeil = String.Empty,
                    Transmission_Id = 0
                };
            }
            catch (SQLiteException)
            {
                paLogging.log.Critical("Bij het ophalen van de toegewezen wagen werd ontdekt dat de database niet correct ogpevuld werd.");
                return;
            }
            catch (Exception ex)
            {
                paLogging.log.Critical(String.Format("Kon de toegewezen wagen niet ophalen.\n{0}", ex.Message));
            }
            completeAuto = new CompleteAuto(auto.Id, auto.Number);
            completeAuto.SetBrand(await GetMerk(auto.Brand_Id));
            completeAuto.SetEngine(await GetMotor(auto.Engine_Id));
            completeAuto.SetFuel(await GetBrandstof(completeAuto.Motor_Brandstof_Id));
            completeAuto.SetTransmissie(await GetTransmissie(auto.Transmission_Id));
        }

        public void SetConnectieString(String pad)
        {
            webService.ZetConnectieStringAsync(pad);
        }

        /// <summary>
        /// Methode om het Id van de toegewezen auto op te halen.
        /// </summary>
        /// <returns>string met auto id</returns>
        public string GetAutoId()
        {
            return completeAuto.Number;
        }

        public async Task<Boolean> ControleerGebruikers()
        {
            try
            {
                var refUsers = await webService.SQLite_GetUsersAsync();
                if(refUsers.Body.SQLite_GetUsersResult.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Alle tabellen worden geïnitializeerd en opgevuld met de nodige gegevens.
        /// Alle gegevens worden van de server gehaald.
        /// </summary>
        public async Task CreateDatabase(bool toegewezenAuto)
        {
            try
            {
                #region Verwijderen en aanmaken van tabellen

                await db.DropTableAsync<USER>(); //Tabel USER verwijderen
                await db.CreateTableAsync<USER>(); //Tabel USER aanmaken

                await db.DropTableAsync<VEHICLE>(); //Tabel VEHICLE verwijderen
                await db.CreateTableAsync<VEHICLE>(); //Tabel VEHICLE aanmaken

                await db.DropTableAsync<ENGINE>(); //Tabel ENGINE verwijderen
                await db.CreateTableAsync<ENGINE>(); //Tabel ENGINE aanmaken

                await db.DropTableAsync<BRAND>(); //Tabel BRAND verwijderen
                await db.CreateTableAsync<BRAND>(); //Tabel BRAND aanmaken

                await db.DropTableAsync<FUEL>(); //Tabel FUEL verwijderen
                await db.CreateTableAsync<FUEL>(); //Tabel FUEL aanmaken

                await db.DropTableAsync<TRANSMISSION>(); //Tabel TRANSMISSION verwijderen
                await db.CreateTableAsync<TRANSMISSION>(); //Tabel TRANSMISSION aanmaken

                await db.DropTableAsync<WEIGHTS>(); //Tabel WEIGHTS verwijderen
                await db.CreateTableAsync<WEIGHTS>(); //Tabel WEIGHTS aanmaken

                await db.DropTableAsync<DEFECT_CODES>(); //Tabel PROBLEM verwijderen
                await db.CreateTableAsync<DEFECT_CODES>(); //Tabel PROBLEM aanmaken

                await db.DropTableAsync<SCRIPT>(); //Tabel SCRIPT verwijderen
                await db.CreateTableAsync<SCRIPT>(); //Tabel SCRIPT aanmaken

                if (!toegewezenAuto)
                {
                    await db.DropTableAsync<INSPECTIE>(); //Tabel INSPECTIE verwijderen
                    await db.CreateTableAsync<INSPECTIE>(); //Tabel INSPECTIE aanmaken

                    await db.DropTableAsync<COMMENT>(); //Tabel COMMENT verwijderen
                    await db.CreateTableAsync<COMMENT>(); //Tabel COMMENT aanmaken
                }

                await db.DropTableAsync<CATEGORY>(); //Tabel CATEGORIE verwijderen
                await db.CreateTableAsync<CATEGORY>(); //Tabel CATEGORIE aanmaken

                await db.DropTableAsync<OBJECT_CODES>(); //Tabel OBJECT_CODES verwijderen
                await db.CreateTableAsync<OBJECT_CODES>(); //Tabel OBJECT_CODES aanmaken

                await db.DropTableAsync<OILSAMPLE>(); //Tabel OILSAMPLE verwijderen
                await db.CreateTableAsync<OILSAMPLE>(); //Tabel OILSAMPLE aanmaken

                #endregion

                //Deze region bevat commentaar, alle andere regions gebruiken het zelfde principe.
                #region Users aanmaken & toevoegen

                //Referentie naar users uit de Webservice halen
                var refUsers = await webService.SQLite_GetUsersAsync();
                try
                {
                    //De body bevat een lijst van lijsten.
                    var userPairs = refUsers.Body.SQLite_GetUsersResult; //Elke lijst in de lijst bevat een KeyValuePair (serializable klasse van de webservice) dat een USER object representeeert.
                    var user = new USER();
                    try
                    {
                        List<USER> users = new List<USER>(); //Zal alle gevonden users bevatten
                        foreach (List<WebserviceRef.SerializableKeyValuePair> userPair in userPairs) //Voor elk KeyValuePair
                        {
                            user = new USER
                            {
                                Id = Convert.ToInt32(userPair[0].Value),
                                Username = userPair[1].Value.ToString(),
                                Password = userPair[2].Value.ToString(),
                                Admin = (bool)userPair[3].Value
                            }; //Een nieuw gebruiker aanmaken van een KeyValuePair
                            users.Add(user); //Gebruiker toevoegen aan de lijst met gebruikers
                        }
                        await db.InsertAllAsync(users); //De lijst asynchroon importeren in de SQLite database
                    }
                    catch (InvalidCastException)
                    {
                        paLogging.log.Error(String.Format("User met Id ({0}) heeft een veld dat geen integer bevat.",
                                                    user.Id + 1));
                    }
                    catch (FormatException)
                    {
                        paLogging.log.Error(String.Format("Een veld van engine {0} kon niet worden geconverteerd naar een integer.",
                                                    user.Id + 1));
                    }
                }
                catch (Exception e)
                {
                    paLogging.log.Error("Er is een fout opgetreden bij het opvullen van de database (tabel problem) : " + e.Message);
                }

                #endregion

                #region Auto gerelateerde opvullingmethodes
                if (toegewezenAuto)
                {
                    await HaalToegewezenAutoOp();

                    #region Brand aanmaken & toevoegen

                    try
                    {
                        var refMerkPairs = (await webService.SQLite_GetBrandsAsync(completeAuto.Merk_Id)).Body.SQLite_GetBrandsResult;
                        if (refMerkPairs.Any())
                        {
                            var merkPair = refMerkPairs.First();
                            Brand merk = new Brand();
                            try
                            {
                                merk = new Brand(Convert.ToInt32(merkPair[0].Value), merkPair[1].Value.ToString(), merkPair[2].Value.ToString());
                                await db.InsertAsync(merk.GetBRAND());
                                completeAuto.SetBrand(merk);
                            }
                            catch (InvalidCastException)
                            {
                                paLogging.log.Error(String.Format("Fuel met id {0} heeft een veld dat geen integer bevat.",
                                                            merk.Id + 1));
                            }
                            catch (FormatException)
                            {
                                paLogging.log.Error(String.Format("Een veld van fuel {0} kon niet worden geconverteerd naar een integer.",
                                                            merk.Id + 1));
                            }
                        }
                        else
                        {
                            completeAuto.SetBrand(new Brand());
                        }
                    }
                    catch (Exception e)
                    {
                        paLogging.log.Error("Er is een fout opgetreden bij het opvullen van de database (tabel problem) : " + e.Message);
                    }

                    #endregion

                    #region Engine aanmaken & toevoegen

                    try
                    {
                        var refEngines = (await webService.SQLite_GetEnginesAsync(completeAuto.Motor_Id)).Body.SQLite_GetEnginesResult;
                        if (refEngines.Any())
                        {
                            var enginePair = refEngines.First();
                            var engine = new Engine();
                            try
                            {
                                engine = new Engine(Convert.ToInt32(enginePair[0].Value), enginePair[1].Value.ToString(), Convert.ToDecimal(enginePair[2].Value.ToString()), Convert.ToInt32(enginePair[3].Value));

                                await db.InsertAsync(engine.GetENGINE());
                            }
                            catch (InvalidCastException)
                            {
                                paLogging.log.Error(String.Format("Engine met id {0} heeft een veld dat geen integer bevat.",
                                                            engine.Id + 1));
                            }
                            catch (FormatException)
                            {
                                paLogging.log.Error(String.Format("Een veld van engine {0} kon niet worden geconverteerd naar een integer.",
                                                            engine.Id + 1));
                            }
                        }
                        else
                        {
                            completeAuto.SetEngine(new Engine());
                        }
                    }
                    catch (Exception e)
                    {
                        paLogging.log.Error("Er is een fout opgetreden bij het opvullen van de database (tabel problem) : " + e.Message);
                    }

                    #endregion

                    #region Fuel aanmaken & toevoegen

                    try
                    {
                        var fuelPairs = (await webService.SQLite_GetFuelsAsync(completeAuto.Motor_Brandstof_Id)).Body.SQLite_GetFuelsResult;
                        if (fuelPairs.Any())
                        {
                            var fuelPair = fuelPairs.First();
                            Fuel brandstof = new Fuel();
                            try
                            {
                                brandstof = new Fuel(Convert.ToInt32(fuelPair[0].Value), fuelPair[1].Value.ToString());
                                await db.InsertAsync(brandstof.GetFUEL());
                                completeAuto.SetFuel(brandstof);
                            }
                            catch (InvalidCastException)
                            {
                                paLogging.log.Error(String.Format("Fuel met id {0} heeft een veld dat geen integer bevat.",
                                                            brandstof.Id + 1));
                            }
                            catch (FormatException)
                            {
                                paLogging.log.Error(String.Format("Een veld van fuel {0} kon niet worden geconverteerd naar een integer.",
                                                            brandstof.Id + 1));
                            }
                        }
                        else
                        {
                            completeAuto.SetFuel(new Fuel());
                        }
                    }
                    catch (Exception e)
                    {
                        paLogging.log.Error("Er is een fout opgetreden bij het opvullen van de database (tabel problem) : " + e.Message);
                    }

                    #endregion

                    #region Transmission aanmaken & toevoegen

                    try
                    {
                        var transmissionPairs = (await webService.SQLite_GetTransmissionsAsync(completeAuto.Transmissie_Id)).Body.SQLite_GetTransmissionsResult;
                        if (transmissionPairs.Any())
                        {
                            var transmissionPair = transmissionPairs.First();
                            Transmission transmissie = new Transmission();
                            try
                            {
                                transmissie = new Transmission(Convert.ToInt32(transmissionPair[0].Value), transmissionPair[2].Value.ToString(), Convert.ToInt32(transmissionPair[1].Value));
                                await db.InsertAsync(transmissie.GetTRANSMISSION());
                                completeAuto.SetTransmissie(transmissie);
                            }
                            catch (InvalidCastException)
                            {
                                paLogging.log.Error(String.Format("Transmissie met id {0} heeft een veld dat geen integer bevat.",
                                                            transmissie.Id + 1));
                            }
                            catch (FormatException)
                            {
                                paLogging.log.Error(String.Format("Een veld van transmissie {0} kon niet worden geconverteerd naar een integer.",
                                                            transmissie.Id + 1));
                            }
                        }
                        else
                        {
                            completeAuto.SetTransmissie(new Transmission());
                        }
                    }
                    catch (Exception e)
                    {
                        paLogging.log.Error("Er is een fout opgetreden bij het opvullen van de database (tabel problem) : " + e.Message);
                    }

                    #endregion
                }
                #endregion

                #region Vehicles aanmaken & toevoegen

                var refVehicles = await webService.SQLite_GetVehiclesAsync();
                try
                {
                    var vehiclePairs = refVehicles.Body.SQLite_GetVehiclesResult;
                    var auto = new VEHICLE();
                    try
                    {
                        List<VEHICLE> autos = new List<VEHICLE>();
                        foreach (List<WebserviceRef.SerializableKeyValuePair> vehiclePair in vehiclePairs)
                        {
                            auto = new VEHICLE
                            {
                                Id = Convert.ToInt32(vehiclePair[0].Value),
                                Number = vehiclePair[1].Value.ToString(),
                                Brand_Id = Convert.ToInt32(vehiclePair[2].Value),
                                Engine_Id = Convert.ToInt32(vehiclePair[3].Value),
                                Transmission_Id = Convert.ToInt32(vehiclePair[4].Value),
                                Eenheid_Id = Convert.ToInt32(vehiclePair[5].Value.ToString()),
                                Active = (bool)vehiclePair[6].Value
                            };
                            autos.Add(auto);
                        }
                        await db.InsertAllAsync(autos);
                    }
                    catch (InvalidCastException)
                    {
                        paLogging.log.Error(String.Format("Vehicle Id ({0}) is geen integer.",
                                                    auto.Id + 1));
                    }
                    catch (FormatException)
                    {
                        paLogging.log.Error(String.Format("Vehicle Id ({0}) kon niet worden geconverteerd naar een integer.",
                                                    auto.Id + 1));
                    }
                }
                catch (Exception e)
                {
                    paLogging.log.Error("Er is een fout opgetreden bij het opvullen van de database (tabel problem) : " + e.Message);
                }

                #endregion

                #region Inspecties aanmaken & toevoegen

                if (!toegewezenAuto)
                {
                    try
                    {
                        var refInspecties = await webService.SQLite_GetScriptsAsync(completeAuto.Id.ToString());
                        var inspectiePairs = refInspecties.Body.SQLite_GetScriptsResult;
                        var inspectie = new Inspectie();
                        try
                        {
                            List<INSPECTIE> inspecties = new List<INSPECTIE>();
                            List<Inspectie> inspectieScherm = new List<Inspectie>();
                            foreach (List<WebserviceRef.SerializableKeyValuePair> inspectiePair in inspectiePairs)
                            {
                                inspectie = new Inspectie
                                {
                                    Id = Convert.ToInt32(inspectiePair[0].Value),
                                    VehicleId = Convert.ToInt32(inspectiePair[1].Value),
                                    Kilometerstand = Convert.ToInt32(inspectiePair[2].Value),
                                    Cycli = inspectiePair[3].Value.ToString(),
                                    Activity = inspectiePair[4].Value.ToString(),
                                    Eenheid = Convert.ToInt32(inspectiePair[5].Value),
                                    Status = false
                                };
                                inspecties.Add(inspectie.GetINSPECTIE());
                                inspectieScherm.Add(inspectie);
                            }
                            await db.InsertAllAsync(inspecties);
                            Inspecties.HaalInspectiesOp(inspectieScherm);
                        }
                        catch (InvalidCastException)
                        {
                            paLogging.log.Error(String.Format("Script met id {0} heeft een veld dat geen integer bevat.",
                                                        inspectie.Id + 1));
                        }
                        catch (FormatException)
                        {
                            paLogging.log.Error(String.Format("Een veld van script {0} kon niet worden geconverteerd naar een integer.",
                                                        inspectie.Id + 1));
                        }
                    }
                    catch (NullReferenceException)
                    {
                        paLogging.log.Warn("De database werd opgevuld hoewel er geen wagen toegewezen is. Er zijn geen inspecties opgehaald.");
                    }
                    catch (Exception e)
                    {
                        paLogging.log.Error("Er is een fout opgetreden bij het opvullen van de database (tabel inspecties) : " + e.Message + " " + e.StackTrace);
                    }
                }
                else
                {
                    paLogging.log.Warn("De database werd opgevuld hoewel er geen wagen toegewezen is. Er zijn geen inspecties opgehaald.");
                }
                #endregion

                #region Defectcodes aanmaken & toevoegen

                var refDefectCodes = await webService.SQLite_GetDefectcodesAsync();
                try
                {
                    var defectPairs = refDefectCodes.Body.SQLite_GetDefectcodesResult;
                    var defectcode = new DEFECT_CODES();
                    try
                    {
                        List<DEFECT_CODES> defectcodes = new List<DEFECT_CODES>();
                        foreach (List<WebserviceRef.SerializableKeyValuePair> defectPair in defectPairs)
                        {
                            defectcode = new DEFECT_CODES
                            {
                                Id = Convert.ToInt32(defectPair[0].Value),
                                Code = defectPair[1].Value.ToString(),
                                Beschrijving = defectPair[2].Value.ToString(),
                                Type_Defect = defectPair[3].Value.ToString()
                            };
                            defectcodes.Add(defectcode);
                        }
                        await db.InsertAllAsync(defectcodes);
                    }
                    catch (InvalidCastException)
                    {
                        paLogging.log.Error(String.Format("Defectcode met id {0} heeft een veld dat geen integer bevat.",
                                                    defectcode.Id + 1));
                    }
                    catch (FormatException)
                    {
                        paLogging.log.Error(String.Format("Een veld van defectcode {0} kon niet worden geconverteerd naar een integer.",
                                                    defectcode.Id + 1));
                    }
                }
                catch (Exception e)
                {
                    paLogging.log.Error("Er is een fout opgetreden bij het opvullen van de database (tabel defect codes) : " + e.Message);
                }

                #endregion

                #region Objectcodes aanmaken & toevoegen

                try
                {
                    var objectPairs = (await webService.SQLite_GetObjectCodesAsync()).Body.SQLite_GetObjectCodesResult;
                    var objectcode = new ObjectCodes();
                    try
                    {
                        List<OBJECT_CODES> objectcodes = new List<OBJECT_CODES>();
                        List<ObjectCodes> doorTeGevenCodes = new List<ObjectCodes>();
                        foreach (List<WebserviceRef.SerializableKeyValuePair> objectPair in objectPairs)
                        {
                            objectcode = new ObjectCodes(
                                Convert.ToInt32(objectPair[0].Value), objectPair[1].Value.ToString(), objectPair[2].Value.ToString(), objectPair[3].Value.ToString(), objectPair[4].Value.ToString());
                            objectcodes.Add(objectcode.GetOBJECTCODES());
                            doorTeGevenCodes.Add(objectcode);
                        }
                        await db.InsertAllAsync(objectcodes);
                    }
                    catch (InvalidCastException)
                    {
                        paLogging.log.Error(String.Format("Object code met id {0} heeft een veld dat geen integer bevat.",
                                                    objectcode.Id + 1));
                    }
                    catch (FormatException)
                    {
                        paLogging.log.Error(String.Format("Een veld van object code {0} kon niet worden geconverteerd naar een integer.",
                                                    objectcode.Id + 1));
                    }
                }
                catch (Exception e)
                {
                    paLogging.log.Error("Er is een fout opgetreden bij het opvullen van de database (tabel object codes) : " + e.Message);
                }

                #endregion

                #region Categorie aanmaken & toevoegen
                var c1 = new CATEGORY
                {
                    Id = 1,
                    ProblemCategory = "Motor",
                    Description = "Problemen ivm de motor"
                };
                var c2 = new CATEGORY
                {
                    Id = 2,
                    ProblemCategory = "Remfunctie",
                    Description = "Problemen ivm de remfunctie"
                };
                var c3 = new CATEGORY
                {
                    Id = 3,
                    ProblemCategory = "Lichten",
                    Description = "Problemen met de lichten"
                };
                await db.InsertAsync(c1);
                await db.InsertAsync(c2);
                await db.InsertAsync(c3);
                #endregion

                #region Comments aanmaken & toevoegen

                /*var refComments = await webService.SQLite_GetCommentsAsync();
                try
                {
                    var commentPairs = refComments.Body.SQLite_GetCommentsResult;
                    var commentaar = new COMMENT();
                    try
                    {
                        List<COMMENT> commentaren = new List<COMMENT>();
                        foreach (List<WebserviceRef.SerializableKeyValuePair> commentPair in commentPairs)
                        {
                            commentaar = new COMMENT
                            {
                                Id = Convert.ToInt32(commentPair[0].Value),
                                DefectCodeId = Convert.ToInt32(commentPair[1].Value),
                                Omschrijving = commentPair[2].Value.ToString()
                            };
                            commentaren.Add(commentaar);
                        }
                        await db.InsertAllAsync(commentaren);
                    }
                    catch (InvalidCastException)
                    {
                        paLogging.log.Error(String.Format("Commentaar met id {0} heeft een veld dat geen integer bevat.",
                                                    commentaar.Id + 1));
                    }
                    catch (FormatException)
                    {
                        paLogging.log.Error(String.Format("Een veld van commentaar {0} kon niet worden geconverteerd naar een integer.",
                                                    commentaar.Id + 1));
                    }
                }
                catch (Exception e)
                {
                    paLogging.log.Error("Er is een fout opgetreden bij het opvullen van de database (tabel object codes) : " + e.Message);
                }*/
                #endregion
            }
            catch (Exception e)
            {
                paLogging.log.Error("Fout bij het lezen of schrijven van de database\nFoutmelding: " + e.Message);
            }
        }

        #endregion

        #region User-gerelateerd

        /// <summary>
        /// Haalt een specifieke gebruiker op gebaseerd op zijn gebruikersnaam en paswoord.
        /// </summary>
        /// <param name="username">Ingegeven gebruikersnaam van het type String </param>
        /// <param name="password">Ingegeven paswoord van het type String</param>
        /// <returns>Specifieke gebruiker van het type User</returns>
        public async Task<User> GetUser(String username, String password)
        {
            try
            {
                //SELECT-query met SQL-Injection prevention (?)
                var user = await db.QueryAsync<USER>("SELECT * FROM DSS_USER WHERE Username=? AND Password=?", new object[] { username, password });
                int count = user.Any() ? user.Count : 0; //Als er een user is (.Any), aantal geven, anders 0 als aantal nemen <- verkorte if-structuur

                if (count > 0)
                {
                    gebruiker = new User(user[0].Id, username, password, user[0].Admin);
                    return gebruiker;
                }
            }
            catch (Exception e)
            {
                paLogging.log.Error("Fout bij het ophalen van gebruikers uit de database\nFoutmelding: " + e.Message);
            }

            return null;
        }

        /// <summary>
        /// Haalt de ingelogde gebruiker op
        /// </summary>
        /// <returns>gebruiker van het type User</returns>
        public User GetIngelogdeGebruiker()
        {
            return gebruiker;
        }

        #endregion

        #region Inspectie-gerelateerd

        /// <summary>
        /// Haalt alle inspecties op uit de database
        /// </summary>
        /// <returns>Een lijst van Inspectie instanties</returns>
        public async Task<List<Inspectie>> GetInspecties()
        {
            if (inspecties.Count > 0)
                return inspecties;
            else
            {
                try
                {
                    var refInspecties = await db.QueryAsync<INSPECTIE>(String.Format("SELECT * FROM DSS_INSPECTIE WHERE Vehicle_Id = {0}", completeAuto.Id));
                    if (refInspecties.Any())
                    {
                        for (int counter = 0; counter < refInspecties.Count; counter++)
                        {
                            inspecties.Add(new Inspectie(refInspecties[counter].Id, refInspecties[counter].Inspectie_Id, refInspecties[counter].Vehicle_Id, refInspecties[counter].Kilometerstand,
                                refInspecties[counter].Cycli, refInspecties[counter].Activity, refInspecties[counter].Eenheid, refInspecties[counter].Status));
                        }
                        return inspecties;
                    }
                }
                catch (Exception e)
                {
                    paLogging.log.Error("Fout bij het ophalen van testen uit de database\nFoutmelding: " + e.Message);
                }
                return null;
            }
        }

        /// <summary>
        /// Haalt een lijst van inspecties op gebaseerd op het aantal kilometers dat deze verwijderd zijn
        /// van de opgegevens kilometers met de opgegeven marge.
        /// </summary>
        /// <param name="kilometers"></param>
        /// <param name="marge"></param>
        /// <returns>Een lijst van Inspectie instanties</returns>
        public async Task<List<Inspectie>> GetInspectiesBijKilometers(int kilometers, int marge)
        {
            try
            {
                if (inspecties.Count == 0)
                {
                    await GetInspecties();
                }
                IEnumerable<Inspectie> inspectieQuery =
                from inspectie in inspecties
                where inspectie.Kilometerstand > kilometers && inspectie.Kilometerstand < kilometers + marge
                select inspectie;

                return inspectieQuery.ToList<Inspectie>();
            }
            catch (Exception e)
            {
                paLogging.log.Error(String.Format("Fout bij het ophalen van inspecties met kilometerstand ca. {0} uit de database\nFoutmelding: {1} ", kilometers, e.Message));
            }

            return new List<Inspectie>();
        }

        /// <summary>
        /// Een lijst van inspecties wordt weggeschreven naar de SQLite database.
        /// Dit gaat over een lijst die reeds bestaat, elke inspectie wordt bijgewerkt met de nieuwste gegevens.
        /// </summary>
        /// <param name="updateInspecties"></param>
        /// <returns></returns>
        public async Task UpdateInspecties(List<Inspectie> updateInspecties, List<Inspectie> nieuweInspecties)
        {
            inspecties = nieuweInspecties;
            foreach (Inspectie inspectie in updateInspecties)
            {
                var updateInspectie = new INSPECTIE
                {
                    Id = inspectie.Id,
                    Vehicle_Id = inspectie.VehicleId,
                    Kilometerstand = inspectie.Kilometerstand,
                    Cycli = inspectie.Cycli,
                    Activity = inspectie.Activity,
                    Eenheid = inspectie.Eenheid,
                    Status = inspectie.Status
                };
                await db.UpdateAsync(updateInspectie);
            }
        }

        #endregion

        #region Script-gerelateerd

        /// <summary>
        /// Haalt een specifiek script op gebaseerd op het Id
        /// </summary>
        /// <param name="scriptId">Te selecteren script Id</param>
        /// <returns>Een Script instantie</returns>
        public async Task<Script> GetScript(int scriptId)
        {
            try
            {
                if (scripten.Count == 0)
                {
                    await GetScripts();
                }
                return scripten[scriptId];
            }
            catch (Exception e)
            {
                paLogging.log.Error("Fout bij het ophalen van script " + scriptId + " uit de database\nFoutmelding: " + e.Message);
            }

            return null;
        }

        /// <summary>
        /// Haalt alle scripten op uit de database
        /// </summary>
        /// <returns>Een lijst van Script instanties</returns>
        public async Task<List<Script>> GetScripts()
        {
            if (scripten.Count > 0)
                return scripten;
            else
            {
                try
                {
                    var refScripten = await db.QueryAsync<SCRIPT>("SELECT * FROM DSS_SCRIPT");
                    if (refScripten.Any())
                    {
                        for (int counter = 0; counter < refScripten.Count; counter++)
                        {
                            scripten.Add(new Script(refScripten[counter].Id, refScripten[counter].Vehicle_Id,
                                refScripten[counter].Kilometerstand, refScripten[counter].Cycli, refScripten[counter].Activity,
                                refScripten[counter].Eenheid, refScripten[counter].Status));
                        }
                        return scripten;
                    }

                    return null;
                }
                catch (Exception e)
                {
                    paLogging.log.Error("Fout bij het ophalen van testen uit de database\nFoutmelding: " + e.Message);
                }
                return null;
            }
        }

        #endregion

        #region Auto-gerelateerd

        /// <summary>
        /// Haalt een bepaalde auto op door een LINQ query uit te voeren op de bestaande lijst met autos.
        /// De lijst wordt aangemaakt als deze nog niet bestaat.
        /// </summary>
        /// <param name="number"></param>
        /// <returns>Een Vehicle instantie</returns>
        public async Task<Vehicle> GetAuto(string number)
        {   //Als de autos lijst nog niet is aangemaakt dan roepen we de GetAutos methode op.
            if (autos == null)
                await GetAutos(true);

            //LINQ query om een auto in de autos lijst de selecteren
            IEnumerable<Vehicle> vehicleQuery =
                from vehicle in autos
                where vehicle.Number.Equals(number)
                select vehicle;

            return vehicleQuery.First();
        }

        /// <summary>
        /// Haalt alle auto's op uit de lokale SQLite database.
        /// Als de auto's eerder opgehaald werden wordt een opgeslagen lijst terug gegeven.
        /// </summary>
        /// <param name="verversen"></param>
        /// <returns>Een lijst van Vehicle instanties</returns>
        public async Task<List<Vehicle>> GetAutos(bool verversen)
        {
            try
            {
                if (verversen)
                {
                    autos = new List<Vehicle>();
                    var vehicles = await db.QueryAsync<VEHICLE>("SELECT * FROM DSS_VEHICLE");
                    if (vehicles.Any())
                    {
                        for (int i = 0; i < vehicles.Count; i++)
                        {
                            autos.Add(new Vehicle(vehicles[i].Id, vehicles[i].Number, vehicles[i].Brand_Id, vehicles[i].Engine_Id, vehicles[i].Transmission_Id, vehicles[i].Eenheid_Id, vehicles[i].Active));
                        }
                        return autos;
                    }
                }
                else
                {
                    if (autos.Count > 0)
                        return autos;
                    else
                        return await GetAutos(true);
                }

            }
            catch (Exception e)
            {
                paLogging.log.Error("Fout bij het ophalen van de voertuigen uit de database\nFoutmelding: " + e.Message);
            }
            return null;
        }

        /// <summary>
        /// Haalt de toegewezen auto op. Dit is een complete instantie met alle gebundelde gegevens over
        /// de huidige toegewezen auto. Als deze niet bestaat wordt een leeg (niet null) object van de complete auto
        /// terug gegeven.
        /// </summary>
        /// <returns>Een CompleteAuto instantie</returns>
        public async Task<CompleteAuto> GetToegewezenAuto()
        {
            if (completeAuto == null)
            {
                var auto = await db.QueryAsync<ASSIGNEDVEHICLE>("SELECT * FROM DSS_ASSIGNEDVEHICLE");
                if (auto.Count > 0)
                {
                    completeAuto = new CompleteAuto(auto[0].Id, auto[0].Number, auto[0].Oliepeil);
                    completeAuto.SetBrand(await GetMerk(auto[0].Brand_Id));
                    completeAuto.SetEngine(await GetMotor(auto[0].Engine_Id));
                    completeAuto.SetFuel(await GetBrandstof(completeAuto.Motor_Brandstof_Id));
                    completeAuto.SetTransmissie(await GetTransmissie(auto[0].Transmission_Id));
                }
            }
            return completeAuto;
        }

        /// <summary>
        ///Haalt alle mogelijke gewichten op uit de lokale databank.
        /// </summary>
        /// <returns>Een lijst van Weights instanties</returns>
        public async Task<List<Weights>> GetGewichten()
        {
            List<Weights> gewichten = new List<Weights>();
            return gewichten;
        }
        /// <summary>
        /// Het ophalen van de gewichten en bandenspanning uit excel document
        /// </summary>
        /// <returns> lijst van gewichten en bandenspanning</returns>
        public async Task<List<string>> GetGewichtenExcel()
        {
            try
            {
                return (await webService.LeesExcelAsync()).Body.LeesExcelResult.ToList();
            }
            catch (Exception e)
            {
                paLogging.log.Error("Er is een fout opgetreden bij het ophalen van de gewichten uit het excel document: " + e.Message);
            }
            return null;
        }

        /// <summary>
        /// Vult de database met de gekozen toegewezen auto uit de autolijst op dmv een LINQ query en het nummer van de auto.
        /// </summary>
        /// <param name="number"></param>
        /// <returns>String resultaat</returns>
        public async Task<String> SetToegewezenAuto(string number)
        {
            try
            {
                await db.DropTableAsync<ASSIGNEDVEHICLE>(); //Tabel ASSIGNEDVEHICLE verwijderen
                await db.CreateTableAsync<ASSIGNEDVEHICLE>(); //Tabel ASSIGNEDVEHICLE aanmaken

                ASSIGNEDVEHICLE auto = (await GetAuto(number)).GetASSIGNEDVEHICLE();
                completeAuto = new CompleteAuto(auto.Id, auto.Number, auto.Oliepeil);
                completeAuto.SetBrand(await GetMerk(auto.Brand_Id));
                completeAuto.SetEngine(await GetMotor(auto.Engine_Id));
                completeAuto.SetFuel(await GetBrandstof(completeAuto.Motor_Brandstof_Id));
                completeAuto.SetTransmissie(await GetTransmissie(auto.Transmission_Id));
                await db.InsertAsync(auto);

                return "Tablet is met succes toegewezen aan de auto";

            }
            catch (Exception e)
            {
                paLogging.log.Error("Fout bij het ophalen van de voertuigen uit de database\nFoutmelding: " + e.Message);
                return "Er is iets fout gelopen. Probeer later opnieuw...";
            }
        }

        /// <summary>
        /// Het toevoegen van het oliepeil en kilometerstand aan tabel DSS_ASSIGNEDVEHICLE
        /// Geeft false terug als het aanmaken mislukt is
        /// </summary>
        /// <param name="oliepeil">oliepeil van het type String</param>
        /// <param name="kilometerstand">kilometersand van het type String</param>
        /// <returns>Een boolean</returns>
        public async Task<bool> SetkilometerstandEnOliepeil(String oliepeil, String kilometerstand)
        {
            try
            {

                var auto = await db.QueryAsync<ASSIGNEDVEHICLE>("SELECT * FROM DSS_ASSIGNEDVEHICLE");
                ASSIGNEDVEHICLE updateKilometer = new ASSIGNEDVEHICLE
                {
                    Id = auto[0].Id,
                    Number = auto[0].Number,
                    Brand_Id = auto[0].Brand_Id,
                    Engine_Id = auto[0].Engine_Id,
                    Transmission_Id = auto[0].Transmission_Id,
                    Oliepeil = oliepeil,
                    Kilometerstand = kilometerstand,
                };
                await db.UpdateAsync(updateKilometer);
                return true;

            }
            catch (Exception e)
            {
                paLogging.log.Error(String.Format("Fout bij toevoegen van de kilometerstand en oliepeil", e.Message));
                return false;
            }
        }

        /// <summary>
        /// Het veranderen van het oliepeil van een auto
        /// Geeft false terug als het wijzigen mislukt is 
        /// </summary>
        /// <param name="oliepeil">oliepeil van het type String</param>
        /// <returns>Een boolean</returns>
        public async Task<bool> UpdateOliepeil(String oliepeil)
        {
            try
            {
                var auto = await db.QueryAsync<ASSIGNEDVEHICLE>("SELECT * FROM DSS_ASSIGNEDVEHICLE");
                ASSIGNEDVEHICLE updateKilometer = new ASSIGNEDVEHICLE
                {
                    Id = auto[0].Id,
                    Number = auto[0].Number,
                    Brand_Id = auto[0].Brand_Id,
                    Engine_Id = auto[0].Engine_Id,
                    Transmission_Id = auto[0].Transmission_Id,
                    Kilometerstand = auto[0].Kilometerstand,
                    Oliepeil = oliepeil
                };
                await db.UpdateAsync(updateKilometer);
                return true;
            }
            catch (Exception e)
            {
                paLogging.log.Error(String.Format("Fout bij het wijzigen van oliepeil", e.Message));
                return false;
            }
        }

        /// <summary>
        /// Set the odometer in the table DSS_ASSIGNEDVEHICLE
        /// </summary>
        /// <param name="Kilometerstand"></param>
        /// <returns></returns>
        public async Task<bool> Setkilometer(String Kilometerstand)
        {
            try
            {

                var auto = await db.QueryAsync<ASSIGNEDVEHICLE>("SELECT * FROM DSS_ASSIGNEDVEHICLE");
                ASSIGNEDVEHICLE updateKilometer = new ASSIGNEDVEHICLE
                {
                    Id = auto[0].Id,
                    Number = auto[0].Number,
                    Brand_Id = auto[0].Brand_Id,
                    Engine_Id = auto[0].Engine_Id,
                    Transmission_Id = auto[0].Transmission_Id,
                    Kilometerstand = Kilometerstand,
                    Oliepeil = auto[0].Oliepeil
                };
                await db.UpdateAsync(updateKilometer);
                return true;
            }
            catch (Exception e)
            {
                paLogging.log.Error(String.Format("Fout bij het wijzigen van de kilometerstand", e.Message));
                return false;
            }
        }

        public async Task<ASSIGNEDVEHICLE> getOilLevel()
        {
            var auto = await db.QueryAsync<ASSIGNEDVEHICLE>("Select * from DSS_ASSIGNEDVEHICLE");
            ASSIGNEDVEHICLE test = new ASSIGNEDVEHICLE
            {
                Id = auto[0].Id,
                Number = auto[0].Number,
                Brand_Id = auto[0].Brand_Id,
                Engine_Id = auto[0].Engine_Id,
                Transmission_Id = auto[0].Transmission_Id,
                Kilometerstand = auto[0].Kilometerstand,
                Oliepeil = auto[0].Oliepeil
            };
            return test;
        }

        /// <summary>
        /// Deze methode zorgt ervoor dat de gewichten van een wagen veranderd kunnen worden 
        /// </summary>
        /// <param name="gewichten">Lijst van Weights</param>
        public async void SetGewichten(List<Weights> gewichten)
        {
            await db.DropTableAsync<WEIGHTS>(); //Tabel WEIGHTS verwijderen
            await db.CreateTableAsync<WEIGHTS>(); //Tabel WEIGHTS aanmaken

            var w1 = new WEIGHTS
            {
                Id = gewichten[0].Id,
                Weight = gewichten[0].Weight
            };

            var w2 = new WEIGHTS
            {
                Id = gewichten[1].Id,
                Weight = gewichten[1].Weight
            };

            var w3 = new WEIGHTS
            {
                Id = gewichten[2].Id,
                Weight = gewichten[2].Weight
            };

            var w4 = new WEIGHTS
            {
                Id = gewichten[3].Id,
                Weight = gewichten[3].Weight
            };

            await db.InsertAsync(w1);
            await db.InsertAsync(w2);
            await db.InsertAsync(w3);
            await db.InsertAsync(w4);

            if (gewichten.Count == 5)
            {
                var w5 = new WEIGHTS
                {
                    Id = gewichten[4].Id,
                    Weight = gewichten[4].Weight
                };
                await db.InsertAsync(w5);
            }
        }

        /// <summary>
        /// Haalt een bepaalde merk op uit de lokale database aan de hand van het Id.
        /// Geeft een lege merk met standaardwaarden terug als de overeenstemmende merk niet gevonden werd.
        /// </summary>
        /// <param name="transmissieId"></param>
        /// <returns>Brand instantie</returns>
        public async Task<Brand> GetMerk(int merkId)
        {
            try
            {
                var merken = await db.QueryAsync<BRAND>(String.Format("SELECT * FROM DSS_BRAND WHERE Id={0}", merkId));
                if (merken.Any())
                    return new Brand(merken[0].Id, merken[0].BrandName, merken[0].Type);
                else
                    return new Brand(0, "Onbekend", "Onbekend");
            }
            catch (Exception e)
            {
                paLogging.log.Error(String.Format("Fout bij het ophalen van merk {0} uit de SQLite database - Foutmelding: {1}.", merkId, e.Message));
            }
            return null;
        }

        /// <summary>
        /// Haalt een bepaalde motor op uit de lokale database aan de hand van het Id.
        /// Geeft een lege motor met standaardwaarden terug als de overeenstemmende motor niet gevonden werd.
        /// </summary>
        /// <param name="motorId">Gezochte motor van het type int</param>
        /// <returns>Een Engine instantie</returns>
        public async Task<Engine> GetMotor(int motorId)
        {
            try
            {
                var motoren = await db.QueryAsync<ENGINE>(String.Format("SELECT * FROM DSS_ENGINE WHERE Id={0}", motorId));
                if (motoren.Any())
                    return new Engine(motoren[0].Id, motoren[0].Engine_Code, Convert.ToDecimal(motoren[0].Capacity), motoren[0].Fuel_Id);
                else
                    return new Engine(0, "Onbekend", 0, 0);
            }
            catch (Exception e)
            {
                paLogging.log.Error(String.Format("Fout bij het ophalen van motor {0} uit de SQLite database - Foutmelding: {1}.", motorId, e.Message));
            }
            return null;
        }

        /// <summary>
        /// Haalt een bepaalde transmissie op uit de lokale database aan de hand van het Id.
        /// Geeft een lege transmissie met standaardwaarden terug als de overeenstemmende transmissie niet gevonden werd.
        /// </summary>
        /// <param name="transmissieId"></param>
        /// <returns>Transmission instantie</returns>
        public async Task<Transmission> GetTransmissie(int transmissieId)
        {
            try
            {
                var transmissies = await db.QueryAsync<TRANSMISSION>(String.Format("SELECT * FROM DSS_TRANSMISSION WHERE Id={0}", transmissieId));
                if (transmissies.Any())
                    return new Transmission(transmissies[0].Id, transmissies[0].Transmission_Name, transmissies[0].Gears);
                else
                    return new Transmission(0, "Onbekend", 0);
            }
            catch (Exception e)
            {
                paLogging.log.Error(String.Format("Fout bij het ophalen van transmissie {0} uit de database - Foutmelding: {1}.", transmissieId, e.Message));
            }
            return null;
        }

        /// <summary>
        /// Haalt een bepaalde brandstof op uit de lokale database aan de hand van het Id.
        /// Geeft een lege brandstof met standaardwaarden terug als de overeenstemmende brandstof niet gevonden werd.
        /// </summary>
        /// <param name="brandstofId"></param>
        /// <returns>Een Fuel instantie</returns>
        public async Task<Fuel> GetBrandstof(int brandstofId)
        {
            try
            {
                var brandstoffen = await db.QueryAsync<FUEL>(String.Format("SELECT * FROM DSS_FUEL WHERE Id={0}", brandstofId));
                if (brandstoffen.Any())
                    return new Fuel(brandstoffen[0].Id, brandstoffen[0].FuelName);
                else
                    return new Fuel(0, "Onbekend");
            }
            catch (Exception e)
            {
                paLogging.log.Error(String.Format("Fout bij het ophalen van brandstof {0} uit de database - Foutmelding: {1}.", brandstofId, e.Message));
            }
            return null;
        }

        #endregion

        #region Defect- en objectcode gerelateerd

        /// <summary>
        /// Haalt alle probleemcodes op die aanwezig zijn in de locale database
        /// </summary>
        /// <returns>Een lijst van DefectCodes instanties</returns>
        public async Task<List<DefectCodes>> GetDefectCodes()
        {
            List<DefectCodes> lijstDefectCodes = new List<DefectCodes>();
            try
            {
                var refDefectCodes = await db.QueryAsync<DEFECT_CODES>("SELECT *  FROM DSS_DEFECT_CODES");
                if (refDefectCodes.Any())
                {
                    foreach (DEFECT_CODES defectCode in refDefectCodes)
                    {
                        lijstDefectCodes.Add(new DefectCodes(defectCode.Id, defectCode.Code, defectCode.Beschrijving, defectCode.Type_Defect));
                    }
                }
            }
            catch (Exception e)
            {
                paLogging.log.Error("Fout bij het ophalen van de problemen uit de database\nFoutmelding: " + e.Message);
            }
            return lijstDefectCodes;
        }

        /// <summary>
        /// Haalt alle objectcodes op die aanwezig zijn in de locale database
        /// </summary>
        /// <returns>List van ObjectCodes</returns>
        public async Task<List<ObjectCodes>> GetObjectCodes()
        {
            List<ObjectCodes> lijstObjectCodes = new List<ObjectCodes>();
            try
            {

                var refObjectCodes = await db.QueryAsync<OBJECT_CODES>("SELECT * FROM DSS_OBJECT_CODES");
                if (refObjectCodes.Any())
                {
                    foreach (OBJECT_CODES objectCode in refObjectCodes)
                    {
                        lijstObjectCodes.Add(new ObjectCodes(objectCode.Id, objectCode.Code, objectCode.Type, objectCode.Referentie, objectCode.Beschrijving));
                    }
                }
            }
            catch (Exception e)
            {
                paLogging.log.Error("Fout bij het ophalen van de objectcodes uit de database\nFoutmelding: " + e.Message);
            }
            return lijstObjectCodes;
        }

        /// <summary>
        /// Deze methode maakt een lijst van strings met alle categorieën.
        /// De categorie "alle categorieën" is standaard aanwezig in de lijst.
        /// Geeft een lege lijst terug als er geen categorieën gevonden werd.
        /// </summary>
        /// <returns>Een lijst van string instanties</returns>
        public async Task<List<string>> GetDefectCategorieën()
        {
            if (categoriëën.Count > 0)
                return categoriëën;
            else
            {
                try
                {
                    List<String> lijst = new List<String>();
                    lijst.Add("alle categorieën");
                    var problemen = await db.QueryAsync<DEFECT_CODES>("SELECT distinct Type_Defect FROM DSS_DEFECT_CODES");
                    if (problemen.Any())
                    {
                        for (int i = 0; i < problemen.Count; i++)
                        {
                            lijst.Add(problemen[i].Type_Defect.ToLower());
                        }
                        return lijst;
                    }
                }
                catch (Exception e)
                {
                    paLogging.log.Error("Fout bij het ophalen van de problemen uit de database\nFoutmelding: " + e.Message);
                }
            }
            return null;
        }

        public async Task<List<string>> GetObjectCategorieën()
        {
            if (categoriëën.Count > 0)
                return categoriëën;
            else
            {
                try
                {
                    List<String> lijst = new List<String>();
                    lijst.Add("alle categorieën");
                    var problemen = await db.QueryAsync<OBJECT_CODES>("SELECT distinct Type FROM DSS_OBJECT_CODES");
                    if (problemen.Any())
                    {
                        for (int i = 0; i < problemen.Count; i++)
                        {
                            lijst.Add(problemen[i].Type.ToLower());
                        }
                        return lijst;
                    }
                }
                catch (Exception e)
                {
                    paLogging.log.Error("Fout bij het ophalen van de problemen uit de database\nFoutmelding: " + e.Message);
                }
            }
            return null;
        }

        #endregion

        #region Comment-gerelateerd

        /// <summary>
        /// Het toevoegen van een opmerking
        /// </summary>
        /// <param name="commentaar">Een opmerking van het type Comment</param>
        /// <returns>Een Comment instantie</returns>
        public async Task<Comment> AddComment(Comment commentaar)
        {
            try
            {
                COMMENT nieuwCommentaar = new COMMENT
                {
                    Id = aantalCommentaren,
                    ObjectCodeId = commentaar.ObjectCodeId,
                    ObjectCode = commentaar.ObjectCode,
                    DefectCodeId = commentaar.DefectCodeId,
                    DefectCode = commentaar.DefectCode,
                    Omschrijving = commentaar.Omschrijving,
                    Vehicle_Id = completeAuto.Id,
                    Chauffeur = commentaar.Chauffeur,
                    Datum = commentaar.Datum,
                    Duplicate = commentaar.Duplicate,
                    OriginalId = commentaar.OriginalId,
                    Position = commentaar.Position,
                    Rating = commentaar.Rating
                };
                await db.InsertAsync(nieuwCommentaar);
                Comment opmerking = new Comment(nieuwCommentaar.Id, nieuwCommentaar.Omschrijving, nieuwCommentaar.ObjectCodeId, nieuwCommentaar.DefectCodeId, nieuwCommentaar.Vehicle_Id, nieuwCommentaar.DefectCode, nieuwCommentaar.ObjectCode, nieuwCommentaar.Chauffeur, nieuwCommentaar.Datum, nieuwCommentaar.Duplicate, nieuwCommentaar.OriginalId, nieuwCommentaar.Position, nieuwCommentaar.Rating);
                aantalCommentaren++;
                return opmerking;
            }
            catch (SQLiteException sqlEx)
            {
                paLogging.log.Error(String.Format("SQLite fout bij het wegschrijven van rijberichten {0} ({1} - {2}: {3}", commentaar.Id, commentaar.ObjectCodeId, commentaar.DefectCodeId, sqlEx.Message));
                return null;
            }
            catch (Exception e)
            {
                paLogging.log.Error(String.Format("Onbekende fout bij het wegschrijven van rijberichten {0} ({1} - {2}: {3}", commentaar.Id, commentaar.ObjectCodeId, commentaar.DefectCodeId, e.Message));
                return null;
            }

        }

        /// <summary>
        /// Het wijzigen van een opmerking
        /// Geeft false terug als de bewerking niet uitgevoerd kon worden
        /// </summary>
        /// <param name="commentaar">Een opmerking van het type Comment</param>
        /// <returns>Een boolean</returns>
        public async Task<bool> UpdateComment(Comment commentaar)
        {
            try
            {
                COMMENT updateCommentaar = new COMMENT
                {
                    Id = commentaar.Id,
                    ObjectCodeId = commentaar.ObjectCodeId,
                    ObjectCode = commentaar.ObjectCode,
                    DefectCodeId = commentaar.DefectCodeId,
                    DefectCode = commentaar.DefectCode,
                    Omschrijving = commentaar.Omschrijving,
                    Vehicle_Id = commentaar.Vehicle_Id,
                    Chauffeur = commentaar.Chauffeur,
                    Datum = commentaar.Datum,
                    Duplicate = commentaar.Duplicate,
                    OriginalId = commentaar.OriginalId
                };
                await db.UpdateAsync(updateCommentaar);
                return true;
            }
            catch (Exception e)
            {
                paLogging.log.Error(String.Format("Fout bij het aanpassen van het rijbericht {0}\nFoutmelding: {1}", commentaar.Id, e.Message));
                return false;
            }
        }

        /// <summary>
        /// Een opmerking kan verwijderd worden
        /// Geeft false terug als de bewerking niet uitgevoerd kon worden
        /// </summary>
        /// <param name="commentaar">Een opmerking van het type Comment</param>
        /// <returns>Een boolean</returns>
        public async Task<bool> DeleteComment(Comment commentaar)
        {
            try
            {
                await db.DeleteAsync(commentaar.GetCOMMENT());
                //aantalCommentaren--;
                return true;
            }
            catch (Exception e)
            {
                paLogging.log.Error("Fout bij het ophalen van de voertuig uit de database\nFoutmelding: " + e.Message);
                return false;
            }
        }

        /// <summary>
        /// Het ophalen van de comments die aanwezig zijn in de SQLite database
        /// </summary>
        /// <returns>Een lijst van Comment instanties</returns>
        public async Task<List<Comment>> GetComments()
        {
            if (commentaren.Count > 0)
                return commentaren;
            else
            {
                try
                {
                    var opmerkingen = await db.QueryAsync<COMMENT>("SELECT * FROM DSS_COMMENT where Vehicle_Id=?", new object[] { completeAuto.Id });
                    if (opmerkingen.Any())
                    {
                        int hulp = 0;
                        foreach (COMMENT opmerking in opmerkingen)
                        {
                            commentaren.Add(new Comment(opmerking.Id, opmerking.Omschrijving, opmerking.ObjectCodeId, opmerking.DefectCodeId, opmerking.Vehicle_Id, opmerking.DefectCode, opmerking.ObjectCode, opmerking.Chauffeur, opmerking.Datum, opmerking.Duplicate, opmerking.OriginalId, opmerking.Position, opmerking.Rating));
                            
                            if (hulp < opmerking.Id)
                            {
                                aantalCommentaren = opmerking.Id;
                            }                                                
                        }
                        aantalCommentaren += 1;
                        return commentaren;
                    }
                    else
                    {
                        aantalCommentaren = 0;
                    }
                }
                catch (Exception e)
                {
                    paLogging.log.Error("Fout bij het ophalen van de Rijberichten uit de database\n Foutmelding: " + e.Message);
                }
                return commentaren;
            }
        }

        #endregion

        #region Bijlagen ophalen

        /// <summary>
        /// Methode die een lijst van word bestandsnamen ophaalt.
        /// </summary>
        /// <returns>Een lijst van string instanties.</returns>
        public async Task<List<string>> GetPdfNamen()
        {
            try
            {
                return (await webService.haalPdfNamenOpAsync(LocalDB.database.GetAutoId())).Body.haalPdfNamenOpResult.ToList();
            }
            catch (Exception e)
            {
                paLogging.log.Error("Er is een fout opgetreden bij het ophalen van pdf namen: " + e.Message);
            }
            return null;
        }

        /// <summary>
        ///  Methode die alle pdf bestanden ophaalt van de webservice.
        /// </summary>
        /// <param name="pdfGewijzigdeNamen">De gewijzigde pdfnamen. </param>
        /// <returns>byte arrays met pdf byte arrays.</returns>
        public async Task<byte[][]> GetPDFBestanden(IReadOnlyList<string> pdfGewijzigdeNamen)
        {
            try
            {
                WebserviceRef.ArrayOfString array = new WebserviceRef.ArrayOfString();
                array.InsertRange(0, pdfGewijzigdeNamen);
                return ((await webService.haalPdfBestandenOpAsync(LocalDB.database.GetAutoId(), array)).Body.haalPdfBestandenOpResult.ToArray());
            }
            catch (Exception e)
            {
                paLogging.log.Error(String.Format("Er is een fout opgetreden bij het ophalen van pdf bestanden: {0}", e.Message));
            }
            return null;
        }

        /// <summary>
        /// Deze methode haalt den nieuwste gegevens op (pdfnamen en wijzigingsdata) en vergelijkt deze
        /// met de huidige wijzigingsdata. Alle op te halen pdf namen worden als resultaat opgehaald en doorgestuurd.
        /// </summary>
        /// <returns>Lijst van op te halen pdf bestanden</returns>
        public async Task<List<string>> ControleerTeVernieuwenBijlagen()
        {
            try
            {
                List<string> pdfWijzigingsdata = await GetGewijzigdePdfData();
                List<object> pdfHuidigeData = ((ApplicationDataCompositeValue)LocalStorage.localStorage.LaadGegevens("PdfWijzigingsData")).Values.ToList();

                List<string> pdfNamen = await PdfSupport.pdfSupport.GetPdfNamen(true);
                if (pdfHuidigeData == null || pdfWijzigingsdata.Count != pdfHuidigeData.Count)
                {
                    LocalStorage.localStorage.SlaGegevensOp("PdfWijzigingsData", pdfWijzigingsdata);
                    return pdfNamen;
                }

                IEnumerable<string> pdfTeVernieuwen =
                    from datum in pdfWijzigingsdata
                    where !pdfHuidigeData.Contains(datum)
                    select pdfNamen.ElementAt(pdfWijzigingsdata.IndexOf(datum));

                if (pdfTeVernieuwen.Any())
                {
                    PdfSupport.pdfSupport.VerwijderPdfBestanden();
                    LocalStorage.localStorage.SlaGegevensOp("PdfWijzigingsData", pdfWijzigingsdata);
                    return pdfTeVernieuwen.ToList<string>();
                }
            }
            catch (Exception ex)
            {
                paLogging.log.Critical(String.Format("Fout bij het controleren op nieuwe pdf-bestanden.\n{0}", ex.Message));
            }

            return new List<string>();
        }

        /// <summary>
        /// Roept de webservice op om de laatste data door te sturen voor de word en excel bestanden.
        /// </summary>
        /// <returns>Lijst van strings</returns>
        public async Task<List<string>> GetGewijzigdePdfData()
        {
            return (await webService.haalPdfWijzigingsdataOpAsync(GetAutoId())).Body.haalPdfWijzigingsdataOpResult.ToList<string>();
        }

        #endregion

        #region Synchronisatie

        public async Task<bool> SetMaakSVCvoorWebSync(ArrayOfString gegevens, string bestandsnaam)
        {
            return (await webService.MaakCSVAsync(gegevens, bestandsnaam)).Body.MaakCSVResult;
        }

        /// <summary>
        /// BRON: http://msdn.microsoft.com/en-us/library/windows/hardware/dn303343(v=vs.85).aspx
        /// Haalt alle bestanden van een USB stick en verplaatst deze naar de bijlagenmap
        /// </summary>
        /// <returns></returns>
        public async Task<bool> SynchroniseerVanUSB()
        {
            StorageFolder usbFolder = null, autoFolder = null;

            var usbApparaten = await DeviceInformation.FindAllAsync(StorageDevice.GetDeviceSelector());

            if (usbApparaten.Any())
                usbFolder = StorageDevice.FromId(usbApparaten[0].Id);
            if (usbFolder != null)
            {
                MessageDialog melding = new MessageDialog(String.Empty);
                try 
                {
                    if (completeAuto == null)
                        await HaalToegewezenAutoOp();

                    if (completeAuto != null)
                        autoFolder = await usbFolder.GetFolderAsync(completeAuto.Number);
                }
                catch
                {
                    melding = new MessageDialog(String.Format("De USB-stick bevat geen map voor wagen {0}.", completeAuto.Number), "Bijlagen niet gevonden");
                    melding.Commands.Add(new UICommand("Begrepen"));
                }
                if (!melding.Content.Equals(String.Empty))
                {
                    await melding.ShowAsync();
                    return false;
                }
            }
            else
                return false;

            var bijlageBestanden = (await autoFolder.GetFilesAsync()).Where(pdf => pdf.Name.Contains(".pdf"));
            StorageFolder doelMap = await PdfSupport.pdfSupport.GetDoelFolder();
            foreach (StorageFile pdf in bijlageBestanden)
                await pdf.CopyAsync(doelMap, pdf.Name, NameCollisionOption.ReplaceExisting);

            await PdfSupport.pdfSupport.GetAfbeeldingenVanPDF(PdfSupport.RENDEROPTIONS.NORMAL, bijlageBestanden.ToList<StorageFile>());
            return true;
        }

        /// <summary>
        /// Haalt alle commentaren op die reeds aangemaakt werden en vormt de juiste CSV-context om weg te schrijven naar een CSV-bestand.
        /// Elke commentaar vormt een nieuwe regel in dit bestand.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> SynchroniseerNaarUSB()
        {
            List<Comment> alleCommentaren = await OverzichtOpmerkingen.HaalCommentsOp();
            List<string> csvLijnen = new List<string>();
            //bool csvGeschreven = false;
            foreach (Comment commentaar in alleCommentaren)
            {
                csvLijnen.Add("Chauffeur: " + commentaar.Chauffeur +
                    ", OpmerkingID: " + commentaar.Id.ToString() +
                    ", Aangemaakt op: " + commentaar.Datum.ToString("dd/MM/yyyy HH:mm") +
                    ", Objectcode: " + commentaar.ObjectCode +
                    ", Defectcode: " + commentaar.DefectCode +
                    ", Positie: " + commentaar.Position +
                    ", Rating: " + commentaar.Rating +
                    ", Omschrijving: " + commentaar.Omschrijving +
                    ", Duplicate: " + commentaar.Duplicate +
                    ", Origineel: " + commentaar.OriginalId);
            }

            if (csvLijnen.Count() == 0) //Als er geen commentaren zijn...
            {
                MessageDialog okAnnuleer = new MessageDialog(String.Format("Er zijn geen rijberichten om te synchroniseren voor auto {0}.", completeAuto.Number), "Synchronisatie niet toepasselijk");
                okAnnuleer.Commands.Add(new UICommand("Begrepen"));
                await okAnnuleer.ShowAsync();
                return false; //Melding geven en methode afbreken
            }


            StorageFolder usbFolder = null, autoFolder = null;

            var usbApparaten = await DeviceInformation.FindAllAsync(StorageDevice.GetDeviceSelector()); //DeviceInformation gebruiken om alle USB-apparaten op te halen.

            if (usbApparaten.Count > 0) //Als er USB-apparaten zijn, neem de eerste (tabblet met 1 usb poort)
                usbFolder = StorageDevice.FromId(usbApparaten[0].Id);
            if (usbFolder != null) //Als er een USB-apparaat gebruikt is, en hier een auto folder aangetroffen werd...
            {
                bool autoFolderAanmaken = true;
                try
                {
                    autoFolder = await usbFolder.GetFolderAsync(completeAuto.Number); //Auto folder ophalen
                    autoFolderAanmaken = false; //Als deze code bereikt wordt, dan bestaat de auto folder op de USB.
                }
                catch
                {
                    paLogging.log.Info(String.Format("Map voor auto {0} aangemaakt op de USB-stick.", completeAuto.Number));
                }
                if (autoFolderAanmaken) //als dit nog true is, is de auto folder niet aangetroffen op de USB-stick
                    autoFolder = await usbFolder.CreateFolderAsync(completeAuto.Number); //en maken we hier een folder voor aan
            }

            string bestandsNaam = String.Format("Wagen {0} - {1}-{2}-{3} {4}h{5}m opmerkingen.csv", completeAuto.Number, DateTime.Now.Day, DateTime.Now.Month, DateTime.Now.Year,
                                                                                DateTime.Now.Hour, DateTime.Now.Minute); //bestandsnaam voor CSV-bestand aanmaken op basis van de datum en tijd.

            StorageFile exportBestand = await autoFolder.CreateFileAsync(bestandsNaam, CreationCollisionOption.GenerateUniqueName); //CSV-bestand aanmaken op de USB-stick
            await FileIO.AppendLinesAsync(exportBestand, csvLijnen); //Elke string in de lijst van CSV-lijnen asynchroon wegschrijven.
            //StorageFolder test = await ApplicationData.Current.LocalFolder.GetFolderAsync(@"\Photos");
            
            return true;
        }

        public async Task<int> getAantalCommentaren()
        {
            return aantalCommentaren;
        }
        #endregion


        /// <summary>
        /// Adding an oilsample
        /// </summary>
        /// <returns></returns>
        public async Task<bool> AddOilsample(Oilsample oilsample)
        {
            try
            {
                OILSAMPLE newOilsample = new OILSAMPLE
                {
                    Username = oilsample.Username,
                    Vehicle_Id = oilsample.Vehicle_Id,
                    Date = oilsample.Date,
                    Odo = oilsample.Odo,
                    Oillevel = oilsample.Oillevel,
                    Oiltaken = oilsample.Oiltaken,
                    Oilfilled = oilsample.Oilfilled,
                    OilUnit = oilsample.OilUnit,
                    Reason = oilsample.Reason,
                    Remarks = oilsample.Remarks
                };
                db.InsertAsync(newOilsample);
                
                return true;
            }
            catch (SQLiteException sqlEx)
            {
                paLogging.log.Error(String.Format("SQLite fout bij het wegschrijven van oilsample {0} ({1} - {2}: {3}", oilsample.Id, oilsample.Username, oilsample.Vehicle_Id, sqlEx.Message));
                return false;
            }
            catch (Exception e)
            {
                paLogging.log.Error(String.Format("Onbekende fout bij het wegschrijven van oilsample {0} ({1} - {2}: {3}", oilsample.Id, oilsample.Username, oilsample.Vehicle_Id, e.Message));
                return false;
            }
        }

    }
}