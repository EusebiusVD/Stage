using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Webservice
{
    public class LeesUSB
    {
        public static Boolean KopieerBestanden(String wagennummer)
        {
            string pad = "";
            try
            {
                DriveInfo[] ListDrives = DriveInfo.GetDrives();
                foreach (DriveInfo Drive in ListDrives)
                {
                    if (Drive.DriveType == DriveType.Removable && Drive.IsReady)
                        pad = Drive.Name + @"\" + wagennummer;
                }

                if (File.Exists(pad+"/Wagenmap.pdf")){
                    foreach (string file in Directory.GetFiles(pad))
                    {
                        try
                        {
                            File.Copy(file,Path.Combine(@"~/bestanden/" + wagennummer, Path.GetFileName(file))); //Van, Naar
                        }
                        catch (Exception e)
                        {
                            Logging.log.WriteLine("LeesUSB", "Er is een fout opgetreden bij het kopieren van de bestanden \n" + e.Message + "\n" + e.StackTrace);
                        }
                    }
                    
                }
                else
                {
                    File.Replace(pad, @"~/bestanden", "Backup");
                }
                
                
                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }
    }
}