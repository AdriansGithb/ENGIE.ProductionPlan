using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using Moq;
using ProductionPlan.Api.Controllers;
using ProductionPlan.Core.Abstract;
using ProductionPlan.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ProductionPlan.Api.Test
{
    public class ProductionPlanControllerTest
    {
        private readonly ProductionPlanController _controller;
        private readonly Mock<IProductionService> _productionService;
        private readonly Mock<ILogger<ProductionPlanController>> _logger;
        private readonly Mock<IValidator<Payload>> _validator;

        public ProductionPlanControllerTest()
        {
            _productionService = new Mock<IProductionService>();
            _logger = new Mock<ILogger<ProductionPlanController>>();
            _validator = new Mock<IValidator<Payload>>();
            _controller = new ProductionPlanController(_productionService.Object, _logger.Object, _validator.Object);
        }

        [Fact]
        public void Post_ShouldReturnOkReponse_IfNoExceptionIsThrown()
        {
            _validator.Setup(x => x.Validate(It.IsAny<Payload>())).Returns(new ValidationResult());
            _productionService.Setup(x => x.PlanProduction(It.IsAny<Payload>())).Returns(new List<PlannedProductionPowerplant>());
            var payload = new Payload
            {
                Load = It.IsAny<decimal>(),
                Powerplants = new List<Powerplant>(),
            };

            var res = _controller.Post(payload);
            Assert.IsType<OkObjectResult>(res);
        }

        [Fact]
        public void Post_ShouldReturnBadRequest_WhenValidationFails()
        {
            _validator.Setup(x => x.Validate(It.IsAny<Payload>())).Returns(new ValidationResult(new List<ValidationFailure>
            {
                new ValidationFailure("Load", "Load is negative"),
            })) ;
            _productionService.Setup(x => x.PlanProduction(It.IsAny<Payload>())).Throws<ArgumentException>(() => new ArgumentException("Target load is higher than maximum producible power"));
            var payload = new Payload
            {
                Load = It.IsAny<decimal>(),
                Powerplants = new List<Powerplant>(),
            };

            var actionRes = _controller.Post(payload);
            Assert.IsType<BadRequestObjectResult>(actionRes);
            var res = actionRes as BadRequestObjectResult;
            Assert.NotNull(res.Value);
            Assert.StartsWith("Invalid payload object", res.Value.ToString());
        }

        [Fact]
        public void Post_ShouldReturnBadRequest_IfLoadIsHigherThanMaximumSumsExceptionIsThrown()
        {
            _validator.Setup(x => x.Validate(It.IsAny<Payload>())).Returns(new ValidationResult());
            _productionService.Setup(x => x.PlanProduction(It.IsAny<Payload>())).Throws<ArgumentException>(() => new ArgumentException("Target load is higher than maximum producible power"));
            var payload = new Payload
            {
                Load = It.IsAny<decimal>(),
                Powerplants = new List<Powerplant>(),
            };

            var actionRes = _controller.Post(payload);
            Assert.IsType<BadRequestObjectResult>(actionRes);
            var res = actionRes as BadRequestObjectResult;
            Assert.NotNull(res.Value);
            Assert.Equal("Target load is higher than maximum producible power", res.Value );
        }

        [Fact]
        public void Post_ShouldReturnStatusCode500_IfAnyOtherExceptionIsThrown()
        {
            _validator.Setup(x => x.Validate(It.IsAny<Payload>())).Returns(new ValidationResult());
            _productionService.Setup(x => x.PlanProduction(It.IsAny<Payload>())).Throws<Exception>();
            var payload = new Payload
            {
                Load = It.IsAny<decimal>(),
                Powerplants = new List<Powerplant>(),
            };

            var actionRes = _controller.Post(payload);
            Assert.IsType<ObjectResult>(actionRes);
            var res = actionRes as ObjectResult;
            Assert.NotNull(res.Value);
            Assert.Equal(StatusCodes.Status500InternalServerError, res.StatusCode );
        }


    }
}
