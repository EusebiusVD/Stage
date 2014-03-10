using PiXeL_Apps.SQLite.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiXeL_Apps.Classes
{
    public class Inspectie
    {
        public int Id { get; set; }
        public int InspectieId { get; set; }
        public int VehicleId { get; set; }
        public int Kilometerstand { get; set; }
        public string Cycli { get; set; }
        public string Activity { get; set; }
        public int Eenheid { get; set; }
        public bool Status { get; set; } //voltooid of te doen

        public Inspectie(int Id, int InspectieId, int VehicleId, int Km, string Cycli, string Activity, int Eenheid, bool Status)
        {
            this.Id = Id;
            this.InspectieId = InspectieId;
            this.VehicleId = VehicleId;
            this.Kilometerstand = Km;
            this.Cycli = Cycli;
            this.Activity = Activity;
            this.Eenheid = Eenheid;
            this.Status = Status;
        }

        public Inspectie()
        { }

        /// <summary>
        /// Geef het database object van deze instantie terug.
        /// De database instantie kan gebruikt worden om een record aan te maken, bij te werken of te verwijderen
        /// </summary>
        /// <returns></returns>
        public INSPECTIE GetINSPECTIE()
        {
            return new INSPECTIE
            {
                Id = this.Id,
                Inspectie_Id = this.InspectieId,
                Vehicle_Id = this.VehicleId,
                Kilometerstand = this.Kilometerstand,
                Cycli = this.Cycli,
                Activity = this.Activity,
                Eenheid = this.Eenheid,
                Status = this.Status
            };
        }
    }
}
