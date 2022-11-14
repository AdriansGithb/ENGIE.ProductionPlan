using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductionPlan.Core.Models
{
    public class Payload
    {
        public Load Load { get; set; }
        public Fuel Fuels { get; set; }
        public IEnumerable<Powerplant> Powerplants { get; set; }
    }
}
