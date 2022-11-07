using ProductionPlan.Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductionPlan.Core.Models
{
    public class Powerplant
    {
        public string Name { get; set; }
        public PowerplantTypeEnum Type { get; set; }
        public float Efficiency { get; set; }
        public int PMin { get; set; }
        public int PMax { get; set; }
    }
}
