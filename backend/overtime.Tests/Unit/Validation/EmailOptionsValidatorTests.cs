using FluentAssertions;
using Microsoft.Extensions.Options;
using Overtime.Service.Models.Email;

namespace Overtime.Tests.Unit.Validation;

public sealed class EmailOptionsValidatorTests
{
    private static EmailOptionsValidator Validator() => new();

    [Fact]
    public void Validate_EmptySmtpHost_ReturnsFailure()
    {
        var result = Validator().Validate(null, new EmailOptions { SmtpHost = "" });
        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("SmtpHost");
    }

    [Fact]
    public void Validate_WhitespaceSmtpHost_ReturnsFailure()
    {
        var result = Validator().Validate(null, new EmailOptions { SmtpHost = "   " });
        result.Failed.Should().BeTrue();
    }

    [Fact]
    public void Validate_InvalidSmtpPort_Zero_ReturnsFailure()
    {
        var result = Validator().Validate(null, new EmailOptions { SmtpHost = "smtp.example.com", SmtpPort = 0 });
        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().Contain("SmtpPort");
    }

    [Fact]
    public void Validate_InvalidSmtpPort_TooHigh_ReturnsFailure()
    {
        var result = Validator().Validate(null, new EmailOptions { SmtpHost = "smtp.example.com", SmtpPort = 70000 });
        result.Failed.Should().BeTrue();
    }

    [Fact]
    public void Validate_ValidOptions_ReturnsSuccess()
    {
        var result = Validator().Validate(null, new EmailOptions
        {
            SmtpHost = "smtp.example.com",
            SmtpPort = 587
        });
        result.Succeeded.Should().BeTrue();
    }
}
