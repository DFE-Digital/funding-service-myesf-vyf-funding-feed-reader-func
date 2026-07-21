using System.Net.Http;
using System.Threading.Tasks;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Clients
{
    /// <summary>
    /// Vyf External Api Service.
    /// </summary>
    public interface IVyfExternalApiService
    {
        /// <summary>
        /// Gets all auto pull configured funding streams from Vyf external api.
        /// </summary>
        /// <param name="httpClient">HttpClient instance.</param>
        /// <param name="logger">App Insights logger.</param>
        /// <param name="originalLogger">Azure trigger method logger.</param>
        /// <returns>Comma seperated string of funding streams configured for auto pull.</returns>
        Task<string> GetAutoPullFundingStreams(HttpClient httpClient, ApplicationLogger.ILogger logger, ILogger originalLogger);
    }
}
