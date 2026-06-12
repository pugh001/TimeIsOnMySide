using FluentValidation;
using Overtime.Service.Models.Users;

namespace Overtime.Service.Validators;

public sealed class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    private static readonly HashSet<string> ValidDays =
        new(["monday", "tuesday", "wednesday", "thursday", "friday", "saturday", "sunday"],
            StringComparer.OrdinalIgnoreCase);

    private const string TimePattern = @"^([01]\d|2[0-3]):[0-5]\d$";

    public CreateUserRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().WithMessage("First name is required.");
        RuleFor(x => x.LastName).NotEmpty().WithMessage("Last name is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.");

        RuleFor(x => x.LocationId).NotEmpty().WithMessage("Location is required.");

        RuleFor(x => x.WorkingTimes)
            .NotEmpty().WithMessage("At least one working time is required.");

        RuleForEach(x => x.WorkingTimes).ChildRules(wt =>
        {
            wt.RuleFor(w => w.Day)
                .NotEmpty().WithMessage("Day is required.")
                .Must(d => ValidDays.Contains(d)).WithMessage("Day must be a valid day name (monday–sunday).");

            wt.RuleFor(w => w.ShiftStart)
                .NotEmpty().WithMessage("Shift start is required.")
                .Matches(TimePattern).WithMessage("Shift start must be in HH:MM format.");

            wt.RuleFor(w => w.ShiftEnd)
                .NotEmpty().WithMessage("Shift end is required.")
                .Matches(TimePattern).WithMessage("Shift end must be in HH:MM format.");

            wt.RuleFor(w => w)
                .Must(HaveValidShiftSpan)
                .When(w => IsValidTime(w.ShiftStart) && IsValidTime(w.ShiftEnd))
                .WithName("ShiftEnd")
                .WithMessage("Shift span must not exceed 8 hours.");
        });

        RuleFor(x => x.WorkingTimes)
            .Must(HaveNoDuplicateDays).WithMessage("Each day may only appear once in working times.")
            .When(x => x.WorkingTimes.Length > 0);
    }

    private static bool HaveValidShiftSpan(UserWorkingTimeDto entry)
    {
        var start = TimeOnly.Parse(entry.ShiftStart);
        var end = TimeOnly.Parse(entry.ShiftEnd);
        var spanMinutes = end >= start
            ? (end - start).TotalMinutes
            : (end.AddHours(24) - start).TotalMinutes;
        return spanMinutes <= 8 * 60;
    }

    private static bool HaveNoDuplicateDays(UserWorkingTimeDto[] times)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        return times.All(t => seen.Add(t.Day));
    }

    private static bool IsValidTime(string value) =>
        !string.IsNullOrEmpty(value) && System.Text.RegularExpressions.Regex.IsMatch(value, TimePattern);
}
