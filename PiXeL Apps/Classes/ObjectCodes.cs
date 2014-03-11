using PiXeL_Apps.SQLite.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiXeL_Apps.Classes
{
    public class ObjectCodes
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Type { get; set; }
        public string Referentie { get; set; }
        public string Beschrijving { get; set; }

        public ObjectCodes(int id, string code, string type, string referentie, string beschrijving)
        {
            Id = id;
            Code = code;
            Type = type;
            Referentie = referentie;
            Beschrijving = beschrijving;
        }
        public ObjectCodes(int id, string code)
        {
            Id = id;
            Code = code;
        }
        public ObjectCodes(int id, string code, string type, string beschrijving)
        {
            Id = id;
            Code = code;
            Type = type;
            Beschrijving = beschrijving;
        }

        public ObjectCodes()
        {
            // TODO: Complete member initialization
        }

        public OBJECT_CODES GetOBJECTCODES()
        {
            return new OBJECT_CODES
            {
                Id = this.Id,
                Code = this.Code,
                Type = this.Type,
                Referentie = this.Referentie,
                Beschrijving = this.Beschrijving
            };
        }
    }
}
