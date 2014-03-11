using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiXeL_Apps.Classes
{
    public class Category
    {
        public int Id { get; set; }
        public string Problemcategory { get; set; }
        public string Description { get; set; }
        public Category(int id, string problemcategorie, string omschrijving)
        {
            Id = id;
            Problemcategory = problemcategorie;
            Description = omschrijving;
        }
        public Category(int id, string problemcategorie)
        {
            Id = id;
            Problemcategory = problemcategorie;
        }
    }
}
