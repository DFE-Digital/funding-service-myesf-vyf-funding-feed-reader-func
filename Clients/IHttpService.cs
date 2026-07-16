using System.Threading.Tasks;

namespace Clients
{
    /// <summary>
    /// Proxy class for HttpClient.
    /// </summary>
    public interface IHttpService
    {
        /// <summary>
        /// Make an async call to a Uri.
        /// </summary>
        /// <param name="uri">The URI of the page to call.</param>
        /// <returns>Return Task containing HTTP response.</returns>
        Task<string> GetAsync(string uri);
    }
}