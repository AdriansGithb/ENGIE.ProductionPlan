using ProductionPlan.Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductionPlan.Core.Models
{
    public class Fuel
    {
        public decimal Gas { get; set; }
        public decimal Kerosine { get; set; }
        public decimal Co2 { get; set; }
        public decimal Wind { get; set; }
    }
}
