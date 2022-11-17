using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using ProductionPlan.Core.Abstract;
using ProductionPlan.Core.Models;
using System.Text;

namespace ProductionPlan.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductionPlanController : ControllerBase
    {
        private readonly IProductionService _productionService;
        private readonly ILogger<ProductionPlanController> _logger;
        private readonly IValidator<Payload> _validator;

        public ProductionPlanController(IProductionService productionService, ILogger<ProductionPlanController> logger, IValidator<Payload> validator)
        {
            _productionService = productionService;
            _logger = logger;
            #region log
            _logger.LogDebug(1, "NLog injected into HomeController");
            #endregion 
            _validator = validator;
        }

        [HttpPost]
        public IActionResult Post([FromBody]Payload payload)
        {
            try
            {
                ValidationResult validationResult = _validator.Validate(payload);
                if (!validationResult.IsValid)
                {
                    StringBuilder fullFailuresMessages = new StringBuilder();
                    foreach (var failure in validationResult.Errors)
                    {
                        fullFailuresMessages.Append(string.Concat("Property " , failure.PropertyName , " failed validation. Error was: " , failure.ErrorMessage, " // "));
                    }
                    #region log
                    _logger.LogError($"Received payload object was invalid : {fullFailuresMessages}");
                    #endregion
                    return BadRequest($"Invalid payload object : {fullFailuresMessages}");
                }

                #region log
                _logger.LogInformation("Post payload reached!"); 
                #endregion
                var proposedProductionPlan = _productionService.PlanProduction(payload);

                #region log
                _logger.LogInformation($"Controller sent a valid production plan : received load '{payload.Load}' - received powerplants count '{payload.Powerplants.Count()}' // sent load '{proposedProductionPlan.Sum(pwp => pwp.P)}' - sent powerplants count '{proposedProductionPlan.Count()}'"); 
                #endregion
                return Ok(proposedProductionPlan);
            }
            catch (Exception ex)
            {
                #region log
                _logger.LogError($"Controller response catched an exception : {ex.Message}"); 
                #endregion
                var actionResponse = ex.Message switch
                {
                    "Target load is higher than maximum producible power" => BadRequest(ex.Message),
                    "Target load is less than minimum producible power" => BadRequest(ex.Message),
                    _ => StatusCode(StatusCodes.Status500InternalServerError, ex),
                };
                #region log
                _logger.LogInformation($"Controller error response is : {actionResponse.StatusCode} - {actionResponse.Value}");
                #endregion
                return actionResponse;
            }
        }
    }
}
