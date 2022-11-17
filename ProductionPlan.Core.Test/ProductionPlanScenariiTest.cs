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

    }
}
