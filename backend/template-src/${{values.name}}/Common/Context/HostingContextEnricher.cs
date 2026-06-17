using Serilog.Core;
using Serilog.Events;

namespace RestService.Common.Context;

/// <summary>
/// Configures and enriches Serilog with hosting information. 
/// </summary>
public class HostingContextEnricher : ILogEventEnricher
{
    public HostingContextEnricher()
    {
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("HostIp",
                                      ApplicationHostingContext.HostIp, true));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("Namespace",
                                      ApplicationHostingContext.Namespace, true));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("NodeName",
                                      ApplicationHostingContext.NodeName, true));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("PodIp",
                                      ApplicationHostingContext.PodIp, true));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("PodName",
                                      ApplicationHostingContext.PodName, true));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ServiceAccountName",
                                      ApplicationHostingContext.ServiceAccountName, true));

        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ApplicationVer",
                                      ApplicationHostingContext.ApplicationVer, true));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ApplicationName",
                                      ApplicationHostingContext.ApplicationName, true));
    }
}