using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Polly;
using Polly.Retry;
using System;
using System.Net.Http;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Clients
{
    /// <summary>
    /// Proxy class for HttpClient.
    /// </summary>
    public class HttpService : IHttpService
    {
        private const int MaxRetryAttempts = 3;

        private readonly TimeSpan pauseBetweenFailures = TimeSpan.FromSeconds(5);
        private readonly ApplicationLogger.ILogger _logger;
        private readonly ILogger _originalLogger;

        /// <summary>
        /// HTTP client.
        /// </summary>
        private readonly HttpClient _client;

        /// <summary>
        /// Authentication service for the HTTP Client.
        /// </summary>
        private readonly IAuthenticationService _authenticationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpService"/> class.
        /// </summary>
        /// <param name="httpClient">HTTP Client.</param>
        /// <param name="authenticationService">Authentication service for HTTP endpoint. Null if not required.</param>
        /// <param name="logger">Application logger.</param>
        /// <param name="originalLogger">The original logger.</param>
        public HttpService(
            HttpClient httpClient,
            IAuthenticationService authenticationService,
            ApplicationLogger.ILogger logger,
            ILogger originalLogger)
        {
            _client = httpClient;
            _authenticationService = authenticationService;
            _logger = logger;
            _originalLogger = originalLogger;
        }

        /// <summary>
        /// Make a Async call to a Uri.
        /// </summary>
        /// <param name="uri">Uri to query.</param>
        /// <returns>Return Task containing response.</returns>
        public async Task<string> GetAsync(string uri)
        {
            var response = await RetryPolicy().ExecuteAsync(async () =>
            {
                var dtStart = DateTime.Now;

                var httpRequestMsg = new HttpRequestMessage(HttpMethod.Get, uri);
                httpRequestMsg.Headers.Add(HeaderNames.Accept, MediaTypeNames.Application.Json);

                if (_authenticationService != null)
                {
                    var accessToken = await _authenticationService.GetAccessToken().ConfigureAwait(false);
                    httpRequestMsg.Headers.Add(HeaderNames.Authorization, $"Bearer {accessToken}");
                }

                var msg = $"Request made to {uri}";
                _logger?.LogTrace(msg);
                _originalLogger?.LogInformation(msg);

                var responseInternal = await _client.SendAsync(httpRequestMsg).ConfigureAwait(false);
                var totalSeconds = (DateTime.Now - dtStart).TotalSeconds;

                msg = $"Request to {uri} returned {responseInternal?.StatusCode} in {totalSeconds} seconds";
                _logger?.LogTrace(msg);
                _originalLogger?.LogInformation(msg);

                responseInternal.EnsureSuccessStatusCode();

                return responseInternal;
            });

            var content = response != null ? await response.Content.ReadAsStringAsync()
                .ConfigureAwait(false) : null;

            if (response?.IsSuccessStatusCode != true)
            {
                throw new Exception($"Unable to get funding data from API. Uri: {uri}, Response: {response?.ToString()}, Content: {content}");
            }

            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

        private AsyncRetryPolicy RetryPolicy()
        {
            return Policy
                .Handle<HttpRequestException>()
                .WaitAndRetryAsync(MaxRetryAttempts, i => pauseBetweenFailures);
        }
    }
}