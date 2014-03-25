using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiXeL_Apps.SQLite.Tables
{
    [Table("DSS_OILSAMPLE")]

    public class OILSAMPLE
    {
        [PrimaryKey, Unique, AutoIncrement]
        public int Id { get; set; }
        public String Username { get; set; }
        public int Vehicle_Id { get; set; }
        public DateTime Date { get; set; }
        public int Odo { get; set; }
        public double Oillevel { get; set; }
        public int Oiltaken { get; set; }
        public int Oilfilled { get; set; }
        public String Reason { get; set; }
        public String Remarks { get; set; }
    }
}
