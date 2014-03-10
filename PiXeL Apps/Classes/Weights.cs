using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiXeL_Apps.Classes
{
    public class Weights
    {
        public int Id { get; set; }
        public float Weight { get; set; }

        public Weights(int id, float weight)
        {
            Id = id;
            Weight = weight;
        }
    }
}