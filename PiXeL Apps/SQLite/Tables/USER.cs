using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiXeL_Apps.SQLite.Tables
{
    [Table("DSS_USER")]
    public class USER
    {
        [PrimaryKey, Unique]
        public int Id { get; set; }
        public String Username { get; set; }
        public String Password { get; set; }
        public Boolean Admin { get; set; } //0 = testrijder / 1 = admin
    }
}