using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiXeL_Apps.SQLite.Tables
{
    [Table("DSS_DEFECT_CODES")]
    public class DEFECT_CODES
    {
        [PrimaryKey, Unique]
        public int Id { get; set; }
        public string Code { get; set; }
        public string Beschrijving { get; set; }
        public string Type_Defect { get; set; }
    }
}

