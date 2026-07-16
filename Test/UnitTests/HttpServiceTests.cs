using Clients;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Test.UnitTests
{
    [TestClass]
    public class HttpServiceTests
    {
        private readonly Mock<HttpMessageHandler> _mockMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        private readonly Mock<IAuthenticationService> _mockAuthService = new Mock<IAuthenticationService>(MockBehavior.Strict);

        #region Tests

        [TestMethod, TestCategory("Unit")]
        public async Task GetAsync_AuthenticationServiceTurnedOffValidHttpRequest_EnsureHttpServiceReturnsCorrectMessageBody()
        {
            // Arrange
            CreateMockHandler(HttpStatusCode.OK, "StringContent");

            // use real HTTP client with mocked handler.
            var httpClient = new HttpClient(_mockMessageHandler.Object);

            string url = "http://localhost:51862";
            var httpService = new HttpService(httpClient, null, null, null);

            // Act
            var result = await httpService.GetAsync(url);

            // Assert
            result.Should().Be("StringContent");

            // Also, ensure HTTP call was as expected.
            var expectedUri = new Uri(url);

            _mockMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Exactly(1), // expect a single external request
                ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get // we expect a GET request
                        && req.RequestUri == expectedUri), // to this uri
                ItExpr.IsAny<CancellationToken>());
        }

        [TestMethod, TestCategory("Unit")]
        public void GetAsync_AuthenticationServiceTurnedOffAndInvalidHttpRequest_EnsureHttpServiceThrowsAnException()
        {
            // Arrange
            CreateMockHandler(HttpStatusCode.BadRequest, string.Empty);

            // use real HTTP client with mocked handler.
            var httpClient = new HttpClient(_mockMessageHandler.Object);
            var httpService = new HttpService(httpClient, null, null, null);

            string url = "http://localhost:51862";

            // Act
            Func<Task> act = () => httpService.GetAsync(url);

            // Assert
            act.Should().Throw<Exception>();
        }

        [TestMethod, TestCategory("Unit")]
        public async Task GetAsync_AuthenticationServiceTurnedOnAndValidHttpRequest_EnsureHttpServiceReturnsCorrectMessageBody()
        {
            // Arrange
            CreateMockHandler(HttpStatusCode.OK, "ResponseText");

            // use real HTTP client with mocked message handler.
            var httpClient = new HttpClient(_mockMessageHandler.Object);

            string url = "http://localhost:51862";

            _mockAuthService
                .Setup(x => x.GetAccessToken()).ReturnsAsync("token");

            var httpService = new HttpService(httpClient, _mockAuthService.Object, null, null);

            // Act
            var result = await httpService.GetAsync(url);

            // Assert
            result.Should().Be("ResponseText");
            _mockAuthService.Verify(x => x.GetAccessToken(), Times.Once);
        }

        [TestMethod, TestCategory("Unit")]
        public void GetAsync_AuthenticationServiceTurnedOnAndInvalidHttpRequest_EnsureHttpServiceThrowsAnException()
        {
            // Arrange
            CreateMockHandler(HttpStatusCode.BadRequest, string.Empty);

            // use real HTTP client with mocked handler.
            var httpClient = new HttpClient(_mockMessageHandler.Object);

            _mockAuthService
                .Setup(x => x.GetAccessToken()).ReturnsAsync("token");

            var httpService = new HttpService(httpClient, _mockAuthService.Object, null, null);
            var url = "http://localhost:51862";

            // Act
            Func<Task> act = () => httpService.GetAsync(url);

            // Assert
            act.Should().Throw<Exception>();
            _mockAuthService.Verify(x => x.GetAccessToken(), Times.Exactly(4));
        }

        [TestMethod, TestCategory("Unit")]
        public void GetAsync_AuthenticationServiceTurnedOnAndInvalidCredentialsSupplied_EnsureHttpServiceThrowsAnException()
        {
            // Arrange
            CreateMockHandler(HttpStatusCode.OK, "ResponseText");

            // use real HTTP client with mocked message handler.
            var httpClient = new HttpClient(_mockMessageHandler.Object);
            var url = "http://localhost:51862";

            _mockAuthService
                .Setup(x => x.GetAccessToken()).Throws<Exception>();

            var httpService = new HttpService(httpClient, _mockAuthService.Object, null, null);

            Func<Task> act = () => httpService.GetAsync(url);

            act.Should().Throw<Exception>();
            _mockAuthService.Verify(x => x.GetAccessToken(), Times.Once);
        }

        #endregion


        #region Helper Methods

        /// <summary>
        /// Set mock expectations for the HTTP message handler.
        /// </summary>
        /// <param name="statusCode">Status code to return from HTTP response message.</param>
        /// <param name="content">HTTP response message text.</param>
        private void CreateMockHandler(HttpStatusCode statusCode, string content)
        {
            _mockMessageHandler
                .Protected()

                // Setup the protected method to mock
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()) // expected response of the mocked http call
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = statusCode,
                    Content = new StringContent(content),
                });
        }

        #endregion
    }
}
