using ProductionPlan.Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ProductionPlan.Core.Models
{
    public class Fuel
    {
        [JsonPropertyName(@"gas(euro/MWh)")]
        public decimal GasCostPerMwh { get; set; }
        [JsonPropertyName(@"kerosine(euro/MWh)")]
        public decimal KerosineCostPerMwh { get; set; }
        [JsonPropertyName(@"co2(euro/ton)")]
        public decimal Co2CostPerTon { get; set; }
        [JsonPropertyName(@"wind(%)")]
        public decimal WindEfficiency { get; set; }
    }
}
