using RestService.Common.Exceptions;
using Serilog.Events;

namespace RestService.DependencyInjection.Models;

/// <summary>
/// Fluentbit configuration model
/// </summary>
public record FluentBitOptions
{
    private readonly string? _host;

    public required string Host
    {
        get => _host ?? throw new ConfigurationException(nameof(FluentBitOptions), nameof(Host));
        init => _host = value;
    }

    private readonly int _port;

    public required int Port
    {
        get
        {
            if (_port == default)
            {
                throw new ConfigurationException(nameof(FluentBitOptions), nameof(Port));
            }

            return _port;
        }
        init => _port = value;
    }

    private readonly string? _tag;

    public required string Tag
    {
        get => _tag ?? throw new ConfigurationException(nameof(FluentBitOptions), nameof(Tag));
        init => _tag = value;
    }

    public LogEventLevel RestrictedToMinimumLevel { get; set; }
}