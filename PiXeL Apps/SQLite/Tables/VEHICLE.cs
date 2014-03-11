using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiXeL_Apps.SQLite.Tables
{
    [Table("DSS_VEHICLE")]
    public class VEHICLE
    {
        [PrimaryKey, Unique]
        public int Id { get; set; }
        public string Number { get; set; }
        public int Brand_Id { get; set; } //Linken
        public int Engine_Id { get; set; } //Linken
        public int Transmission_Id { get; set; } //Linken
        public int Eenheid_Id { get; set; }
        public bool Active { get; set; }
    }
}