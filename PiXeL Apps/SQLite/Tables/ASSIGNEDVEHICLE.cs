using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiXeL_Apps.SQLite.Tables
{
    [Table("DSS_ASSIGNEDVEHICLE")]
    public class ASSIGNEDVEHICLE
    {
        [PrimaryKey, Unique]
        public int Id { get; set; }
        public string Number { get; set; }
        public int Brand_Id { get; set; }
        public int Engine_Id { get; set; }
        public int Transmission_Id { get; set; }
        public string Kilometerstand { get; set; }
        public string Oliepeil { get; set; }
    }
}
