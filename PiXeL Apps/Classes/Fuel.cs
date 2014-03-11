using PiXeL_Apps.SQLite.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiXeL_Apps.Classes
{
    public class Fuel
    {
        public int Id { get; set; }
        public String FuelName { get; set; }

        public Fuel(int id, String fuel)
        {
            Id = id;
            FuelName = fuel;
        }

        /// <summary>
        /// Maakt een nieuwe FUEL instantie aan dat kan weggeschreven worden naar de database
        /// </summary>
        /// <returns>FUEL database object</returns>
        public FUEL GetFUEL()
        {
            return new FUEL
            {
                Id = this.Id,
                FuelName = this.FuelName
            };
        }

        public Fuel()
        { }
    }
}