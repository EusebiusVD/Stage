using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiXeL_Apps.SQLite.Tables
{
    [Table("DSS_ENGINE")]
    public class ENGINE
    {
        [PrimaryKey, Unique]
        public int Id { get; set; }
        public string Engine_Code { get; set; }
        public string Capacity { get; set; }
        public int Fuel_Id { get; set; }
    }
}
