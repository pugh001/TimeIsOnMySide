using Microsoft.Extensions.Options;

namespace Overtime.Service.Models.Email;

public sealed class EmailOptionsValidator : IValidateOptions<EmailOptions>
{
    public ValidateOptionsResult Validate(string? name, EmailOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.SmtpHost))
            return ValidateOptionsResult.Fail("Email:SmtpHost is required.");
        if (options.SmtpPort is <= 0 or > 65535)
            return ValidateOptionsResult.Fail("Email:SmtpPort must be between 1 and 65535.");
        return ValidateOptionsResult.Success;
    }
}
