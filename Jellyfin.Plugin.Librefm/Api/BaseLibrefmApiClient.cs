namespace Jellyfin.Plugin.Librefm.Api
{
    using Models.Requests;
    using Models.Responses;
    using Resources;
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Utils;
    using Microsoft.Extensions.Logging;

    public class BaseLibrefmApiClient
    {
        private const string ApiVersion = "2.0";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        public BaseLibrefmApiClient(IHttpClientFactory httpClientFactory, ILogger logger)
        {
            _httpClientFactory = httpClientFactory;
            _httpClient = _httpClientFactory.CreateClient();
            _logger = logger;
        }

        /// <summary>
        /// Send a POST request to the Libre.fm Api
        /// </summary>
        /// <typeparam name="TRequest">The type of the request</typeparam>
        /// <typeparam name="TResponse">The type of the response</typeparam>
        /// <param name="request">The request</param>
        /// <returns>A response with type TResponse</returns>
        public async Task<TResponse> Post<TRequest, TResponse>(TRequest request) where TRequest : BaseRequest where TResponse : BaseResponse
        {
            var data = request.ToDictionary();

            // Append the signature
            Helpers.AppendSignature(ref data);

            var url = BuildPostUrl(request.Secure, GetApiEndpointHost());
            LogRequestDiagnostics(data, url, HttpMethod.Post.Method);
            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
            requestMessage.Content = new StringContent(SetPostData(data), Encoding.UTF8, "application/x-www-form-urlencoded");
            using (var response = await _httpClient.SendAsync(requestMessage, CancellationToken.None))
            {
                var result = await TryDeserializeResponse<TResponse>(response, url, HttpMethod.Post.Method);
                if (result == null)
                {
                    return null;
                }

                if (result.IsError())
                    _logger.LogError(result.Message);

                return result;
            }
        }

        public async Task<TResponse> Get<TRequest, TResponse>(TRequest request) where TRequest : BaseRequest where TResponse : BaseResponse
        {
            return await Get<TRequest, TResponse>(request, CancellationToken.None);
        }

        public async Task<TResponse> Get<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken) where TRequest : BaseRequest where TResponse : BaseResponse
        {
            var url = BuildGetUrl(request.ToDictionary(), request.Secure, GetApiEndpointHost());
            using (var response = await _httpClient.GetAsync(url, cancellationToken))
            {
                var result = await TryDeserializeResponse<TResponse>(response, url, HttpMethod.Get.Method);
                if (result == null)
                {
                    return null;
                }

                if (result.IsError())
                    _logger.LogError(result.Message);

                return result;
            }
        }

        #region Private methods
        private static string BuildGetUrl(Dictionary<string, string> requestData, bool secure, string endpointHost)
        {
            return String.Format("{0}://{1}/{2}/?format=json&{3}",
                                    secure ? "https" : "http",
                                    endpointHost,
                                    ApiVersion,
                                    Helpers.DictionaryToQueryString(requestData)
                                );
        }

        private static string BuildPostUrl(bool secure, string endpointHost)
        {
            return String.Format("{0}://{1}/{2}/?format=json",
                                    secure ? "https" : "http",
                                    endpointHost,
                                    ApiVersion
                                );
        }

        private static string GetApiEndpointHost()
        {
            var configured = Plugin.Instance?.PluginConfiguration?.LibrefmApiHost;
            return NormalizeEndpointHost(configured, Strings.Endpoints.LibrefmApi);
        }

        private static string NormalizeEndpointHost(string configured, string fallbackHost)
        {
            if (string.IsNullOrWhiteSpace(configured))
            {
                return fallbackHost;
            }

            var host = configured.Trim();

            if (Uri.TryCreate(host, UriKind.Absolute, out var absoluteUri) && !string.IsNullOrWhiteSpace(absoluteUri.Host))
            {
                return absoluteUri.Host;
            }

            host = host.Replace("https://", string.Empty, StringComparison.OrdinalIgnoreCase)
                       .Replace("http://", string.Empty, StringComparison.OrdinalIgnoreCase)
                       .Trim('/');

            var slashIndex = host.IndexOf('/');
            if (slashIndex > -1)
            {
                host = host.Substring(0, slashIndex);
            }

            return string.IsNullOrWhiteSpace(host) ? fallbackHost : host;
        }

        private void LogRequestDiagnostics(Dictionary<string, string> data, string url, string method)
        {
            if (!data.TryGetValue("method", out var requestMethod) || !requestMethod.Equals(Strings.Methods.GetMobileSession, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var keys = string.Join(",", data.Keys.OrderBy(k => k));
            var hasUsername = data.TryGetValue("username", out var username) && !string.IsNullOrWhiteSpace(username);
            var hasPassword = data.TryGetValue("password", out var password) && !string.IsNullOrWhiteSpace(password);
            var hasAuthToken = data.TryGetValue("authToken", out var authToken) && !string.IsNullOrWhiteSpace(authToken);
            var hasApiKey = data.TryGetValue("api_key", out var apiKey) && !string.IsNullOrWhiteSpace(apiKey);
            var hasApiSig = data.TryGetValue("api_sig", out var apiSig) && !string.IsNullOrWhiteSpace(apiSig);

            _logger.LogInformation(
                "Sending mobile session request. Method={Method}, Url={Url}, Keys={Keys}, HasUsername={HasUsername}, HasPassword={HasPassword}, HasAuthToken={HasAuthToken}, HasApiKey={HasApiKey}, HasApiSig={HasApiSig}, UsernamePreview={UsernamePreview}",
                method,
                url,
                keys,
                hasUsername,
                hasPassword,
                hasAuthToken,
                hasApiKey,
                hasApiSig,
                MaskValue(username));
        }

        private static string MaskValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "(empty)";
            }

            if (value.Length <= 2)
            {
                return "**";
            }

            return value.Substring(0, 2) + "***";
        }

        private async Task<TResponse> TryDeserializeResponse<TResponse>(HttpResponseMessage response, string url, string method) where TResponse : BaseResponse
        {
            var responseBody = await response.Content.ReadAsStringAsync();

            var serializeOptions = new JsonSerializerOptions
            {
                NumberHandling = JsonNumberHandling.AllowReadingFromString
            };

            try
            {
                var result = JsonSerializer.Deserialize<TResponse>(responseBody, serializeOptions);
                if (result == null)
                {
                    _logger.LogError("{Method} {Url} returned empty JSON response. StatusCode={StatusCode}", method, url, (int)response.StatusCode);
                    return null;
                }

                return result;
            }
            catch (Exception e)
            {
                var bodyPreview = responseBody;
                if (!string.IsNullOrWhiteSpace(bodyPreview) && bodyPreview.Length > 250)
                {
                    bodyPreview = bodyPreview.Substring(0, 250);
                }

                _logger.LogError(e, "Failed to parse API response as JSON. Method={Method}, Url={Url}, StatusCode={StatusCode}, BodyPreview={BodyPreview}", method, url, (int)response.StatusCode, bodyPreview);
                return null;
            }
        }

        private static string SetPostData(Dictionary<string, string> dic)
        {
            var strings = dic.Keys.Select(key => string.Format("{0}={1}", key, Uri.EscapeDataString(dic[key])));
            return string.Join("&", strings.ToArray());

        }
        #endregion
    }
}

