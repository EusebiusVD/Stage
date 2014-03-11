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

       public Comment() { }
       public Comment(int id, string omschrijving)
       {
           Id = id;
           Omschrijving = omschrijving;
       }
       public Comment(int id, string omschrijving, int objectCodeId, int defectcodeid, int assignedvehicle)
       {
           Id = id;
           Omschrijving = omschrijving;
           ObjectCodeId = objectCodeId;
           DefectCodeId = defectcodeid;
           Vehicle_Id = assignedvehicle;
       }
       public Comment(int id, string omschrijving, int objectCodeId, int defectcodeid, int assignedvehicle, string defectcode, string objectcode)
       {
           Id = id;
           Omschrijving = omschrijving;
           ObjectCodeId = objectCodeId;
           DefectCodeId = defectcodeid;
           Vehicle_Id = assignedvehicle;
           DefectCode = defectcode;
           ObjectCode = objectcode;
       }
       public Comment(int id, string omschrijving, int objectCodeId, int defectcodeid, int assignedvehicle, string defectcode, string objectcode,string chauffeur, DateTime datum)
       {
           Id = id;
           Omschrijving = omschrijving;
           ObjectCodeId = objectCodeId;
           DefectCodeId = defectcodeid;
           Vehicle_Id = assignedvehicle;
           DefectCode = defectcode;
           ObjectCode = objectcode;
           Chauffeur = chauffeur;
           Datum = datum;
       }
       public Comment(int id, string omschrijving, int objectCodeId, int defectcodeid, int assignedvehicle, string chauffeur,DateTime datum)
       {
           Id = id;
           Omschrijving = omschrijving;
           ObjectCodeId = objectCodeId;
           DefectCodeId = defectcodeid;
           Vehicle_Id = assignedvehicle;
           Chauffeur = chauffeur;
           Datum = datum;
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
               Datum = this.Datum
           };
       }

    }
}
