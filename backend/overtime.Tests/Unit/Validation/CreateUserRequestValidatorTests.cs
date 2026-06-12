using FluentAssertions;
using Overtime.Service.Models.Users;
using Overtime.Service.Validators;

namespace Overtime.Tests.Unit.Validation;

public sealed class CreateUserRequestValidatorTests
{
    private readonly CreateUserRequestValidator _validator = new();

    private static UserWorkingTimeDto ValidShift(string day = "monday", string start = "09:00", string end = "17:00")
        => new(day, start, end);

    private static CreateUserRequest ValidRequest() => new()
    {
        FirstName = "Jane",
        LastName = "Doe",
        Password = "secret99",
        LocationId = Guid.NewGuid().ToString(),
        WorkingTimes = [ValidShift()]
    };

    [Fact]
    public async Task ValidRequest_Passes()
    {
        var result = await _validator.ValidateAsync(ValidRequest());
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task MissingFirstName_Fails()
    {
        var result = await _validator.ValidateAsync(ValidRequest() with { FirstName = "" });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FirstName");
    }

    [Fact]
    public async Task MissingLastName_Fails()
    {
        var result = await _validator.ValidateAsync(ValidRequest() with { LastName = "" });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "LastName");
    }

    [Fact]
    public async Task PasswordTooShort_Fails()
    {
        var result = await _validator.ValidateAsync(ValidRequest() with { Password = "short" });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    [Fact]
    public async Task MissingPassword_Fails()
    {
        var result = await _validator.ValidateAsync(ValidRequest() with { Password = "" });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    [Fact]
    public async Task MissingLocationId_Fails()
    {
        var result = await _validator.ValidateAsync(ValidRequest() with { LocationId = "" });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "LocationId");
    }

    [Fact]
    public async Task EmptyWorkingTimes_Fails()
    {
        var result = await _validator.ValidateAsync(ValidRequest() with { WorkingTimes = [] });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "WorkingTimes");
    }

    [Fact]
    public async Task InvalidDayName_Fails()
    {
        var result = await _validator.ValidateAsync(ValidRequest() with
        {
            WorkingTimes = [ValidShift("funday")]
        });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Day"));
    }

    [Fact]
    public async Task InvalidShiftStartFormat_Fails()
    {
        var result = await _validator.ValidateAsync(ValidRequest() with
        {
            WorkingTimes = [ValidShift(start: "9:00")]
        });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("ShiftStart"));
    }

    [Fact]
    public async Task InvalidShiftEndFormat_Fails()
    {
        var result = await _validator.ValidateAsync(ValidRequest() with
        {
            WorkingTimes = [ValidShift(end: "17:60")]
        });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("ShiftEnd"));
    }

    [Fact]
    public async Task ShiftSpanOver8Hours_Fails()
    {
        var result = await _validator.ValidateAsync(ValidRequest() with
        {
            WorkingTimes = [ValidShift(start: "09:00", end: "18:01")]
        });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("8 hours"));
    }

    [Fact]
    public async Task ShiftSpanExactly8Hours_Passes()
    {
        var result = await _validator.ValidateAsync(ValidRequest() with
        {
            WorkingTimes = [ValidShift(start: "09:00", end: "17:00")]
        });
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task MidnightCrossingWithin8Hours_Passes()
    {
        var result = await _validator.ValidateAsync(ValidRequest() with
        {
            WorkingTimes = [ValidShift(start: "22:00", end: "06:00")]
        });
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task MidnightCrossingOver8Hours_Fails()
    {
        var result = await _validator.ValidateAsync(ValidRequest() with
        {
            WorkingTimes = [ValidShift(start: "22:00", end: "06:01")]
        });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("8 hours"));
    }

    [Fact]
    public async Task DuplicateDays_Fails()
    {
        var result = await _validator.ValidateAsync(ValidRequest() with
        {
            WorkingTimes = [ValidShift("monday"), ValidShift("monday", "10:00", "18:00")]
        });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("once"));
    }

    [Fact]
    public async Task AllValidDays_MultipleDays_Passes()
    {
        var result = await _validator.ValidateAsync(ValidRequest() with
        {
            WorkingTimes =
            [
                ValidShift("monday"),
                ValidShift("tuesday"),
                ValidShift("wednesday"),
                ValidShift("thursday"),
                ValidShift("friday"),
                ValidShift("saturday"),
                ValidShift("sunday")
            ]
        });
        result.IsValid.Should().BeTrue();
    }
}
