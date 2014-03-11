using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Webservice
{
    public class CSVGenerator
    {   
        /// <summary>
        /// Het omzetten van een bestand naar CSV + dt wegschrijven op de webserver 
        /// </summary>
        /// <param name="lijstGegevens">De lijst van string instanties </param>
        /// <param name="bestandsnaam">De naam van het bestand van het type string</param>
        /// <returns>Een boolean die aangeeft of het wegschrijven gelukt is </returns>
        public static Boolean SchrijfCSV(List<String> lijstGegevens, String bestandsnaam)
        {
            try
            {
                string pad = @"C://PiXel-Apps/CSVBestanden/" + bestandsnaam; //In geval van nood

                //De tekst goed zetten voor in het CSV bestand
                StringBuilder sbUitvoerMaken = new StringBuilder();
                for (int index = 0; index < lijstGegevens.Count; index++)
                    sbUitvoerMaken.AppendLine(string.Join(";", lijstGegevens[index]));

                //Het CSV bestand aanmaken + wegschrijven 
                File.WriteAllText(pad, sbUitvoerMaken.ToString());

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Het omzetten van een bestand naar een CSV + deze wegschrijven naar een USB
        /// </summary>
        /// <param name="lijstGegevens">Lijst van String instanties die weggeschreven gaan worden</param>
        /// <param name="bestandsnaam"></param>
        /// <returns></returns>
        public static Boolean SchrijfCSVUSB(List<String> lijstGegevens, String bestandsnaam)
        {
            try
            {   //pad dat gebruikt gaat worden ingeval van nood
                string pad = @"C:/Pixel-Apps/" + bestandsnaam;

                //Het ophalen van de usb stick of hardeschijf
                DriveInfo[] ListDrives = DriveInfo.GetDrives();
                foreach (DriveInfo Drive in ListDrives)
                {
                    if (Drive.DriveType == DriveType.Removable)
                        pad = Drive.Name + bestandsnaam;
                }

                //De tekst goed zetten voor in het CSV bestand
                StringBuilder sbUitvoerMaken = new StringBuilder();
                for (int index = 0; index < lijstGegevens.Count; index++)
                    sbUitvoerMaken.AppendLine(string.Join(";", lijstGegevens[index]));

                //Het CSV bestand aanmaken + wegschrijven 
                File.WriteAllText(pad, sbUitvoerMaken.ToString());

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}