using System.Reflection;

namespace RestService.Common.Context;

/// <summary>
/// Reads all hosting information from the configuration into this context to be used to enrich the logging.
/// </summary>
public static class ApplicationHostingContext
{
    public static void ReadHostingConfiguration(IConfiguration configuration)
    {
        NodeName = GetValueOrDefault(configuration, "NODE_NAME");
        HostIp = GetValueOrDefault(configuration, "HOST_IP");
        PodName = GetValueOrDefault(configuration, "POD_NAME");
        Namespace = GetValueOrDefault(configuration, "NAMESPACE");
        PodIp = GetValueOrDefault(configuration, "POD_IP");
        ServiceAccountName = GetValueOrDefault(configuration, "SERVICE_ACCOUNT_NAME");
    }

    private static string GetValueOrDefault(IConfiguration configuration, string key)
    {
        return configuration[key] ?? DefaultValue;
    }

    private const string DefaultValue = "N/A";

    public static string NodeName { get; set; }
    public static string HostIp { get; set; }
    public static string Namespace { get; set; }
    public static string PodIp { get; set; }
    public static string PodName { get; set; }
    public static string ServiceAccountName { get; set; }

    public static string ApplicationName
    {
        get { return Assembly.GetEntryAssembly()?.GetName().Name; }
    }

    public static string ApplicationVer
    {
        get { return Assembly.GetEntryAssembly()?.GetName().Version.ToString(); }
    }
}