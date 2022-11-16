using FluentValidation;
using ProductionPlan.Core.Models;

namespace ProductionPlan.Api.Validators
{
    public class FuelValidator : AbstractValidator<Fuel>
    {
        public FuelValidator()
        {
            RuleFor(f => f.Kerosine).NotNull().GreaterThanOrEqualTo(0);
            RuleFor(f => f.Co2).NotNull().GreaterThanOrEqualTo(0);
            RuleFor(f => f.Gas).NotNull().GreaterThanOrEqualTo(0);
            RuleFor(f => f.Wind).NotNull().GreaterThanOrEqualTo(0).LessThanOrEqualTo(100).WithMessage("Value of property Wind must be between 0 and 100");
        }
    }
}