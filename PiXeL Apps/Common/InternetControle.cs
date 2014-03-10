using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;

namespace PiXeL_Apps.Common
{
    static class InternetControle
    {    
        /// <summary>
        /// Er wordt gecontroleerd of er internet is
        /// </summary>
        /// <returns>Een boolean </returns>
        public static Boolean ControleerInternet()
        {
            ConnectionProfile profiel = NetworkInformation.GetInternetConnectionProfile();
            if (profiel != null && profiel.GetNetworkConnectivityLevel() >= NetworkConnectivityLevel.LocalAccess)
                return true;
            else return false;
        }
    }
}