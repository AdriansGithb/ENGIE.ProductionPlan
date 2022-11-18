using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ProductionPlan.Core.Models
{
    public class Payload
    {
        public decimal Load { get; set; }
        public Fuel Fuels { get; set; }
        public IEnumerable<Powerplant> Powerplants { get; set; }
    }
}
