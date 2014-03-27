using PiXeL_Apps.SQLite.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiXeL_Apps.Classes
{
    public class Oilsample
    {
        public int Id { get; set; }
        public String Username { get; set; }
        public int Vehicle_Id { get; set; }
        public DateTime Date { get; set; }
        public int Odo { get; set; }
        public double Oillevel { get; set; }
        public int Oiltaken { get; set; }
        public int Oilfilled { get; set; }
        public String OilUnit { get; set; }
        public String Reason { get; set; }
        public String Remarks { get; set; }

        public Oilsample() { }

        public Oilsample(string username, int vehicle_id, DateTime date, int odo, double oillevel, int oiltaken, int oilfilled, string oilunit, string reason, string remarks)
        {
            Username = username;
            Vehicle_Id = vehicle_id;
            Date = date;
            Odo = odo;
            Oillevel = oillevel;
            Oiltaken = oiltaken;
            OilUnit = oilunit;
            Oilfilled = oilfilled;
            Reason = reason;
            Remarks = remarks;
        }
    }
}
