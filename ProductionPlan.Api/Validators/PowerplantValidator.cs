using FluentValidation;
using FluentValidation.Validators;
using ProductionPlan.Core.Models;

namespace ProductionPlan.Api.Validators
{
    public class PowerplantValidator : AbstractValidator<Powerplant>
    {
        public PowerplantValidator()
        {
            RuleFor(p => p.Name).NotEmpty();
            RuleFor(p => p.Type).NotNull();
            RuleFor(p => p.Efficiency).NotNull().GreaterThanOrEqualTo(0).LessThanOrEqualTo(100).WithMessage("Value of property Efficiency must be between 0 and 100");
            RuleFor(p => p.PMin).NotNull().GreaterThanOrEqualTo(0);
            RuleFor(p => p.PMax).NotNull().GreaterThanOrEqualTo(0);
        }
    }
}