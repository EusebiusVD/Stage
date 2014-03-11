using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiXeL_Apps.Classes
{
    public class AssignedVehicle
    {
        public int Id { get; set; }
        public string Number { get; set; }
        public int BrandId { get; set; } //Linken
        public int EngineId { get; set; } //Linken
        public int TransmissionId { get; set; } //Linken
        public string Kilometerstand { get; set; }
        public string Oliepeil { get; set; }

        public AssignedVehicle(int id, string number)
        {
            Id = id;
            Number = number;
        }

        public AssignedVehicle(int id, string number, int kindId, int engineId, int transmissionId) 
        {
            Id = id;
            Number = number;
            BrandId = kindId;
            EngineId = engineId;
            TransmissionId = transmissionId;
        }
        public AssignedVehicle(int id, string number, int kindId, int engineId, int transmissionId, string kilometerstand, string oliepeil)
        {
            Id = id;
            Number = number;
            BrandId = kindId;
            EngineId = engineId;
            TransmissionId = transmissionId;
            Kilometerstand = kilometerstand;
            Oliepeil = oliepeil;

        }
    }
}