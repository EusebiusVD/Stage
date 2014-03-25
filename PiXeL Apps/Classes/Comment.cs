using PiXeL_Apps.SQLite.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiXeL_Apps.Classes
{
    public class Comment
    {
        public int Id { get; set; }
        public string Omschrijving { get; set; }
        public int ObjectCodeId { get; set; }
        public int DefectCodeId { get; set; }
        public int Vehicle_Id { get; set; }
        public string DefectCode { get; set; }
        public string ObjectCode { get; set; }
        public string Chauffeur { get; set; }
        public DateTime Datum { get; set; }
        public int Duplicate { get; set; }
        public int OriginalId { get; set; }
        public string Position { get; set; }
        public string Rating { get; set; }

        public Comment() { }
        public Comment(int id, string omschrijving)
        {
            Id = id;
            Omschrijving = omschrijving;
        }
        public Comment(int id, string Omschrijving, int ObjectCodeId, int DefectCodeId, int Vehicle_Id)
        {
            this.Id = id;
            this.Omschrijving = Omschrijving;
            this.ObjectCodeId = ObjectCodeId;
            this.DefectCodeId = DefectCodeId;
            this.Vehicle_Id = Vehicle_Id;
        }
        public Comment(int Id, string Omschrijving, int ObjectCodeId, int DefectCodeId, int Vehicle_Id, string DefectCode, string ObjectCode)
        {
            this.Id = Id;
            this.Omschrijving = Omschrijving;
            this.ObjectCodeId = ObjectCodeId;
            this.DefectCodeId = DefectCodeId;
            this.Vehicle_Id = Vehicle_Id;
            this.DefectCode = DefectCode;
            this.ObjectCode = ObjectCode;
        }
        public Comment(int Id, string Omschrijving, int ObjectCodeId, int DefectCodeId, int Vehicle_Id, string DefectCode, string ObjectCode, string Chauffeur, DateTime Datum)
        {
            this.Id = Id;
            this.Omschrijving = Omschrijving;
            this.ObjectCodeId = ObjectCodeId;
            this.DefectCodeId = DefectCodeId;
            this.Vehicle_Id = Vehicle_Id;
            this.DefectCode = DefectCode;
            this.ObjectCode = ObjectCode;
            this.Chauffeur = Chauffeur;
            this.Datum = Datum;
        }
        public Comment(int Id, string Omschrijving, int ObjectCodeId, int DefectCodeId, int Vehicle_Id, string Chauffeur, DateTime Datum)
        {
            this.Id = Id;
            this.Omschrijving = Omschrijving;
            this.ObjectCodeId = ObjectCodeId;
            this.DefectCodeId = DefectCodeId;
            this.Vehicle_Id = Vehicle_Id;
            this.Chauffeur = Chauffeur;
            this.Datum = Datum;
        }
        public Comment(int Id, string Omschrijving, int ObjectCodeId, int DefectCodeId, int Vehicle_Id, string DefectCode, string ObjectCode, string Chauffeur, DateTime Datum, int Duplicate, int OriginalId, string Position, string Rating)
        {
            this.Id = Id;
            this.Omschrijving = Omschrijving;
            this.ObjectCodeId = ObjectCodeId;
            this.DefectCodeId = DefectCodeId;
            this.Vehicle_Id = Vehicle_Id;
            this.DefectCode = DefectCode;
            this.ObjectCode = ObjectCode;
            this.Chauffeur = Chauffeur;
            this.Datum = Datum;
            this.Duplicate = Duplicate;
            this.OriginalId = OriginalId;
            this.Position = Position;
            this.Rating = Rating;
        }

        public COMMENT GetCOMMENT()
        {
            return new COMMENT
            {
                Id = this.Id,
                ObjectCodeId = this.ObjectCodeId,
                ObjectCode = this.ObjectCode,
                DefectCodeId = this.DefectCodeId,
                DefectCode = this.DefectCode,
                Vehicle_Id = this.Vehicle_Id,
                Omschrijving = this.Omschrijving,
                Chauffeur = this.Chauffeur,
                Datum = this.Datum,
                Duplicate = this.Duplicate,
                OriginalId = this.OriginalId
            };
        }

    }
}