using PiXeL_Apps.SQLite.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiXeL_Apps.Classes
{
    public class Brand
    {
        public int Id { get; set; }
        public String BrandName { get; set; } //vb Ford
        public string Type { get; set; } //vb Mondeo

        public Brand(int id, string brand, string type)
        {
            Id = id;
            BrandName = brand;
            Type = type;
        }

        public Brand()
        {
            // TODO: Complete member initialization
        }

        /// <summary>
        /// Maakt een nieuwe BRAND instantie aan dat kan weggeschreven worden naar de database
        /// </summary>
        /// <returns>BRAND database object</returns>
        public BRAND GetBRAND()
        {
            return new BRAND
            {
                Id = this.Id,
                BrandName = this.BrandName,
                Type = this.Type
            };
        }
    }
}
