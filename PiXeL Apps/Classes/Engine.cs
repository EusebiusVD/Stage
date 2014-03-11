using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PiXeL_Apps.SQLite.Tables;

namespace PiXeL_Apps.Classes
{
    public class Engine
    {
        public int Id { get; set; }
        public string Engine_Code { get; set; }
        public decimal Capacity { get; set; }
        public int Fuel_Id { get; set; }

        public Engine() { }

        public Engine(int id, string engine_code, decimal capacity, int fuel_id)
        {
            Id = id;
            Engine_Code = engine_code;
            Capacity = capacity;
            Fuel_Id = fuel_id;
        }

        /// <summary>
        /// Haalt een ENGINE database instantie op waarmee een record kan worden bijgevuld, geupdate of verwijderd.
        /// </summary>
        /// <returns>ENGINE database object</returns>
        public ENGINE GetENGINE()
        {
            return new ENGINE
            {
                Id = this.Id,
                Engine_Code = this.Engine_Code,
                Capacity = this.Capacity.ToString(),
                Fuel_Id = this.Fuel_Id
            };
        }
    }
}