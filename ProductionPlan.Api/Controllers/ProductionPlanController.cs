using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductionPlan.Core.Abstract;
using ProductionPlan.Core.Models;

namespace ProductionPlan.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductionPlanController : ControllerBase
    {
        private readonly IProductionService _productionService;
        private readonly ILogger<ProductionPlanController> _logger;

        public ProductionPlanController(IProductionService productionService, ILogger<ProductionPlanController> logger)
        {
            _productionService = productionService;
            _logger = logger;
            _logger.LogDebug(1, "NLog injected into HomeController");
        }

        [HttpPost]
        public IActionResult Post([FromBody]Payload payload)
        {
            try
            {
                _logger.LogInformation("Post payload reached !");
                throw new NotImplementedException();
                //return _productionService.PlanProduction(payload);
            }
            catch (Exception ex)
            {
                throw new NotImplementedException();
            }
        }
    }
}
