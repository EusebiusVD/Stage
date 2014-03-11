using PiXeL_Apps.SQLite.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiXeL_Apps.Classes
{
    public class Vehicle
    {
        public int Id { get; set; }
        public string Number { get; set; }
        public int Brand_Id { get; set; } //Linken
        public int Engine_Id { get; set; } //Linken
        public int Transmission_Id { get; set; } //Linken
        public int Eenheid_Id { get; set; }
        public bool Active { get; set; }

        public Vehicle(int id, string number)
        {
            Id = id;
            Number = number;
        }

        public Vehicle(int id, string number, int brandId, int engineId, int transmissionId, int eenheidId, bool active) 
        {
            Id = id;
            Number = number;
            Brand_Id = brandId;
            Engine_Id = engineId;
            Transmission_Id = transmissionId;
            Active = active;
        }

        public ASSIGNEDVEHICLE GetASSIGNEDVEHICLE()
        {
            return new ASSIGNEDVEHICLE { Id = this.Id, Number = this.Number, Brand_Id = this.Brand_Id, Engine_Id = this.Engine_Id, Transmission_Id = this.Transmission_Id };
        }
    }
}