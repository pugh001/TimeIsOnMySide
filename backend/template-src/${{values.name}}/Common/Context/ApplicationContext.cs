using RestService.Common.Exceptions;

namespace RestService.Common.Context
{
    /// <summary>
    /// Application Context that can be used to store state relating to the current application.
    /// </summary>
    public class ApplicationContext : IApplicationContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _correlationKey;
        private string _correlationId;

        public ApplicationContext(IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _correlationKey = configuration.GetValue<string>("CorrelationKey") ??
                              throw new
                                  ConfigurationException($"Configuration for key 'CorrelationKey' is missing");
        }

        public string CorrelationId
        {
            get { return _correlationId ?? SetCorrelationId(); }
        }

        public string CorrelationKey
        {
            get { return _correlationKey; }
        }

        public string SetCorrelationId()
        {
            _correlationId =
                _httpContextAccessor.HttpContext?.Request?.Headers[_correlationKey]
                                    .FirstOrDefault() ?? Guid.NewGuid().ToString();

            return _correlationId;
        }

    }
}
