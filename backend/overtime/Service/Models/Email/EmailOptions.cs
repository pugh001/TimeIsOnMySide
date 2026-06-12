namespace Overtime.Service.Models.Email;

public sealed class EmailOptions
{
    public string SmtpHost { get; init; } = string.Empty;
    public int SmtpPort { get; init; } = 587;
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string FromAddress { get; init; } = "noreply@timeisonymyside.local";
    public string FromName { get; init; } = "TimeIsOnMySide";
    public string DemoRecipientEmail { get; init; } = string.Empty;
}
