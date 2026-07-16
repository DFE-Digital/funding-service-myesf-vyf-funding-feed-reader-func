using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Threading.Tasks;

namespace Clients
{
    /// <summary>
    /// Authentication service for the Funding's API.
    /// </summary>
    public class AuthenticationService : IAuthenticationService
    {
        private readonly string _authority;
        private readonly string _tenantId;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _appIdUri;

        private AuthenticationResult _accessToken = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationService"/> class.
        /// Authentication service that will return an authentication token.
        /// </summary>
        /// <param name="authority">authority url.</param>
        /// <param name="tenantId">tenantId key.</param>
        /// <param name="clientId">clientId key.</param>
        /// <param name="clientSecret">Client secret.</param>
        /// <param name="appIdUri">Application Uri.</param>
        public AuthenticationService(string authority, string tenantId, string clientId, string clientSecret, string appIdUri)
        {
            _authority = authority;
            _tenantId = tenantId;
            _clientId = clientId;
            _clientSecret = clientSecret;
            _appIdUri = appIdUri;
        }

        /// <summary>
        /// Get an access token to use for the funding's Api.
        /// </summary>
        /// <returns>An access token.</returns>
        public async Task<string> GetAccessToken()
        {
            // Access token not set yet or access token has expired.
            if (_accessToken == null || _accessToken.ExpiresOn <= DateTime.UtcNow)
            {
                _accessToken = await RequestAccessToken().ConfigureAwait(false);
            }

            return _accessToken.AccessToken;
        }

        /// <summary>
        /// Request an access token that can be used with the CFS Api.
        /// </summary>
        /// <returns>An auth result and the token.</returns>
        private async Task<AuthenticationResult> RequestAccessToken()
        {
            var authContext = new AuthenticationContext($"{_authority}{_tenantId}");
            var clientCredential = new ClientCredential(_clientId, _clientSecret);

            var token = await authContext.AcquireTokenAsync(_appIdUri, clientCredential).ConfigureAwait(false);

            if (token == null)
            {
                throw new Exception("Access token cannot be acquired");
            }

            return token;
        }
    }
}