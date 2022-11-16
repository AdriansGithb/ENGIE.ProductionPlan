using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductionPlan.Core.Abstract;
using ProductionPlan.Core.Models;
using System;
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
            _logger.LogDebug(1, "NLog injected into HomeController");
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
                    _logger.LogError($"Received payload object was invalid : {fullFailuresMessages}");
                    return BadRequest($"Invalid payload object : {fullFailuresMessages}");
                }

                _logger.LogInformation("Post payload reached!");
                var proposedProductionPlan = _productionService.PlanProduction(payload);

                _logger.LogInformation($"Controller sent a valid production plan : received load '{payload.Load}' - received powerplants count '{payload.Powerplants.Count()}' // sent load '{proposedProductionPlan.Sum(pwp => pwp.P)}' - sent powerplants count '{proposedProductionPlan.Count()}'");
                return Ok(proposedProductionPlan);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Controller response catched an exception : {ex.Message}");
                var actionResponse = ex.Message switch
                {
                    "Received load is less than zero" => BadRequest(ex.Message),
                    "Target load is higher than maximum producible power" => BadRequest(ex.Message),
                    _ => StatusCode(StatusCodes.Status500InternalServerError, ex),
                };
                _logger.LogInformation($"Controller error response is : {actionResponse.StatusCode} - {actionResponse.Value}");
                return actionResponse;
            }
        }
    }
}
