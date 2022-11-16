using FluentValidation;
using ProductionPlan.Core.Models;

namespace ProductionPlan.Api.Validators
{
    public class PayloadValidator : AbstractValidator<Payload>
    {
        public PayloadValidator()
        {
            RuleFor(p => p.Load).GreaterThanOrEqualTo(0).NotNull();
            RuleFor(p => p.Fuels).SetValidator(new FuelValidator());
            RuleForEach(p => p.Powerplants).SetValidator(new PowerplantValidator());
            RuleFor(p => p.Powerplants)
                .NotEmpty().WithMessage("Powerplants list can't be empty")
                .Must(pList => pList.Count() == pList.Select(p => p.Name).Distinct().Count())
                .WithMessage("Powerplants list doesn't contain unique names");
        }
    }
}
