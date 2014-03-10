using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;

namespace Webservice
{
    class Logging
    {
        private String fileNameValue;
        public static Logging log = new Logging(String.Format("{0}{1:dd-MM-yyyy}{2}", ConfigurationManager.AppSettings["LoggingPath"], DateTime.UtcNow, ".log"));

        public String FileName
        {
            get
            {
                return fileNameValue;
            }
            set
            {
                fileNameValue = value;
            }
        }

        /// <summary>
        ///In deze constructor wordt de methode makeFolder aangeroepen
        /// </summary>
        public Logging()
        {
            MakeFolder();
        }
        /// <summary>
        /// In deze constructor wordt de methode makeFolder aangeroepen, ook wordt de FileName gezet 
        /// </summary>
        /// <param name="fileName">FileName is van het type tekst</param>
        public Logging(String fileName)
        {
            MakeFolder();
            this.FileName = fileName;
        }
        /// <summary>
        /// In deze methode wordt het path opgehaald waarop de folder aangemaakt moet worden
        /// Indien er geen Directory aanwezig is op dit path dan wordt deze aangemaakt
        /// </summary>
        public void MakeFolder()
        {
            String path = ConfigurationManager.AppSettings["LoggingPath"].ToString();
            try
            {
                if (!Directory.Exists(path))
                {
                    DirectoryInfo di = Directory.CreateDirectory(path);
                }
            }
            catch (IOException ioex)
            {
                Console.WriteLine(ioex.Message);
            }
        }

        /// <summary>
        /// In deze methode wordt de StreamWriter aangemaakt, de foutmelding samengesteld en doorgegeven
        /// </summary>
        /// <param name="app">App is van het type String</param>
        /// <param name="line">Line is van het type String, is de regel waarop het fout wordt opgeworpen</param>
        public void WriteLine(String app, String line)
        {
            StreamWriter writer = null;
            try
            {
                using (writer = new StreamWriter(FileName, true))
                {
                    String prefix = DateTime.UtcNow.ToString("dd/MM/yyyy hh:mm:ss UTC") + " : App : " + app.PadRight(20) + " - ";

                    line = prefix + line;

                    line = line.Replace("/n", "/n" + prefix);

                    writer.WriteLine(line);
                    writer.Flush();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                writer.Close();
                writer = null;
            }
        }
        /// <summary>
        /// In deze methode wordt de StreamWriter aangemaakt, de foutmelding samengesteld
        /// </summary>
        /// <param name="App">App is van het type String</param>
        /// <param name="ex">ex is van het type Exception</param>
        public void WriteLine(String App, Exception ex)     //STEVEN!!!!
        {
            StreamWriter writer = null;
            String Line;
            try
            {
                using (writer = new StreamWriter(FileName, true))
                {
                    String prefix = DateTime.UtcNow.ToString("dd/MM/yyyy hh:mm:ss UTC") + " : App : " + App.PadRight(20);

                    Line = prefix + " - " + "Message : " + ex.Message;
                    writer.WriteLine(Line);

                    var currentStack = new StackTrace(ex, 0, true);
                    StackFrame[] stackFrames = currentStack.GetFrames();

                    for (int i = stackFrames.Length - 1; i >= 0; i--)
                    {

                        Line = prefix + " - " + "Source : " + System.IO.Path.GetFileName(stackFrames[i].GetFileName()) + " Methode : " + stackFrames[i].GetMethod() + " Line : " + stackFrames[i].GetFileLineNumber();
                        writer.WriteLine(Line);
                        prefix = prefix + "   ";
                    }
                    writer.Flush();
                }
            }
            catch (Exception exept)
            {
                throw exept;
            }
            finally
            {
                writer.Close();
                writer = null;
            }
        }

    }
}
