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
            return _apiClient.RequestSession(libreFMUser.Username, libreFMUser.Password).Result;
        }
    }

    public class LibreFMUser
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
