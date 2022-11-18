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

        [Fact]
        public void PlanProduction_Gas_Pmin()
        {
            Payload productionPlan = new Payload
            {
                Load = 125,
                Fuels = _baseEnergyMetrics,
                Powerplants = new List<Powerplant>
                {
                new Powerplant{ Name = "Wind1", Type = PowerplantTypeEnum.windturbine , Efficiency = 1, PMin = 0, PMax = 50 },
                new Powerplant{ Name = "Gas1", Type = PowerplantTypeEnum.gasfired , Efficiency = 0.5M, PMin = 110, PMax = 200 },
                new Powerplant{ Name = "Gas2", Type = PowerplantTypeEnum.gasfired , Efficiency = 0.8M, PMin = 80, PMax = 150 },
                }
            };

            var result = _service.PlanProduction(productionPlan).ToList();

            Assert.Equal(100, result.First(x => x.Name == "Gas2").P);
            Assert.Equal(0, result.First(x => x.Name == "Gas1").P);
        }

        [Fact]
        public void PlanProduction_Kerosine()
        {
            Payload productionPlan = new Payload
            {
                Load = 100,
                Fuels = _baseEnergyMetrics,
                Powerplants = new List<Powerplant>
                {
                new Powerplant{ Name = "Wind1", Type = PowerplantTypeEnum.windturbine , Efficiency = 1, PMin = 0, PMax = 150 },
                new Powerplant{ Name = "Gas1", Type = PowerplantTypeEnum.gasfired , Efficiency = 0.5M, PMin = 100, PMax = 200 },
                new Powerplant{ Name = "Kerosine1", Type = PowerplantTypeEnum.turbojet , Efficiency = 0.5M, PMin = 0, PMax = 200 },
                }
            };

            var result = _service.PlanProduction(productionPlan).ToList();

            Assert.Equal(0, result.First(x => x.Name == "Gas1").P);
            Assert.Equal(25, result.First(x => x.Name == "Kerosine1").P);
        }

        [Fact]
        public void PlanProduction_CO2Impact()
        {
            Payload productionPlan = new Payload
            {
                Load = 150,
                Fuels = _baseEnergyMetrics,
                Powerplants = new List<Powerplant>
                {
                new Powerplant{ Name = "Gas1", Type = PowerplantTypeEnum.gasfired , Efficiency = 0.3M, PMin = 100, PMax = 200 },
                new Powerplant{ Name = "Kerosine1", Type = PowerplantTypeEnum.turbojet , Efficiency = 1M, PMin = 0, PMax = 200 },
                }
            };

            var result = _service.PlanProduction(productionPlan).ToList();

            Assert.Equal(150, result.First(x => x.Name == "Kerosine1").P);
        }

        [Fact]
        public void PlanProduction_TrickyTest1()
        {
            Fuel energyMetrics = new Fuel { Co2 = 0, Kerosine = 50.8M, Gas = 20, Wind = 100 };
            Payload productionPlan = new Payload
            {
                Load = 60,
                Fuels = energyMetrics,
                Powerplants = new List<Powerplant>
                {
                new Powerplant{ Name = "windpark1", Type = PowerplantTypeEnum.windturbine , Efficiency = 1, PMin = 0, PMax = 20 },
                new Powerplant{ Name = "gasfired", Type = PowerplantTypeEnum.gasfired , Efficiency = 0.9M, PMin = 50, PMax = 100 },
                new Powerplant{ Name = "gasfiredinefficient", Type = PowerplantTypeEnum.gasfired , Efficiency = 0.1M, PMin = 0, PMax = 100 },
                }
            };

            var result = _service.PlanProduction(productionPlan).ToList();

            Assert.Equal(60, result.Select(x => x.P).Sum());
            Assert.Equal(0, result.First(x => x.Name == "windpark1").P);
            Assert.Equal(60, result.First(x => x.Name == "gasfired").P);
            Assert.Equal(0, result.First(x => x.Name == "gasfiredinefficient").P);
        }

        [Fact]
        public void PlanProduction_TrickyTest2()
        {
            Fuel energyMetrics = new Fuel { Co2 = 0, Kerosine = 50.8M, Gas = 20, Wind = 100 };
            Payload productionPlan = new Payload
            {
                Load = 80,
                Fuels = energyMetrics,
                Powerplants = new List<Powerplant>
                {
                new Powerplant{ Name = "windpark1", Type = PowerplantTypeEnum.windturbine , Efficiency = 1, PMin = 0, PMax = 60 },
                new Powerplant{ Name = "gasfired", Type = PowerplantTypeEnum.gasfired , Efficiency = 0.9M, PMin = 50, PMax = 100 },
                new Powerplant{ Name = "gasfiredinefficient", Type = PowerplantTypeEnum.gasfired , Efficiency = 0.1M, PMin = 0, PMax = 200 },
                }
            };

            var result = _service.PlanProduction(productionPlan).ToList();

            Assert.Equal(80, result.Select(x => x.P).Sum());
            Assert.Equal(0, result.First(x => x.Name == "windpark1").P);
            Assert.Equal(80, result.First(x => x.Name == "gasfired").P);
            Assert.Equal(0, result.First(x => x.Name == "gasfiredinefficient").P);
        }

        [Fact]
        public void PlanProduction_ExamplePayload1_NoCO2()
        {
            Fuel energyMetrics = new Fuel{ Co2 = 0, Kerosine = 50.8M, Gas = 13.4M, Wind = 60 };
            Payload productionPlan = new Payload
            {
                Load = 480,
                Fuels = energyMetrics,
                Powerplants = new List<Powerplant>
                {
                new Powerplant{ Name = "gasfiredbig1", Type = PowerplantTypeEnum.gasfired , Efficiency = 0.53M, PMin = 100, PMax = 460 },
                new Powerplant{ Name = "gasfiredbig2", Type = PowerplantTypeEnum.gasfired , Efficiency = 0.53M, PMin = 100, PMax = 460 },
                new Powerplant{ Name = "gasfiredsomewhatsmaller", Type = PowerplantTypeEnum.gasfired , Efficiency = 0.37M, PMin = 40, PMax = 210 },
                new Powerplant{ Name = "tj1", Type = PowerplantTypeEnum.turbojet , Efficiency = 0.3M, PMin = 0, PMax = 16 },
                new Powerplant{ Name = "windpark1", Type = PowerplantTypeEnum.windturbine , Efficiency = 1, PMin = 0, PMax = 150 },
                new Powerplant{ Name = "windpark2", Type = PowerplantTypeEnum.windturbine , Efficiency = 1, PMin = 0, PMax = 36 },
                }
            };

            var result = _service.PlanProduction(productionPlan).ToList();

            Assert.Equal(480, result.Select(x => x.P).Sum());
            Assert.Equal(90, result.First(x => x.Name == "windpark1").P);
            Assert.Equal(21.6M, result.First(x => x.Name == "windpark2").P);
            Assert.Equal(368.4M, result.First(x => x.Name == "gasfiredbig1").P);
            Assert.Equal(0, result.First(x => x.Name == "gasfiredbig2").P);
            Assert.Equal(0, result.First(x => x.Name == "gasfiredsomewhatsmaller").P);
            Assert.Equal(0, result.First(x => x.Name == "tj1").P);
        }

        [Fact]
        public void PlanProduction_ExamplePayload2_NoCO2()
        {
            Fuel energyMetrics = new Fuel{ Co2 = 0, Kerosine = 50.8M, Gas = 13.4M, Wind = 0 };
            Payload productionPlan = new Payload
            {
                Load = 480,
                Fuels = energyMetrics,
                Powerplants = new List<Powerplant>
                {
                new Powerplant{ Name = "gasfiredbig1", Type = PowerplantTypeEnum.gasfired , Efficiency = 0.53M, PMin = 100, PMax = 460 },
                new Powerplant{ Name = "gasfiredbig2", Type = PowerplantTypeEnum.gasfired , Efficiency = 0.53M, PMin = 100, PMax = 460 },
                new Powerplant{ Name = "gasfiredsomewhatsmaller", Type = PowerplantTypeEnum.gasfired , Efficiency = 0.37M, PMin = 40, PMax = 210 },
                new Powerplant{ Name = "tj1", Type = PowerplantTypeEnum.turbojet , Efficiency = 0.3M, PMin = 0, PMax = 16 },
                new Powerplant{ Name = "windpark1", Type = PowerplantTypeEnum.windturbine , Efficiency = 1, PMin = 0, PMax = 150 },
                new Powerplant{ Name = "windpark2", Type = PowerplantTypeEnum.windturbine , Efficiency = 1, PMin = 0, PMax = 36 },
                }
            };

            var result = _service.PlanProduction(productionPlan).ToList();

            Assert.Equal(480, result.Select(x => x.P).Sum());
            Assert.Equal(0, result.First(x => x.Name == "windpark1").P);
            Assert.Equal(0, result.First(x => x.Name == "windpark2").P);
            Assert.Equal(380, result.First(x => x.Name == "gasfiredbig1").P);
            Assert.Equal(100, result.First(x => x.Name == "gasfiredbig2").P);
            Assert.Equal(0, result.First(x => x.Name == "gasfiredsomewhatsmaller").P);
            Assert.Equal(0, result.First(x => x.Name == "tj1").P);
        }

        [Fact]
        public void PlanProduction_ExamplePayload3_NoCO2()
        {
            Fuel energyMetrics = new Fuel{ Co2 = 0, Kerosine = 50.8M, Gas = 13.4M, Wind = 60 };
            Payload productionPlan = new Payload
            {
                Load = 910,
                Fuels = energyMetrics,
                Powerplants = new List<Powerplant>
                {
                new Powerplant{ Name = "gasfiredbig1", Type = PowerplantTypeEnum.gasfired , Efficiency = 0.53M, PMin = 100, PMax = 460 },
                new Powerplant{ Name = "gasfiredbig2", Type = PowerplantTypeEnum.gasfired , Efficiency = 0.53M, PMin = 100, PMax = 460 },
                new Powerplant{ Name = "gasfiredsomewhatsmaller", Type = PowerplantTypeEnum.gasfired , Efficiency = 0.37M, PMin = 40, PMax = 210 },
                new Powerplant{ Name = "tj1", Type = PowerplantTypeEnum.turbojet , Efficiency = 0.3M, PMin = 0, PMax = 16 },
                new Powerplant{ Name = "windpark1", Type = PowerplantTypeEnum.windturbine , Efficiency = 1, PMin = 0, PMax = 150 },
                new Powerplant{ Name = "windpark2", Type = PowerplantTypeEnum.windturbine , Efficiency = 1, PMin = 0, PMax = 36 },
                }
            };

            var result = _service.PlanProduction(productionPlan).ToList();

            Assert.Equal(910, result.Select(x => x.P).Sum());
            Assert.Equal(90, result.First(x => x.Name == "windpark1").P);
            Assert.Equal(21.6M, result.First(x => x.Name == "windpark2").P);
            Assert.Equal(460, result.First(x => x.Name == "gasfiredbig1").P);
            Assert.Equal(338.4M, result.First(x => x.Name == "gasfiredbig2").P);
            Assert.Equal(0, result.First(x => x.Name == "gasfiredsomewhatsmaller").P);
            Assert.Equal(0, result.First(x => x.Name == "tj1").P);
        }

        [Fact]
        public void PlanProduction_ExamplePayload1_WithCO2()
        {
            Fuel energyMetrics = new Fuel{ Co2 = 20, Kerosine = 50.8M, Gas = 13.4M, Wind = 60 };
            Payload productionPlan = new Payload
            {
                Load = 480,
                Fuels = energyMetrics,
                Powerplants = new List<Powerplant>
                {
                new Powerplant{ Name = "gasfiredbig1", Type = PowerplantTypeEnum.gasfired , Efficiency = 0.53M, PMin = 100, PMax = 460 },
                new Powerplant{ Name = "gasfiredbig2", Type = PowerplantTypeEnum.gasfired , Efficiency = 0.53M, PMin = 100, PMax = 460 },
                new Powerplant{ Name = "gasfiredsomewhatsmaller", Type = PowerplantTypeEnum.gasfired , Efficiency = 0.37M, PMin = 40, PMax = 210 },
                new Powerplant{ Name = "tj1", Type = PowerplantTypeEnum.turbojet , Efficiency = 0.3M, PMin = 0, PMax = 16 },
                new Powerplant{ Name = "windpark1", Type = PowerplantTypeEnum.windturbine , Efficiency = 1, PMin = 0, PMax = 150 },
                new Powerplant{ Name = "windpark2", Type = PowerplantTypeEnum.windturbine , Efficiency = 1, PMin = 0, PMax = 36 },
                }
            };

            var result = _service.PlanProduction(productionPlan).ToList();

            Assert.Equal(480, result.Select(x => x.P).Sum());
            Assert.Equal(90, result.First(x => x.Name == "windpark1").P);
            Assert.Equal(21.6M, result.First(x => x.Name == "windpark2").P);
            Assert.Equal(268.4M, result.First(x => x.Name == "gasfiredbig1").P);
            Assert.Equal(100, result.First(x => x.Name == "gasfiredbig2").P);
            Assert.Equal(0, result.First(x => x.Name == "gasfiredsomewhatsmaller").P);
            Assert.Equal(0, result.First(x => x.Name == "tj1").P);
        }

        [Fact]
        public void PlanProduction_ExamplePayload2_WithCO2()
        {
            Fuel energyMetrics = new Fuel{ Co2 = 20, Kerosine = 50.8M, Gas = 13.4M, Wind = 0 };
            Payload productionPlan = new Payload
            {
                Load = 480,
                Fuels = energyMetrics,
                Powerplants = new List<Powerplant>
                {
                new Powerplant{ Name = "gasfiredbig1", Type = PowerplantTypeEnum.gasfired , Efficiency = 0.53M, PMin = 100, PMax = 460 },
                new Powerplant{ Name = "gasfiredbig2", Type = PowerplantTypeEnum.gasfired , Efficiency = 0.53M, PMin = 100, PMax = 460 },
                new Powerplant{ Name = "gasfiredsomewhatsmaller", Type = PowerplantTypeEnum.gasfired , Efficiency = 0.37M, PMin = 40, PMax = 210 },
                new Powerplant{ Name = "tj1", Type = PowerplantTypeEnum.turbojet , Efficiency = 0.3M, PMin = 0, PMax = 16 },
                new Powerplant{ Name = "windpark1", Type = PowerplantTypeEnum.windturbine , Efficiency = 1, PMin = 0, PMax = 150 },
                new Powerplant{ Name = "windpark2", Type = PowerplantTypeEnum.windturbine , Efficiency = 1, PMin = 0, PMax = 36 },
                }
            };

            var result = _service.PlanProduction(productionPlan).ToList();

            Assert.Equal(480, result.Select(x => x.P).Sum());
            Assert.Equal(0, result.First(x => x.Name == "windpark1").P);
            Assert.Equal(0, result.First(x => x.Name == "windpark2").P);
            Assert.Equal(380, result.First(x => x.Name == "gasfiredbig1").P);
            Assert.Equal(100, result.First(x => x.Name == "gasfiredbig2").P);
            Assert.Equal(0, result.First(x => x.Name == "gasfiredsomewhatsmaller").P);
            Assert.Equal(0, result.First(x => x.Name == "tj1").P);
        }

        [Fact]
        public void PlanProduction_ExamplePayload3_WithCO2()
        {
            Fuel energyMetrics = new Fuel{ Co2 = 20, Kerosine = 50.8M, Gas = 13.4M, Wind = 60 };
            Payload productionPlan = new Payload
            {
                Load = 910,
                Fuels = energyMetrics,
                Powerplants = new List<Powerplant>
                {
                new Powerplant{ Name = "gasfiredbig1", Type = PowerplantTypeEnum.gasfired , Efficiency = 0.53M, PMin = 100, PMax = 460 },
                new Powerplant{ Name = "gasfiredbig2", Type = PowerplantTypeEnum.gasfired , Efficiency = 0.53M, PMin = 100, PMax = 460 },
                new Powerplant{ Name = "gasfiredsomewhatsmaller", Type = PowerplantTypeEnum.gasfired , Efficiency = 0.37M, PMin = 40, PMax = 210 },
                new Powerplant{ Name = "tj1", Type = PowerplantTypeEnum.turbojet , Efficiency = 0.3M, PMin = 0, PMax = 16 },
                new Powerplant{ Name = "windpark1", Type = PowerplantTypeEnum.windturbine , Efficiency = 1, PMin = 0, PMax = 150 },
                new Powerplant{ Name = "windpark2", Type = PowerplantTypeEnum.windturbine , Efficiency = 1, PMin = 0, PMax = 36 },
                }
            };

            var result = _service.PlanProduction(productionPlan).ToList();

            Assert.Equal(910, result.Select(x => x.P).Sum());
            Assert.Equal(90, result.First(x => x.Name == "windpark1").P);
            Assert.Equal(21.6M, result.First(x => x.Name == "windpark2").P);
            Assert.Equal(460, result.First(x => x.Name == "gasfiredbig1").P);
            Assert.Equal(338.4M, result.First(x => x.Name == "gasfiredbig2").P);
            Assert.Equal(0, result.First(x => x.Name == "gasfiredsomewhatsmaller").P);
            Assert.Equal(0, result.First(x => x.Name == "tj1").P);
        }

    }
}
