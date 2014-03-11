using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiXeL_Apps.SQLite.Tables
{
    [Table("DSS_FUEL")]
    public class FUEL
    {
        [PrimaryKey, Unique]
        public int Id { get; set; }
        public String FuelName { get; set; }
    }
}
