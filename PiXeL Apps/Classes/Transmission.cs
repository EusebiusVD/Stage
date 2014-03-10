using PiXeL_Apps.SQLite.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiXeL_Apps.Classes
{
    public class Transmission
    {
        public int Id { get; set; }
        public string Transmission_Name { get; set; }
        public int Gears { get; set; }

        public Transmission(int id, string transmissionName, int gears)
        {
            Id = id;
            Transmission_Name = transmissionName;
            Gears = gears;
        }

        public Transmission()
        {
            // TODO: Complete member initialization
        }

        /// <summary>
        /// Maakt een nieuwe TRANSMISSIONS instantie aan dat kan weggeschreven worden naar de database
        /// </summary>
        /// <returns>TRANSMISSION database object</returns>
        public TRANSMISSION GetTRANSMISSION()
        {
            return new TRANSMISSION
            {
                Id = this.Id,
                Transmission_Name = this.Transmission_Name,
                Gears = this.Gears
            };
        }
    }
}