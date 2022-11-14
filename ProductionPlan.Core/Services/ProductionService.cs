using Microsoft.Extensions.Logging;
using ProductionPlan.Core.Abstract;
using ProductionPlan.Core.Models;
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



                if( target >= payload.Powerplants.Sum(pwp => pwp.PMax))
                {

                }

                // si le load est supérieur au max : renvoyer tout au max
                // sinon calculer le load
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw ex;
            }
        }

        private IEnumerable<PlannedProductionPowerplant> PlanMaximalProduction(List<Powerplant>)
        {
            throw new NotImplementedException();
        }
        private IEnumerable<PowerGenerationUnit> ToPowerGenerationUnits(List<Powerplant>)
        {
            throw new NotImplementedException();
        }
    }
}
