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

        public void PlanProduction(Payload payload)
        {
            try
            {
                // créer un response powerplant
                // créer un powerplant pour le calcul de prod
                // si le load est supérieur au max : renvoyer tout au max
                // sinon calculer le load
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
