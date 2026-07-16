using System.Threading.Tasks;

namespace Clients
{
    /// <summary>
    /// Authentication service.
    /// </summary>
    public interface IAuthenticationService
    {
        /// <summary>
        /// Get an access token to use for the fundings Api.
        /// </summary>
        /// <returns>An the authentication token.</returns>
        Task<string> GetAccessToken();
    }
}