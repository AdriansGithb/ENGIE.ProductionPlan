using ProductionPlan.Core.Models;
using ProductionPlan.Core.Models.Enums;

namespace ProductionPlan.Core.Mappers
{
    public static class MyMapper
    {
        public static PowerGenerationUnit ToWindPowerGenerationUnit(this Powerplant powerplant, decimal wind)
        {
            if (powerplant.Type != PowerplantTypeEnum.windturbine)
                throw new ArgumentException("Cannot map to (wind)PowerGenerationUnit because powerplant type is not wind");

            decimal p = Math.Round(powerplant.PMax * powerplant.Efficiency * (wind / 100), 1, MidpointRounding.ToZero);

            return new PowerGenerationUnit
            {
                Name = powerplant.Name,
                Type = powerplant.Type,
                Efficiency = powerplant.Efficiency,
                PMin = p,
                PMax = p,
                ProductionCostPerMwh = 0
            };
        }

        public static PowerGenerationUnit ToFuelPowerGenerationUnit(this Powerplant powerplant, decimal fuelPricePerMwh, decimal co2Price)
        {
            if (powerplant.Type == PowerplantTypeEnum.windturbine)
                throw new ArgumentException("Cannot map to (fuel)PowerGenerationUnit because powerplant type is not a fuel type");
            return new PowerGenerationUnit
            {
                Name = powerplant.Name,
                Type = powerplant.Type,
                Efficiency = powerplant.Efficiency,
                PMin = Math.Round(powerplant.PMin * powerplant.Efficiency, 1, MidpointRounding.ToZero),
                PMax = Math.Round(powerplant.PMax * powerplant.Efficiency, 1, MidpointRounding.ToZero),
                ProductionCostPerMwh = powerplant.Type switch
                {
                    PowerplantTypeEnum.gasfired => (fuelPricePerMwh / powerplant.Efficiency) + (0.3M * co2Price),
                    _ => fuelPricePerMwh / powerplant.Efficiency
                },
            };
        }

        public static PlannedProductionPowerplant ToPlannedProductionPowerplant(this PowerGenerationUnit powerUnit) 
        {
            return new PlannedProductionPowerplant
            {
                Name = powerUnit.Name,
                P = powerUnit.AdvisedProduction
            };
        }
    }
}
