using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiXeL_Apps.SQLite.Tables
{
    [Table("DSS_COMMENT")]
    public class COMMENT
    {
        [PrimaryKey, Unique]
        public int Id { get; set; }
        public String Omschrijving { get; set; }
        public int ObjectCodeId { get; set; }
        public String ObjectCode { get; set; }
        public int DefectCodeId { get; set; }
        public String DefectCode { get; set; }
        public int Vehicle_Id { get; set; }
        public String Chauffeur { get; set; }
        public DateTime Datum { get; set; }
        public int Duplicate { get; set; }
        public int OriginalId { get; set; }
    }
}
