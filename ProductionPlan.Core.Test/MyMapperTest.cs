using ProductionPlan.Core.Mappers;
using ProductionPlan.Core.Models;
using ProductionPlan.Core.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductionPlan.Core.Test
{
    public class MyMapperTest
    {
        public MyMapperTest()
        {
        }

        [Fact]
        public void ToWindPowerGenerationUnit_MustReturnAPowerGenerationUnitWithGoodValues()
        {
            Powerplant powerplant = new Powerplant
            {
                Name = "powerplantName",
                Type = PowerplantTypeEnum.windturbine,
                Efficiency = 1,
                PMin = 10,
                PMax = 15,
            };
            decimal wind = 50;

            var res = MyMapper.ToWindPowerGenerationUnit(powerplant, wind);

            Assert.Equal(powerplant.Name, res.Name);
            Assert.Equal(powerplant.Type, res.Type);
            Assert.Equal(powerplant.Efficiency, res.Efficiency);
            Assert.Equal(7.5M, res.PMin );
            Assert.Equal(7.5M, res.PMax );
            Assert.Equal(0, res.ProductionCostPerMwh );
        }

        [Fact]
        public void ToWindPowerGenerationUnit_MustThrowAnArgumentException_IfPowerplantTypeIsNotWind()
        {
            Powerplant powerplant = new Powerplant
            {
                Name = "powerplantName",
                Type = PowerplantTypeEnum.gasfired,
                Efficiency = 0.5M,
                PMin = 10,
                PMax = 15,
            };
            decimal wind = 50;

            var exception = Assert.Throws<ArgumentException>(() => MyMapper.ToWindPowerGenerationUnit(powerplant, wind));
            Assert.Equal("Cannot map to (wind)PowerGenerationUnit because powerplant type is not wind", exception.Message) ;
        }

        [Fact]
        public void ToFuelPowerGenerationUnit_MustReturnAPowerGenerationUnitWithGoodValuesAndCo2CalculatedInCosts_IfPowerplantTypeIsGasfired()
        {
            Powerplant powerplant = new Powerplant
            {
                Name = "powerplantName",
                Type = PowerplantTypeEnum.gasfired,
                Efficiency = 0.5M,
                PMin = 10,
                PMax = 15,
            };
            decimal fuelPricePerMwh = 50;
            decimal co2Price = 10;

            var res = MyMapper.ToFuelPowerGenerationUnit(powerplant, fuelPricePerMwh, co2Price);

            Assert.Equal(powerplant.Name, res.Name);
            Assert.Equal(powerplant.Type, res.Type);
            Assert.Equal(powerplant.Efficiency, res.Efficiency);
            Assert.Equal(5M, res.PMin );
            Assert.Equal(7.5M, res.PMax );
            Assert.Equal(103, res.ProductionCostPerMwh );
        }

        [Fact]
        public void ToFuelPowerGenerationUnit_MustReturnAPowerGenerationUnitWithGoodValuesAndCo2NotCalculatedInCosts_IfPowerplantTypeIsTurbojet()
        {
            Powerplant powerplant = new Powerplant
            {
                Name = "powerplantName",
                Type = PowerplantTypeEnum.turbojet,
                Efficiency = 0.5M,
                PMin = 10,
                PMax = 15,
            };
            decimal fuelPricePerMwh = 50;
            decimal co2Price = 10;

            var res = MyMapper.ToFuelPowerGenerationUnit(powerplant, fuelPricePerMwh, co2Price);

            Assert.Equal(powerplant.Name, res.Name);
            Assert.Equal(powerplant.Type, res.Type);
            Assert.Equal(powerplant.Efficiency, res.Efficiency);
            Assert.Equal(5M, res.PMin );
            Assert.Equal(7.5M, res.PMax );
            Assert.Equal(100, res.ProductionCostPerMwh );
        }

        [Fact]
        public void ToFuelPowerGenerationUnit_MustThrowAnArgumentException_IfPowerplantTypeIsWind()
        {
            Powerplant powerplant = new Powerplant
            {
                Name = "powerplantName",
                Type = PowerplantTypeEnum.windturbine,
                Efficiency = 0.5M,
                PMin = 10,
                PMax = 15,
            };
            decimal fuelPricePerMwh = 50;
            decimal co2Price = 10;

            var exception = Assert.Throws<ArgumentException>(() => MyMapper.ToFuelPowerGenerationUnit(powerplant, fuelPricePerMwh, co2Price));
            Assert.Equal("Cannot map to (fuel)PowerGenerationUnit because powerplant type is not a fuel type", exception.Message);
        }

    }
}
