using FluentValidation;
using ProductionPlan.Core.Models;

namespace ProductionPlan.Api.Validators
{
    public class FuelValidator : AbstractValidator<Fuel>
    {
        public FuelValidator()
        {
            RuleFor(f => f.KerosineCostPerMwh).NotNull().GreaterThanOrEqualTo(0);
            RuleFor(f => f.Co2CostPerTon).NotNull().GreaterThanOrEqualTo(0);
            RuleFor(f => f.GasCostPerMwh).NotNull().GreaterThanOrEqualTo(0);
            RuleFor(f => f.WindEfficiency).NotNull().GreaterThanOrEqualTo(0).LessThanOrEqualTo(100).WithMessage("Value of property Wind must be between 0 and 100");
        }
    }
}