using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiXeL_Apps.Logging
{
    sealed class paLogging
    {
        public static paLogging log;

        public event ProcessLog processLog;
        public delegate void ProcessLog(string level, string message);

        public void Debug(string strMessage)
        {
            log.processLog("Debug", strMessage);
        }

        public void Info(string strMessage)
        {
            log.processLog("Info", strMessage); ;
        }

        public void Warn(string strMessage)
        {
            log.processLog("Warn", strMessage);
        }

        public void Error(string strMessage)
        {
            log.processLog("Error", strMessage);
        }

        public void Critical(string strMessage)
        {
            log.processLog("Critical", strMessage);
        }
    }
}
