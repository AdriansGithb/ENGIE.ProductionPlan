using Microsoft.Extensions.Logging;
using ProductionPlan.Core.Abstract;
using ProductionPlan.Core.Mappers;
using ProductionPlan.Core.Models;
using ProductionPlan.Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductionPlan.Core.Services
{
    public class ProductionService : IProductionService
    {
        private readonly ILogger<ProductionService> _logger;

        public ProductionService(ILogger<ProductionService> logger)
        {
            _logger = logger;
            _logger.LogDebug(1, "NLog injected into ProductionService");
        }

        public IEnumerable<PlannedProductionPowerplant> PlanProduction(Payload payload)
        {
            try
            {
                if (payload.Powerplants is null || payload.Powerplants.Count() == 0)
                {
                    _logger.LogInformation("Return empty list because of empty powerplants list received");
                    return new List<PlannedProductionPowerplant>();
                }

                var target = (decimal)payload.Load.Amount;
                if(target < 0)
                {
                    _logger.LogError("Received load is less than 0.");
                     throw new ArgumentException("Payload is less than zero.");
               }

                var powerUnits = GetPowerGenerationUnits(payload);

                // target load is higher than max producible power
                if( target > powerUnits.Sum(pu => pu.PMax))
                {
                    _logger.LogError("Target load is higher than maximum producible power");
                    return PlanMaximalProduction(powerUnits);
                }
                //target load is equal to max producible power
                else if( target == powerUnits.Sum(pu => pu.PMax))
                {
                    _logger.LogInformation("Target load is equal to maximum producible power");
                    return PlanMaximalProduction(powerUnits);
                }
                //target load is less than max producible power
                else
                {
                    _logger.LogInformation("Target load is less than maximum producible power");
                    return PlanProductionByMeritOrder(powerUnits, target);
                }
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw ex;
            }
        }


        private IEnumerable<PlannedProductionPowerplant> PlanProductionByMeritOrder(List<PowerGenerationUnit> powerGenerationUnits, decimal target)
        {
            // sortir les productions nulles
            var nullPowerGenerationUnits = powerGenerationUnits.Where(pu => pu.PMax == 0).ToList();
            powerGenerationUnits.RemoveAll(pu => pu.PMax == 0);
            //sort list by merit order
            powerGenerationUnits = powerGenerationUnits.OrderBy(item => item.ProductionCostPerUnit).ThenBy(item => item.PMin).ThenByDescending(item => item.PMax).ToList();
            //get full possible combinations list
            var possibleCombinations = (from combination in GetAllPossibleCombinations(powerGenerationUnits)
                          where combination.Sum(c => c.PMax) >= target && combination.Sum(c => c.PMin) <= target
                          select combination.ToList()).ToList();

            var bestPowerGeneration = GetBestPossibleCombination(possibleCombinations, target).ToList();
            if (nullPowerGenerationUnits != null && nullPowerGenerationUnits.Count > 0)
                bestPowerGeneration.AddRange(nullPowerGenerationUnits);
            return bestPowerGeneration.Select(pu => pu.ToPlannedProductionPowerplant());

        }
        private IEnumerable<PlannedProductionPowerplant> PlanMaximalProduction(List<PowerGenerationUnit> powerGenerationUnits)
        {
            List<PlannedProductionPowerplant> plannedProcutionList = new List<PlannedProductionPowerplant>();
            foreach (var unit in powerGenerationUnits)
            {
                unit.AdvisedProduction = unit.PMax;
                plannedProcutionList.Add(unit.ToPlannedProductionPowerplant());
            }
            return plannedProcutionList;
        }
        private List<PowerGenerationUnit> GetPowerGenerationUnits(Payload payload)
        {
            List<PowerGenerationUnit> units = new List<PowerGenerationUnit>();
            foreach(var pwp in payload.Powerplants)
            {
                var unit = pwp.Type switch
                {
                    PowerplantTypeEnum.windturbine => pwp.ToWindPowerGenerationUnit(payload.Fuels.Wind),
                    PowerplantTypeEnum.gasfired => pwp.ToFuelPowerGenerationUnit(payload.Fuels.Gas),
                    PowerplantTypeEnum.turbojet => pwp.ToFuelPowerGenerationUnit(payload.Fuels.Kerosine),
                    _ => throw new ArgumentException($"Powerplant type not configured : {pwp.Type}")
                };
                units.Add(unit);
            }
            return units;
        }

        private IEnumerable<IEnumerable<PowerGenerationUnit>> GetAllPossibleCombinations(IEnumerable<PowerGenerationUnit> availablePowerUnits)
        {
            if (!availablePowerUnits.Any())
                return Enumerable.Repeat(Enumerable.Empty<PowerGenerationUnit>(), 1);

            var unit = availablePowerUnits.Take(1);
            var nextUnits = GetAllPossibleCombinations(availablePowerUnits.Skip(1));
            var possibleUnitsCombination = nextUnits.Select(set => unit.Concat(set));
            return possibleUnitsCombination.Concat(nextUnits);
        }

        private List<PowerGenerationUnit> GetBestPossibleCombination(List<List<PowerGenerationUnit>> allPossibleCombinations, decimal target)
        {
            foreach (var combination in allPossibleCombinations)
            {
                var possibleCombination = new List<PowerGenerationUnit>();
                var minMandatory = combination.Sum(x => x.PMin);
                var tempTarget = target;
                foreach(var powerUnit in combination)
                {
                    minMandatory -= Math.Round(powerUnit.PMin, 1, MidpointRounding.ToZero);
                    decimal residue = Math.Round(tempTarget - minMandatory, 1, MidpointRounding.ToZero);
                    decimal powerToUse = 0;
                    if (Math.Round(powerUnit.PMax, 1, MidpointRounding.ToZero) >= residue)
                        powerToUse = residue;
                    else
                        powerToUse = Math.Round(powerUnit.PMax, 1, MidpointRounding.ToZero);

                    powerUnit.AdvisedProduction = powerToUse;
                    tempTarget -= powerToUse;
                }
            }

            return allPossibleCombinations
                .OrderBy(combination => 
                combination.Sum(pu => 
                pu.AdvisedProduction * pu.ProductionCostPerUnit))
                .First()
                .ToList();
        }


    }
}
