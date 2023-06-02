using Microsoft.Extensions.Logging;
using Moq;
using ProductionPlan.Core.Models;
using ProductionPlan.Core.Models.Enums;
using ProductionPlan.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductionPlan.Core.Test
{
    public class ProductionServiceTest
    {
        private readonly ProductionService _service;
        public ProductionServiceTest()
        {
            var logger = new Mock<ILogger<ProductionService>>();
            _service = new ProductionService(logger.Object);
        }

        [Fact]
        public void PlanProduction_ShouldReturnAListCalculatedByMeritOrder_IfLoadIsLessToMaximumProduction()
        {
            var payload = new Payload
            {
                Load = 150,
                Fuels = new Fuel
                {
                    GasCostPerMwh = 13.4M,
                    KerosineCostPerMwh = 50.8M,
                    WindEfficiency = 100,
                    Co2CostPerTon = 20
                },
                Powerplants = new List<Powerplant>
                {
                    new Powerplant
                    {
                        Name = "gasfired",
                        Type = PowerplantTypeEnum.gasfired,
                        Efficiency = 1,
                        PMin = 100,
                        PMax = 200,
                    },
                    new Powerplant
                    {
                        Name = "windturbine",
                        Type = PowerplantTypeEnum.windturbine,
                        Efficiency = 1,
                        PMin = 0,
                        PMax = 36,
                    },
                    new Powerplant
                    {
                        Name = "windturbine2",
                        Type = PowerplantTypeEnum.windturbine,
                        Efficiency = 1,
                        PMin = 0,
                        PMax = 150,
                    },
                    new Powerplant
                    {
                        Name = "turbojet",
                        Type = PowerplantTypeEnum.turbojet,
                        Efficiency = 1,
                        PMin = 100,
                        PMax = 200,
                    },
                }
            };

            var result = _service.PlanProduction(payload);

            Assert.NotEmpty(result);
            Assert.Equal(150, result.First(r => r.Name == "windturbine2").P);

        }

        [Fact]
        public void PlanProduction_ShouldReturnAListWithEachPlannedProductionPowerplantObjectWithPEqualToPMax_IfLoadIsEqualToMaximumProduction()
        {
            var payload = new Payload
            {
                Load = 600,
                Fuels = new Fuel
                {
                    GasCostPerMwh = 10,
                    KerosineCostPerMwh = 20,
                    WindEfficiency = 100,
                    Co2CostPerTon = 10
                },
                Powerplants = new List<Powerplant>
                {
                    new Powerplant
                    {
                        Name = "gasfired",
                        Type = PowerplantTypeEnum.gasfired,
                        Efficiency = 1,
                        PMin = 100,
                        PMax = 200,
                    },
                    new Powerplant
                    {
                        Name = "windturbine",
                        Type = PowerplantTypeEnum.windturbine,
                        Efficiency = 1,
                        PMin = 100,
                        PMax = 200,
                    },
                    new Powerplant
                    {
                        Name = "turbojet",
                        Type = PowerplantTypeEnum.turbojet,
                        Efficiency = 1,
                        PMin = 100,
                        PMax = 200,
                    },
                }
            };

            var result = _service.PlanProduction(payload);

            Assert.NotEmpty(result);
            Assert.Equal(payload.Powerplants.Sum(p => p.PMax),result.Sum(p => p.P));

        }

        [Fact]
        public void PlanProduction_ShouldThrowAnException_IfLoadIsHigherThanMaximumProduction()
        {
            var payload = new Payload
            {
                Load = 10000,
                Fuels = new Fuel
                {
                    GasCostPerMwh = 10,
                    KerosineCostPerMwh = 20,
                    WindEfficiency = 50,
                    Co2CostPerTon = 10
                },
                Powerplants = new List<Powerplant>
                {
                    new Powerplant
                    {
                        Name = "gasfired",
                        Type = PowerplantTypeEnum.gasfired,
                        Efficiency = 1,
                        PMin = 100,
                        PMax = 200,
                    },
                    new Powerplant
                    {
                        Name = "windturbine",
                        Type = PowerplantTypeEnum.windturbine,
                        Efficiency = 1,
                        PMin = 100,
                        PMax = 200,
                    },
                    new Powerplant
                    {
                        Name = "turbojet",
                        Type = PowerplantTypeEnum.turbojet,
                        Efficiency = 1,
                        PMin = 100,
                        PMax = 200,
                    },
                }
            };

            var exception = Assert.ThrowsAny<ArgumentException>(() => _service.PlanProduction(payload));
            Assert.Equal("Target load is higher than maximum producible power", exception.Message);
        }

        [Fact]
        public void PlanProductionByMeritOrder_ShouldReturnIEnumerableOfPlannedProductionPowerplantsObjectWithCountEqualToSubmittedListCount()
        {
            var powerGenList = new List<PowerGenerationUnit>
                {
                    new PowerGenerationUnit
                    {
                        Name ="1",
                        Efficiency = 1,
                        PMin = 0,
                        PMax = 90,
                        ProductionCostPerMwh = 0,
                    },
                    new PowerGenerationUnit
                    {
                        Name ="2",
                        Efficiency=1,
                        PMin = 0,
                        PMax = 21.6M,
                        ProductionCostPerMwh = 0,
                    },
                    new PowerGenerationUnit
                    {
                        Name ="3",
                        Efficiency = 1,
                        PMin = 53,
                        PMax = 200,
                        ProductionCostPerMwh = 25,
                    },
                    new PowerGenerationUnit
                    {
                        Name ="4",
                        Efficiency = 1,
                        PMin = 14,
                        PMax = 200,
                        ProductionCostPerMwh = 36,
                    },
                };
            decimal target = 140;

            var resultList = _service.PlanProductionByMeritOrder(powerGenList, target);

            Assert.Equal(resultList.Count(), powerGenList.Count);
            foreach (var powerGen in powerGenList)
                Assert.Equal(1, resultList.Count(p => p.Name == powerGen.Name));
        }

        [Fact]
        public void GetPowerGenerationUnits_ShouldReturnAListOfPowerGenerationUnitObjects()
        {
            var payload = new Payload
            {
                Load = It.IsAny<decimal>(),
                Fuels = new Fuel
                {
                    GasCostPerMwh = 10,
                    KerosineCostPerMwh = 20,
                    WindEfficiency = 50,
                    Co2CostPerTon = 10
                },
                Powerplants = new List<Powerplant>
                {
                    new Powerplant
                    {
                        Name = "gasfired",
                        Type = PowerplantTypeEnum.gasfired,
                        Efficiency = 1,
                        PMin = 100,
                        PMax = 200,
                    },
                    new Powerplant
                    {
                        Name = "windturbine",
                        Type = PowerplantTypeEnum.windturbine,
                        Efficiency = 1,
                        PMin = 100,
                        PMax = 200,
                    },
                    new Powerplant
                    {
                        Name = "turbojet",
                        Type = PowerplantTypeEnum.turbojet,
                        Efficiency = 1,
                        PMin = 100,
                        PMax = 200,
                    },
                }
            };

            var result = _service.GetPowerGenerationUnits(payload);

            Assert.NotEmpty(result);
            Assert.Collection(result,
                item => Assert.Equal("gasfired", item.Name),
                item => Assert.Equal("windturbine", item.Name),
                item => Assert.Equal("turbojet", item.Name)
                );

        }

        [Fact]
        public void PlanMaximalProduction_ShouldReturnAListWithEachPlannedProductionPowerplantObjectWithPEqualToPMax()
        {
            var powerGenList = new List<PowerGenerationUnit>
            {
                new PowerGenerationUnit
                {
                    Name ="1",
                    PMax = 1,
                },
                new PowerGenerationUnit
                {
                    Name ="10",
                    PMax = 10,
                },
                new PowerGenerationUnit
                {
                    Name ="100",
                    PMax = 100,
                },
            };
            var result = _service.PlanMaximalProduction(powerGenList);

            Assert.NotEmpty(result);
            Assert.Collection(result,
                item => { Assert.Equal(powerGenList[0].Name, item.Name); Assert.Equal(powerGenList[0].PMax, item.P); },
                item => { Assert.Equal(powerGenList[1].Name, item.Name); Assert.Equal(powerGenList[1].PMax, item.P); },
                item => { Assert.Equal(powerGenList[2].Name, item.Name); Assert.Equal(powerGenList[2].PMax, item.P); }
                );

        }

        [Fact]
        public void GetAllPossibleCombinations_ShouldReturnAListOFListWithEachPowerGenerationUnitObjectCombinated()
        {
            var powerGenList = new List<PowerGenerationUnit>
            {
                new PowerGenerationUnit
                {
                    Name ="1",
                },
                new PowerGenerationUnit
                {
                    Name ="2",
                },
                new PowerGenerationUnit
                {
                    Name ="3",
                },
            };
            var result = _service.GetAllPossibleCombinations(powerGenList);

            Assert.NotEmpty(result);
            Assert.Collection(result,
                item => Assert.Collection(item,
                    subitem => Assert.Equal(powerGenList[0].Name, subitem.Name),
                    subitem => Assert.Equal(powerGenList[1].Name, subitem.Name),
                    subitem => Assert.Equal(powerGenList[2].Name, subitem.Name)
                    ),
                item => Assert.Collection(item,
                    subitem => Assert.Equal(powerGenList[0].Name, subitem.Name),
                    subitem => Assert.Equal(powerGenList[1].Name, subitem.Name)
                    ),
                item => Assert.Collection(item,
                    subitem => Assert.Equal(powerGenList[0].Name, subitem.Name),
                    subitem => Assert.Equal(powerGenList[2].Name, subitem.Name)
                    ),
                item => Assert.Collection(item,
                    subitem => Assert.Equal(powerGenList[0].Name, subitem.Name)
                    ),
                item => Assert.Collection(item,
                    subitem => Assert.Equal(powerGenList[1].Name, subitem.Name),
                    subitem => Assert.Equal(powerGenList[2].Name, subitem.Name)
                    ),
                item => Assert.Collection(item,
                    subitem => Assert.Equal(powerGenList[1].Name, subitem.Name)
                    ),
                item => Assert.Collection(item,
                    subitem => Assert.Equal(powerGenList[2].Name, subitem.Name)
                    ),
                item => Assert.True(item.Count() == 0)
                );

        }

        [Fact]
        public void GetBestPossibleCombinations_ShouldReturnAListOFListWithEachPowerGenerationUnitObjectCombinated()
        {
            var powerGenList = new List<List<PowerGenerationUnit>>
            {
                new List<PowerGenerationUnit>
                {
                    new PowerGenerationUnit
                    {
                        Name ="1",
                        PMin = 0,
                        PMax = 90,
                        ProductionCostPerMwh = 0,
                    },
                    new PowerGenerationUnit
                    {
                        Name ="2",
                        PMin = 0,
                        PMax = 21.6M,
                        ProductionCostPerMwh = 0,
                    },
                    new PowerGenerationUnit
                    {
                        Name ="3",
                        PMin = 53,
                        PMax = 200,
                        ProductionCostPerMwh = 25,
                    },
                    new PowerGenerationUnit
                    {
                        Name ="4",
                        PMin = 14,
                        PMax = 200,
                        ProductionCostPerMwh = 36,
                    },
                },
                new List<PowerGenerationUnit>
                {
                    new PowerGenerationUnit
                    {
                        Name ="1",
                        PMin = 0,
                        PMax = 90,
                        ProductionCostPerMwh = 0,
                    },
                    new PowerGenerationUnit
                    {
                        Name ="3",
                        PMin = 53,
                        PMax = 200,
                        ProductionCostPerMwh = 25,
                    },
                    new PowerGenerationUnit
                    {
                        Name ="4",
                        PMin = 14,
                        PMax = 200,
                        ProductionCostPerMwh = 36,
                    },
                },
                new List<PowerGenerationUnit>
                {
                    new PowerGenerationUnit
                    {
                        Name ="1",
                        PMin = 0,
                        PMax = 90,
                        ProductionCostPerMwh = 0,
                    },
                    new PowerGenerationUnit
                    {
                        Name ="2",
                        PMin = 0,
                        PMax = 21.6M,
                        ProductionCostPerMwh = 0,
                    },
                    new PowerGenerationUnit
                    {
                        Name ="4",
                        PMin = 14,
                        PMax = 200,
                        ProductionCostPerMwh = 36,
                    },
                },
                new List<PowerGenerationUnit>
                {
                    new PowerGenerationUnit
                    {
                        Name ="2",
                        PMin = 0,
                        PMax = 21.6M,
                        ProductionCostPerMwh = 0,
                    },
                    new PowerGenerationUnit
                    {
                        Name ="4",
                        PMin = 14,
                        PMax = 200,
                        ProductionCostPerMwh = 36,
                    },
                },
            };
            decimal target = 140;

            var result = _service.GetBestPossibleCombination(powerGenList, target);

            Assert.NotEmpty(result);
            Assert.Collection(result,
                item => Assert.Equal("1",item.Name),
                item => Assert.Equal("2",item.Name),
                item => Assert.Equal("4",item.Name)
                );

        }


    }
}
