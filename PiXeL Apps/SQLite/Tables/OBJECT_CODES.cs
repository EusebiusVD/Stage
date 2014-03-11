using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiXeL_Apps.SQLite.Tables
{
    [Table("DSS_OBJECT_CODES")]
    public class OBJECT_CODES
    {
        [PrimaryKey, Unique]
        public int Id { get; set; }
        public string Code { get; set; }
        public string Type { get; set; }
        public string Referentie { get; set; }
        public string Beschrijving { get; set; }
    }
}

