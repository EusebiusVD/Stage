using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiXeL_Apps.SQLite.Tables
{
    [Table("DSS_SCRIPT")]
    public class SCRIPT
    {
        [PrimaryKey, Unique]
        public int Id { get; set; }
        public int Vehicle_Id { get; set; }
        public int Kilometerstand { get; set; }
        public string Cycli { get; set; }
        public string Activity { get; set; }
        public int Eenheid { get; set; }
        public bool Status { get; set; }
    }
}
