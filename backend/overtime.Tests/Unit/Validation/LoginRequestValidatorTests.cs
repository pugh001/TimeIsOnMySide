using FluentAssertions;
using Overtime.Service.Models.Auth;
using Overtime.Service.Validators;

namespace Overtime.Tests.Unit.Validation;

public sealed class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator = new();

    private static LoginRequest Req(string username, string password) =>
        new() { Username = username, Password = password };

    // ── Valid usernames ───────────────────────────────────────────────────────

    [Fact]
    public void Validate_AdminUsername_IsValid()
    {
        _validator.Validate(Req("admin", "pass")).Errors
            .Should().NotContain(e => e.PropertyName == "Username");
    }

    [Fact]
    public void Validate_ShortGeneratedUsername_IsValid()
    {
        _validator.Validate(Req("jane0001", "pass")).Errors
            .Should().NotContain(e => e.PropertyName == "Username");
    }

    [Fact]
    public void Validate_LongGeneratedUsername_IsValid()
    {
        // firstName = "Christopher" → username = "christopher0001" (15 chars)
        _validator.Validate(Req("christopher0001", "pass")).Errors
            .Should().NotContain(e => e.PropertyName == "Username");
    }

    [Fact]
    public void Validate_SingleLetterPrefix_IsValid()
    {
        _validator.Validate(Req("a0001", "pass")).Errors
            .Should().NotContain(e => e.PropertyName == "Username");
    }

    // ── Invalid usernames ─────────────────────────────────────────────────────

    [Fact]
    public void Validate_EmptyUsername_ReturnsError()
    {
        _validator.Validate(Req("", "pass")).Errors
            .Should().Contain(e => e.PropertyName == "Username");
    }

    [Fact]
    public void Validate_UppercaseInUsername_ReturnsError()
    {
        // Generated usernames are always lowercase
        _validator.Validate(Req("Jane0001", "pass")).Errors
            .Should().Contain(e => e.PropertyName == "Username");
    }

    [Fact]
    public void Validate_NoDigitSuffix_ReturnsError()
    {
        // "jane" alone has no 4-digit suffix
        _validator.Validate(Req("jane", "pass")).Errors
            .Should().Contain(e => e.PropertyName == "Username");
    }

    [Fact]
    public void Validate_OnlyDigits_ReturnsError()
    {
        _validator.Validate(Req("12340001", "pass")).Errors
            .Should().Contain(e => e.PropertyName == "Username");
    }

    [Fact]
    public void Validate_EmptyPassword_ReturnsError()
    {
        _validator.Validate(Req("admin", "")).Errors
            .Should().Contain(e => e.PropertyName == "Password");
    }
}
