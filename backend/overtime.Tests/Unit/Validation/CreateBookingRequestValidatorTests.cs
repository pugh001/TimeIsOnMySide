using FluentValidation.TestHelper;
using Overtime.Service.Models.Bookings;
using Overtime.Service.Validators;

namespace Overtime.Tests.Unit.Validation;

public sealed class CreateBookingRequestValidatorTests
{
    private readonly CreateBookingRequestValidator _validator = new();

    private static CreateBookingRequest Valid() => new()
    {
        SlotId = "branch-city-a-2026-05-26-09:00",
        Name = "Jane Doe",
        Email = "jane@example.com",
        Phone = "0831231234"
    };

    [Fact]
    public void ValidRequest_PassesValidation()
    {
        var result = _validator.TestValidate(Valid());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ValidRequest_WithNotes_PassesValidation()
    {
        var req = Valid();
        req.Notes = "Discuss project scope";
        var result = _validator.TestValidate(req);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void EmptySlotId_FailsValidation()
    {
        var req = Valid();
        req.SlotId = string.Empty;
        var result = _validator.TestValidate(req);
        result.ShouldHaveValidationErrorFor(x => x.SlotId);
    }

    [Fact]
    public void MalformedSlotId_FailsValidation()
    {
        var req = Valid();
        req.SlotId = "not-a-valid-slot-id";
        var result = _validator.TestValidate(req);
        result.ShouldHaveValidationErrorFor(x => x.SlotId);
    }

    [Fact]
    public void EmptyName_FailsValidation()
    {
        var req = Valid();
        req.Name = string.Empty;
        var result = _validator.TestValidate(req);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void EmptyEmail_FailsValidation()
    {
        var req = Valid();
        req.Email = string.Empty;
        var result = _validator.TestValidate(req);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void InvalidEmail_FailsValidation()
    {
        var req = Valid();
        req.Email = "not-an-email";
        var result = _validator.TestValidate(req);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void EmptyPhone_FailsValidation()
    {
        var req = Valid();
        req.Phone = string.Empty;
        var result = _validator.TestValidate(req);
        result.ShouldHaveValidationErrorFor(x => x.Phone);
    }

    [Fact]
    public void NullNotes_PassesValidation()
    {
        var req = Valid();
        req.Notes = null;
        var result = _validator.TestValidate(req);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ValidSAPhone_PassesValidation()
    {
        var req = Valid();
        req.Phone = "0831231234";
        var result = _validator.TestValidate(req);
        result.ShouldNotHaveValidationErrorFor(x => x.Phone);
    }

    [Fact]
    public void InternationalPhone_FailsValidation()
    {
        var req = Valid();
        req.Phone = "+1234567890";
        var result = _validator.TestValidate(req);
        result.ShouldHaveValidationErrorFor(x => x.Phone);
    }

    [Fact]
    public void PhoneWithoutLeadingZero_FailsValidation()
    {
        var req = Valid();
        req.Phone = "1234567890";
        var result = _validator.TestValidate(req);
        result.ShouldHaveValidationErrorFor(x => x.Phone);
    }

    [Fact]
    public void PhoneWithNineDigits_FailsValidation()
    {
        var req = Valid();
        req.Phone = "083123123";
        var result = _validator.TestValidate(req);
        result.ShouldHaveValidationErrorFor(x => x.Phone);
    }

    [Fact]
    public void PhoneWithElevenDigits_FailsValidation()
    {
        var req = Valid();
        req.Phone = "08312312345";
        var result = _validator.TestValidate(req);
        result.ShouldHaveValidationErrorFor(x => x.Phone);
    }
}
