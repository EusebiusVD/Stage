using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiXeL_Apps.SQLite
{
    [Table("DSS_WEIGHTS")]
    public class WEIGHTS
    {
        [PrimaryKey, Unique]
        public int Id { get; set; }
        public float Weight { get; set; }
    }
}
