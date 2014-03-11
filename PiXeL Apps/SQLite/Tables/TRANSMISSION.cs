using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiXeL_Apps.SQLite.Tables
{
    [Table("DSS_TRANSMISSION")]
    public class TRANSMISSION
    {
        [PrimaryKey, Unique]
        public int Id { get; set; }
        public string Transmission_Name { get; set; }
        public int Gears { get; set; }
    }
}
