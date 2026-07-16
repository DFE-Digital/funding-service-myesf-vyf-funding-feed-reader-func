using Clients;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace Test.UnitTests
{
    [TestClass]
    public class AuthenticationServiceTests
    {
        [TestMethod, TestCategory("Unit")]
        public void GetAccessToken_InvalidCredentialsSupplied_ThenAnExceptionShouldBeThrown()
        {
            // Arrange
            var authority = "https://login.microsoftonline.com/";
            var tenantId = "tenantId";
            var clientId = "clientId";
            var clientSecret = "clientSecret";
            var appIdUr = "https://myFundingServices.com";

            var authService = new AuthenticationService(authority, tenantId, clientId, clientSecret, appIdUr);

            // Act
            Func<Task> act = () => authService.GetAccessToken();

            // Assert
            act.Should().Throw<Exception>();
        }
    }
}