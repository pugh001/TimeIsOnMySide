using FluentValidation;
using Overtime.Service.Models.Auth;

namespace Overtime.Service.Validators;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .Matches(@"^(admin|[a-z]+[0-9]{4})$").WithMessage("Invalid username format.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}
