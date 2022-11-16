using Microsoft.Extensions.Logging;
using ProductionPlan.Core.Abstract;
using ProductionPlan.Core.Mappers;
using ProductionPlan.Core.Models;
using ProductionPlan.Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
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

                var target = payload.Load;
                if(target < 0)
                {
                    //_logger.LogError("Received load is less than 0.");
                     throw new ArgumentException("Received load is less than zero");
               }

                var powerUnits = GetPowerGenerationUnits(payload);
                List<PlannedProductionPowerplant> plannedProcutionList = new List<PlannedProductionPowerplant>();
                // target load is higher than max producible power
                if( target > powerUnits.Sum(pu => pu.PMax))
                {
                    //_logger.LogError("Target load is higher than maximum producible power");
                    throw new ArgumentException("Target load is higher than maximum producible power");
                }
                //target load is equal to max producible power
                else if( target == powerUnits.Sum(pu => pu.PMax))
                {
                    _logger.LogInformation("Target load is equal to maximum producible power");
                    plannedProcutionList = PlanMaximalProduction(powerUnits).ToList();
                }
                //target load is less than max producible power
                else
                {
                    _logger.LogInformation("Target load is less than maximum producible power");
                    plannedProcutionList = PlanProductionByMeritOrder(powerUnits, target).ToList();
                }

                if (plannedProcutionList is null || plannedProcutionList.Count == 0)
                {
                    //_logger.LogError("Planned production list is empty");
                    throw new ArgumentException("Planned production list is empty");
                }
                if(plannedProcutionList.Sum(pwplnt => pwplnt.P) != target)
                {
                    //_logger.LogError("Planned total amount of power to produce is not equal to expected load");
                    throw new ArgumentException("Planned total amount of power to produce is not equal to expected load");
                }
                if(plannedProcutionList.Count != payload.Powerplants.Count())
                {
                    //_logger.LogError("Powerplants count in Planned production list is not equal to powerplants count in received Payload");
                    throw new ArgumentException("Powerplants count in Planned production list is not equal to powerplants count in received Payload");
                }

                return plannedProcutionList;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ExceptionDispatchInfo.Capture(ex).Throw();
                throw;
            }
        }


        private IEnumerable<PlannedProductionPowerplant> PlanProductionByMeritOrder(List<PowerGenerationUnit> powerGenerationUnits, decimal target)
        {
            //sort list by merit order 
            powerGenerationUnits = powerGenerationUnits
                .OrderBy(item => item.ProductionCostPerMwh).ThenBy(item => item.PMin).ThenByDescending(item => item.PMax).ToList();
            //get full possible combinations list and do not take power units with 0 producible power
            var possibleCombinations = (from combination in GetAllPossibleCombinations(powerGenerationUnits.Where(pu => pu.PMax > 0))
                          where combination.Sum(c => c.PMax) >= target && combination.Sum(c => c.PMin) <= target
                          select combination.ToList()).ToList();

            //clone objects
            for (int i = 0; i < possibleCombinations.Count; i++)
            {
                for (int j = 0; j < possibleCombinations[i].Count; j++)
                {
                    var clone = possibleCombinations[i][j].DeepCopy();
                    possibleCombinations[i][j] = clone;
                }
            }

            var bestPowerGeneration = GetBestPossibleCombination(possibleCombinations, target).ToList();
            foreach(var powerGenerationUnit in powerGenerationUnits)
            {
                var computedUnit = bestPowerGeneration.FirstOrDefault(pu => pu.Name.Equals(powerGenerationUnit.Name));
                powerGenerationUnit.AdvisedProduction = computedUnit is null ? 0 : computedUnit.AdvisedProduction;
            }
            return powerGenerationUnits.Select(pu => pu.ToPlannedProductionPowerplant());

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
                    PowerplantTypeEnum.gasfired => pwp.ToFuelPowerGenerationUnit(payload.Fuels.Gas, payload.Fuels.Co2),
                    PowerplantTypeEnum.turbojet => pwp.ToFuelPowerGenerationUnit(payload.Fuels.Kerosine, payload.Fuels.Co2),
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
            return possibleUnitsCombination.Concat(nextUnits) ;
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
                    minMandatory -= powerUnit.PMin;
                    decimal residue = tempTarget - minMandatory;
                    decimal powerToUse = 0;
                    if (powerUnit.PMax >= residue)
                        powerToUse = residue;
                    else
                        powerToUse = powerUnit.PMax;

                    powerUnit.AdvisedProduction = powerToUse;
                    tempTarget -= powerToUse;
                }
            }

            //foreach (var list in allPossibleCombinations)
            //{
            //    Console.WriteLine($"Somme des couts : {list.Sum(pu => pu.AdvisedProduction * pu.ProductionCostPerUnit)}");
            //    Console.WriteLine($"Somme des centrales dans la liste : {list.Count}");
            //    Console.WriteLine($"Somme des centrales utilisées : {list.Count(powerUnit => powerUnit.AdvisedProduction > 0)}");
            //}

            //var res = allPossibleCombinations
            //    .OrderBy(combination =>
            //    combination.Sum(pu =>
            //    pu.AdvisedProduction * pu.ProductionCostPerUnit))
            //    .ThenBy(combination => combination.Count(powerUnit => powerUnit.AdvisedProduction > 0))
            //    .First()
            //    .ToList();
            //// penser à ajouter un tri pour sélectionner le moins de centrales possible

            //Console.WriteLine($"Cout de la combinaison sélectionnée : {res.Sum(pu => pu.AdvisedProduction * pu.ProductionCostPerUnit)}");
            //Console.WriteLine($"Somme des centrales utilisées : {res.Count(powerUnit => powerUnit.AdvisedProduction > 0)}");
            //Console.WriteLine($"Somme des centrales dans la liste : {res.Count()}");

            return allPossibleCombinations
                .OrderBy(combination =>
                combination.Sum(pu =>
                pu.AdvisedProduction * pu.ProductionCostPerMwh))
                .ThenBy(combination => combination.Count(powerUnit => powerUnit.AdvisedProduction > 0))
                .First()
                .ToList();
        }


    }
}
