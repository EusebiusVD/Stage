using PiXeL_Apps.SQLite.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiXeL_Apps.Classes
{
    public class DefectCodes
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Beschrijving { get; set; }
        public string Type_Defect { get; set; }

        public DefectCodes(int id, string code, string omschrijving, string typeDefect)
        {
            Id = id;
            Code = code;
            Beschrijving = omschrijving;
            Type_Defect = typeDefect;
        }
        public DefectCodes(int id, string code)
        {
            Id = id;
            Code = code;
        }
        public DefectCodes(int id, string code, string typeDefect)
        {
            Id = id;
            Code = code;
            Type_Defect = typeDefect;
        }

        public DEFECT_CODES GetDEFECTCODES()
        {
            return new DEFECT_CODES
            {
                Id = this.Id,
                Code = this.Code,
                Beschrijving = this.Beschrijving,
                Type_Defect = this.Type_Defect
            };
        }
    }
}
