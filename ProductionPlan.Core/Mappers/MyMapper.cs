﻿using ProductionPlan.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ProductionPlan.Core.Mappers
{
    public static class MyMapper
    {
        public static PowerGenerationUnit ToWindPowerGenerationUnit(this Powerplant powerplant, decimal wind)
        {
            return new PowerGenerationUnit
            {
                Name = powerplant.Name,
                Type = powerplant.Type,
                Efficiency = powerplant.Efficiency,
                PMin = Math.Round(powerplant.PMin * powerplant.Efficiency * (wind / 100), 1, MidpointRounding.ToZero),
                PMax = Math.Round(powerplant.PMax * powerplant.Efficiency * (wind / 100), 1, MidpointRounding.ToZero),
                ProductionCostPerUnit = 0
            };
        }

        public static PowerGenerationUnit ToFuelPowerGenerationUnit(this Powerplant powerplant, decimal fuelPricePerMwh)
        {
            return new PowerGenerationUnit
            {
                Name = powerplant.Name,
                Type = powerplant.Type,
                Efficiency = powerplant.Efficiency,
                PMin = Math.Round(powerplant.PMin * powerplant.Efficiency, 1, MidpointRounding.ToZero),
                PMax = Math.Round(powerplant.PMax * powerplant.Efficiency, 1, MidpointRounding.ToZero),
                ProductionCostPerUnit = fuelPricePerMwh / powerplant.Efficiency
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
