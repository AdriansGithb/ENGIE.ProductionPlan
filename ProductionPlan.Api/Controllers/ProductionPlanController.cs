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

        public ProductionPlanController(IProductionService productionService)
        {
            _productionService = productionService;
        }

        [HttpPost]
        public IActionResult Post([FromBody]Payload payload)
        {
            try
            {
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
