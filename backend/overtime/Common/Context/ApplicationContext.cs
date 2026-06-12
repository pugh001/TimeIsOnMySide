using Overtime.Common.Exceptions;

namespace Overtime.Common.Context;

public sealed class ApplicationContext : IApplicationContext
{
    private readonly string _correlationId;

    public ApplicationContext(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(httpContextAccessor);

        var correlationKey = configuration.GetValue<string>("CorrelationKey")
            ?? throw new ConfigurationException("Configuration key 'CorrelationKey' is missing.");
        _correlationId =
            httpContextAccessor.HttpContext?.Request.Headers[correlationKey].FirstOrDefault()
            ?? Guid.NewGuid().ToString();
    }

    public string CorrelationId => _correlationId;
}
