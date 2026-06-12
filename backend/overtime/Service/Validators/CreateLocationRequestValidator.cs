using FluentValidation;
using Overtime.Service.Models.Locations;

namespace Overtime.Service.Validators;

public sealed class CreateLocationRequestValidator : AbstractValidator<CreateLocationRequest>
{
    public CreateLocationRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(2)
            .WithMessage("Name must be at least 2 characters.");

        RuleFor(x => x.Address)
            .NotEmpty()
            .WithMessage("Address is required.");
    }
}
