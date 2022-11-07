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
        public FuelTypeEnum Type { get; set; }
        public float Amount { get; set; }
    }
}
