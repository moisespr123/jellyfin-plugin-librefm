using Microsoft.Extensions.Logging;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Jellyfin.Plugin.Librefm.Api
{

    [ApiController]
    [Route("Librefm/Login")]
    public class RestApi : ControllerBase
    {
        private readonly LibrefmApiClient _apiClient;
        private readonly ILogger<RestApi> _logger;
        private static readonly object _apiHostLock = new();

        public RestApi(IHttpClientFactory httpClientFactory, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<RestApi>();
            _apiClient = new LibrefmApiClient(httpClientFactory, _logger);
        }

        [HttpPost]
        [Consumes("application/json")]
        public object CreateMobileSession([FromBody] LibreFMUser libreFMUser)
        {
            _logger.LogInformation("Fetching Libre.fm mobilesession auth for Username={0}", libreFMUser.Username);
            return ExecuteWithApiHostOverride(libreFMUser.ApiHost, () => _apiClient.RequestSession(libreFMUser.Username, libreFMUser.Password).Result);
        }

        private static object ExecuteWithApiHostOverride(string apiHost, Func<object> action)
        {
            lock (_apiHostLock)
            {
                var config = Plugin.Instance?.PluginConfiguration;
                if (config == null)
                {
                    return action();
                }

                var originalHost = config.LibrefmApiHost;

                if (!string.IsNullOrWhiteSpace(apiHost))
                {
                    config.LibrefmApiHost = apiHost;
                }

                try
                {
                    return action();
                }
                finally
                {
                    config.LibrefmApiHost = originalHost;
                }
            }
        }
    }

    public class LibreFMUser
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string ApiHost { get; set; }
    }
}
