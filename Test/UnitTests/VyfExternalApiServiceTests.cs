using Clients;
using Clients.Exceptions;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Test.UnitTests
{
    [TestClass, TestCategory("Unit")]
    public class VyfExternalApiServiceTests
    {
        [TestMethod]
        [DataRow(@"[]", null)]
        [DataRow(@"[{""fundingStreamCode"":""1416"",""fundingStreamName"":""14 to 16 funding""},]", "1416")]
        [DataRow(@"[{""fundingStreamCode"":""LAREC"",""fundingStreamName"":""LA recoupment""},{""fundingStreamCode"":""UIFSM"",""fundingStreamName"":""Universal infant free school meals""}]", "LAREC,UIFSM")]
        [DataRow(@"[{""fundingStreamCode"":""1416"",""fundingStreamName"":""14 to 16 funding""},{""fundingStreamCode"":""LAREC"",""fundingStreamName"":""LA recoupment""},{""fundingStreamCode"":""UIFSM"",""fundingStreamName"":""Universal infant free school meals""}]", "1416,LAREC,UIFSM")]
        public async Task GetAutoPullFundingStreams_ExpectedResult(string apiReturn, string expectedFundingStreamsCommaSeperated)
        {
            //Arrange
            var mockHttpClient = GetMockHttpClient(apiReturn);
            var vyfExternalApiService = GetVyfExternalApiService();

            //Act
            var fundingStreamsCommaSeperated = await vyfExternalApiService.GetAutoPullFundingStreams(mockHttpClient.Object, null, null);

            //Assert
            fundingStreamsCommaSeperated.Should().Be(expectedFundingStreamsCommaSeperated);
        }

        [TestMethod]
        [ExpectedException(typeof(VyfExternalApiException), "Error occured while collecting auto pull configured funding streams for uri: https://localhost:12345/api/external/test123. Content is empty.")]
        public async Task GetAutoPullFundingStreams_InvalidContentFromApi_Null()
        {
            //Arrange
            var mockHttpClient = GetMockHttpClient(null);
            var vyfExternalApiService = GetVyfExternalApiService();

            //Act
            await vyfExternalApiService.GetAutoPullFundingStreams(mockHttpClient.Object, null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(VyfExternalApiException), "Error occured while collecting auto pull configured funding streams for uri: https://localhost:12345/api/external/test123. Content is empty.")]
        public async Task GetAutoPullFundingStreams_InvalidContentFromApi_EmptyString()
        {
            //Arrange
            var mockHttpClient = GetMockHttpClient(string.Empty);
            var vyfExternalApiService = GetVyfExternalApiService();

            //Act
            await vyfExternalApiService.GetAutoPullFundingStreams(mockHttpClient.Object, null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(VyfExternalApiException), $"Error occured while collecting auto pull configured funding streams for uri: https://localhost:12345/api/external/test123. HttpStatusCode = InternalServerError.")]
        public async Task GetAutoPullFundingStreams_FailedHttpRequest_InternalServerError()
        {
            //Arrange
            var mockHttpClient = GetMockHttpClient(string.Empty, HttpStatusCode.InternalServerError);
            var vyfExternalApiService = GetVyfExternalApiService();

            //Act
            await vyfExternalApiService.GetAutoPullFundingStreams(mockHttpClient.Object, null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(VyfExternalApiException), $"Error occured while collecting auto pull configured funding streams for uri: https://localhost:12345/api/external/test123. HttpStatusCode = Unauthorized.")]
        public async Task GetAutoPullFundingStreams_FailedHttpRequest_Unauthorized()
        {
            //Arrange
            var mockHttpClient = GetMockHttpClient(string.Empty, HttpStatusCode.Unauthorized);
            var vyfExternalApiService = GetVyfExternalApiService();

            //Act
            await vyfExternalApiService.GetAutoPullFundingStreams(mockHttpClient.Object, null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(VyfExternalApiException), $"Error occured while collecting auto pull configured funding streams for uri: https://localhost:12345/api/external/test123. HttpStatusCode = NotFound.")]
        public async Task GetAutoPullFundingStreams_FailedHttpRequest_NotFound()
        {
            //Arrange
            var mockHttpClient = GetMockHttpClient(string.Empty, HttpStatusCode.NotFound);
            var vyfExternalApiService = GetVyfExternalApiService();

            //Act
            await vyfExternalApiService.GetAutoPullFundingStreams(mockHttpClient.Object, null, null);
        }

        private VyfExternalApiService GetVyfExternalApiService()
        {
            return new VyfExternalApiService("https://localhost:12345", "secretkey123", "/api/external/test123");
        }

        private Mock<HttpClient> GetMockHttpClient(string sendResponseContent, HttpStatusCode sendResponseStatus = HttpStatusCode.OK)
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            mockHttpMessageHandler
               .Protected()
               .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(new HttpResponseMessage() { StatusCode = sendResponseStatus, Content = sendResponseContent != null ? new StringContent(sendResponseContent) : null })
               .Verifiable();

            var mockHttpClient = new Mock<HttpClient>(mockHttpMessageHandler.Object);
            return mockHttpClient;
        }
    }
}
