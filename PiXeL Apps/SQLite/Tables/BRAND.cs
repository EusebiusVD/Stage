using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiXeL_Apps.SQLite.Tables
{
    [Table("DSS_BRAND")]
    public class BRAND
    {
        [PrimaryKey, Unique]
        public int Id { get; set; }
        public string BrandName { get; set; }
        public string Type { get; set; }
    }
}