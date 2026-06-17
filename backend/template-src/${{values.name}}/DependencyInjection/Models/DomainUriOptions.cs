namespace RestService.DependencyInjection.Models;

public class DomainUriOptions
{
    /// <summary>
    /// Base URL to the Domain
    /// </summary>
    public string GatewayURL { get; set; }

    /// <summary>
    /// All the endpoints in the Domain that you are trying to access
    /// </summary>
    public RelativeURLs RelativeUrLs { get; set; }

    public class RelativeURLs
    {
        /// <summary>
        /// Example of an endpoint in the domain.
        /// </summary>
        public string Endpoint1URL { get; set; }

        public string Endpoint2URL { get; set; }
    }
}