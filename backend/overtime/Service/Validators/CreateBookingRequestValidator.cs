using FluentValidation;
using Overtime.Service.Models.Bookings;

namespace Overtime.Service.Validators;

public sealed class CreateBookingRequestValidator : AbstractValidator<CreateBookingRequest>
{
    private static readonly System.Text.RegularExpressions.Regex SlotIdPattern =
        new(@"^.+-\d{4}-\d{2}-\d{2}-\d{2}:\d{2}$");

    public CreateBookingRequestValidator()
    {
        RuleFor(x => x.SlotId)
            .NotEmpty().WithMessage("SlotId is required.")
            .Matches(SlotIdPattern).WithMessage("SlotId must match format {locationId}-{YYYY-MM-DD}-{HH:mm}.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email is not a valid email address.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone is required.")
            .Matches(@"^0\d{9}$").WithMessage("Phone must be a 10-digit number starting with 0 (e.g. 0831231234).");
    }
}
