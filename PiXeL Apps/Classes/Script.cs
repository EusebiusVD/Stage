using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiXeL_Apps.Classes
{
    public class Script
    {
        public int Id { get; set; }
        public int Vehicle_Id { get; set; }
        public int Kilometerstand { get; set; }
        public string Cycli { get; set; }
        public string Activity { get; set; }
        public int Eenheid { get; set; }
        public bool Status { get; set; }

        public Script(int id, int vehicle_id, int kilometerstand, string cycli, string activity, int eenheid, bool status)
        {
            this.Id = id;
            this.Vehicle_Id = vehicle_id;
            this.Kilometerstand = kilometerstand;
            this.Cycli = cycli;
            this.Activity = activity;
            this.Eenheid = eenheid;
            this.Status = status;
        }
    }
}
