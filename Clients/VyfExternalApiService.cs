using ApplicationLogger;
using Clients.Exceptions;
using Clients.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Threading.Tasks;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Clients
{
    /// <summary>
    /// Interaction service for Vyf's external api.
    /// </summary>
    public class VyfExternalApiService : IVyfExternalApiService
    {
        private readonly string _vyfBaseUri;
        private readonly string _vyfApiKey;
        private readonly string _autoPullEndpointUri;
        private readonly string _fullAutoPullEndpointUri;

        /// <summary>
        /// Initializes a new instance of the <see cref="VyfExternalApiService"/> class.
        /// </summary>
        /// <param name="vyfBaseUri">Base Uri for Vyf external api.</param>
        /// <param name="vyfApiKey">Secret key for Vyf external api.</param>
        /// <param name="autoPullEndpointUri">Uri for GetAutoPullConfiguredFundingStreams endpoint from VYF external Api.</param>
        public VyfExternalApiService(string vyfBaseUri, string vyfApiKey, string autoPullEndpointUri)
        {
            _vyfBaseUri = vyfBaseUri;
            _vyfApiKey = vyfApiKey;
            _autoPullEndpointUri = autoPullEndpointUri;
            _fullAutoPullEndpointUri = $"{_vyfBaseUri}{_autoPullEndpointUri}";
        }

        /// <summary>
        /// Returns the funding streams configured for auto pull from VYF external API.
        /// </summary>
        /// <param name="httpClient">Http client instance.</param>
        /// <param name="logger">App Insights logger.</param>
        /// <param name="originalLogger">Trigger method logger.</param>
        /// <returns>Comma seperated string of funding streams configured for auto pull.</returns>
        public async Task<string> GetAutoPullFundingStreams(HttpClient httpClient, ApplicationLogger.ILogger logger, ILogger originalLogger)
        {
            httpClient.DefaultRequestHeaders.Add("x-secret-key", _vyfApiKey);
            httpClient.DefaultRequestHeaders.Add(HeaderNames.Accept, MediaTypeNames.Application.Json);
            httpClient.BaseAddress = new Uri(_fullAutoPullEndpointUri);

            try
            {
                LogInfo(logger, originalLogger, "Collecting funding streams configured for auto pull.");

                var response = await httpClient.GetAsync(_fullAutoPullEndpointUri).ConfigureAwait(false);
                if (response != null)
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new VyfExternalApiException($"Error occured while collecting auto pull configured funding streams for uri: {httpClient.BaseAddress?.ToString()}. HttpStatusCode = {response.StatusCode}.");
                    }

                    var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        var autoPullFundingStreamResults = JsonConvert.DeserializeObject<IEnumerable<AutoPullFundingStreamResult>>(content).ToList();

                        if (autoPullFundingStreamResults != null && autoPullFundingStreamResults?.Count() > 0)
                        {
                            var fundingStreamsCommaSeperated = string.Join(',', autoPullFundingStreamResults.Select(x => x.FundingStreamCode));
                            LogInfo(logger, originalLogger, $"{autoPullFundingStreamResults.Count()} funding streams configured for auto pull: {fundingStreamsCommaSeperated}. Continue to run.");
                            return fundingStreamsCommaSeperated;
                        }
                        else
                        {
                            LogInfo(logger, originalLogger, "No funding streams configured for auto pull. Do not continue to run.");
                            return null;
                        }
                    }
                    else
                    {
                        throw new VyfExternalApiException($"Error occured while collecting auto pull configured funding streams for uri: {httpClient.BaseAddress?.ToString()}. Content is empty.");
                    }
                }
                else
                {
                    throw new VyfExternalApiException($"Error occured while collecting auto pull configured funding streams for uri: {httpClient.BaseAddress?.ToString()}. Response is null.");
                }
            }
            catch (Exception ex)
            {
                LogError(logger, originalLogger, ex, ex.Message);
                throw;
            }
            finally
            {
                httpClient.DefaultRequestHeaders.Remove("x-secret-key");
                httpClient.DefaultRequestHeaders.Remove(HeaderNames.Accept);
            }
        }

        private void LogError(ApplicationLogger.ILogger logger1, ILogger logger2, Exception ex, string errorMessage)
        {
            logger1?.LogException(ex, errorMessage);
            logger2?.LogError(ex, errorMessage);
        }

        private void LogInfo(ApplicationLogger.ILogger logger1, ILogger logger2, string message)
        {
            logger1?.LogTrace(message);
            logger2?.LogInformation(message);
        }
    }
}
