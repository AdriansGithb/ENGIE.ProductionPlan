using ProductionPlan.Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace ProductionPlan.Core.Models
{
    public class PowerGenerationUnit
    {
        public string Name { get; set; }
        public PowerplantTypeEnum Type { get; set; }
        public decimal Efficiency { get; set; }
        public decimal PMin { get; set; }
        public decimal PMax { get; set; }
        public decimal AdvisedProduction { get; set; } = 0;
        public decimal ProductionCostPerUnit { get; set; }



        public PowerGenerationUnit DeepCopy()
        {
            PowerGenerationUnit clone = (PowerGenerationUnit)this.MemberwiseClone();
            clone.Name = this.Name;
            clone.Type = this.Type;
            return clone;
        }

    }
}
