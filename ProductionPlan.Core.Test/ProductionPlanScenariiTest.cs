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
    public class ProductionPlanScenariiTest
    {
        private readonly ProductionService _service;
        private Fuel _baseEnergyMetrics;
        public ProductionPlanScenariiTest()
        {
            var logger = new Mock<ILogger<ProductionService>>();
            _service = new ProductionService(logger.Object);
            _baseEnergyMetrics = new Fuel() { Co2 = 20, Kerosine = 50, Gas = 15, Wind = 50 };

        }

        [Fact]
        public void ProductionPlan_CannotProvideLoad_NotEnough()
        {
            Payload productionPlan = new Payload
            {
                Load = 500,
                Fuels = _baseEnergyMetrics,
                Powerplants = new List<Powerplant>
                {
                new Powerplant{ Name = "Gas1", Type = PowerplantTypeEnum.gasfired , Efficiency = 0.5M, PMin = 50, PMax = 100 },
                new Powerplant{ Name = "Gas2", Type = PowerplantTypeEnum.gasfired , Efficiency = 0.5M, PMin = 50, PMax = 100 }
                }
            };


            Assert.Throws<ArgumentException>(() => _service.PlanProduction(productionPlan));
        }

        [Fact]
        public void ProductionPlan_CannotProvideLoad_TooMuch()
        {
            Payload productionPlan = new Payload
            {
                Load = 20,
                Fuels = _baseEnergyMetrics,
                Powerplants = new List<Powerplant>
                {
                new Powerplant{ Name = "Gas1", Type = PowerplantTypeEnum.gasfired , Efficiency = 0.5M, PMin = 50, PMax = 100 },
                new Powerplant{ Name = "Wind1", Type = PowerplantTypeEnum.windturbine , Efficiency = 1, PMin = 0, PMax = 50 }
                }
            };

            Assert.Throws<ArgumentException>(() => _service.PlanProduction(productionPlan));
        }

        [Fact]
        public void ProductionPlan_Wind_Enough()
        {
            Payload productionPlan = new Payload
            {
                Load = 25,
                Fuels = _baseEnergyMetrics,
                Powerplants = new List<Powerplant>
                {
                new Powerplant{ Name = "Gas1", Type = PowerplantTypeEnum.gasfired , Efficiency = 0.5M, PMin = 10, PMax = 100 },
                new Powerplant{ Name = "Wind1", Type = PowerplantTypeEnum.windturbine , Efficiency = 1, PMin = 0, PMax = 50 }
                }
            };


            var result = _service.PlanProduction(productionPlan).ToList();

            Assert.Equal(25, result.First(x => x.Name == "Wind1").P);
            Assert.Equal(0, result.First(x => x.Name == "Gas1").P);
        }

        [Fact]
        public void PlanProduction_Wind_NotEnough()
        {
            Payload productionPlan = new Payload
            {
                Load = 50,
                Fuels = _baseEnergyMetrics,
                Powerplants = new List<Powerplant>
                {
                new Powerplant{ Name = "Gas1", Type = PowerplantTypeEnum.gasfired , Efficiency = 0.5M, PMin = 10, PMax = 100 },
                new Powerplant{ Name = "Wind1", Type = PowerplantTypeEnum.windturbine , Efficiency = 1, PMin = 0, PMax = 50 }
                }
            };

            var result = _service.PlanProduction(productionPlan).ToList();

            Assert.Equal(25, result.First(x => x.Name == "Wind1").P);
            Assert.Equal(25, result.First(x => x.Name == "Gas1").P);
        }

        [Fact]
        public void PlanProduction_Wind_TooMuch()
        {
            Payload productionPlan = new Payload
            {
                Load = 20,
                Fuels = _baseEnergyMetrics,
                Powerplants = new List<Powerplant>
                {
                new Powerplant{ Name = "Gas1", Type = PowerplantTypeEnum.gasfired , Efficiency = 0.5M, PMin = 10, PMax = 100 },
                new Powerplant{ Name = "Wind1", Type = PowerplantTypeEnum.windturbine , Efficiency = 1, PMin = 0, PMax = 50 }
                }
            };

            var result = _service.PlanProduction(productionPlan).ToList();

            Assert.Equal(0, result.First(x => x.Name == "Wind1").P);
            Assert.Equal(20, result.First(x => x.Name == "Gas1").P);
        }

        [Fact]
        public void PlanProduction_Gas_Efficiency()
        {
            Payload productionPlan = new Payload
            {
                Load = 20,
                Fuels = _baseEnergyMetrics,
                Powerplants = new List<Powerplant>
                {
                new Powerplant{ Name = "Gas1", Type = PowerplantTypeEnum.gasfired , Efficiency = 0.5M, PMin = 10, PMax = 100 },
                new Powerplant{ Name = "Gas2", Type = PowerplantTypeEnum.gasfired , Efficiency = 0.6M, PMin = 10, PMax = 100 },
                new Powerplant{ Name = "Gas3", Type = PowerplantTypeEnum.gasfired , Efficiency = 0.8M, PMin = 10, PMax = 100 },
                new Powerplant{ Name = "Gas4", Type = PowerplantTypeEnum.gasfired , Efficiency = 0.3M, PMin = 10, PMax = 100 },
                new Powerplant{ Name = "Gas5", Type = PowerplantTypeEnum.gasfired , Efficiency = 0.45M, PMin = 10, PMax = 100 },
                }
            };

            var result = _service.PlanProduction(productionPlan).ToList();

            Assert.Equal(20, result.First(x => x.Name == "Gas3").P);
            Assert.Equal(0, result.Where(x => x.Name != "Gas3").Select(x => x.P).Sum());
        }

        [Fact]
        public void PlanProduction_Gas_AllNeeded()
        {
            Payload productionPlan = new Payload
            {
                Load = 490,
                Fuels = _baseEnergyMetrics,
                Powerplants = new List<Powerplant>
                {
                new Powerplant{ Name = "Gas1", Type = PowerplantTypeEnum.gasfired , Efficiency = 0.5M, PMin = 10, PMax = 100 },
                new Powerplant{ Name = "Gas2", Type = PowerplantTypeEnum.gasfired , Efficiency = 0.6M, PMin = 10, PMax = 100 },
                new Powerplant{ Name = "Gas3", Type = PowerplantTypeEnum.gasfired , Efficiency = 0.8M, PMin = 10, PMax = 100 },
                new Powerplant{ Name = "Gas4", Type = PowerplantTypeEnum.gasfired , Efficiency = 0.3M, PMin = 10, PMax = 100 },
                new Powerplant{ Name = "Gas5", Type = PowerplantTypeEnum.gasfired , Efficiency = 0.45M, PMin = 10, PMax = 100 },
                }
            };

            var result = _service.PlanProduction(productionPlan).ToList();

            Assert.Equal(100, result.First(x => x.Name == "Gas1").P);
            Assert.Equal(100, result.First(x => x.Name == "Gas2").P);
            Assert.Equal(100, result.First(x => x.Name == "Gas3").P);
            Assert.Equal(90, result.First(x => x.Name == "Gas4").P);
            Assert.Equal(100, result.First(x => x.Name == "Gas5").P);
        }
    }
}
