using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pds_azurefunction_fundingfeedreader.Helpers;

namespace Test.UnitTests.Helpers
{
    [TestClass]
    public class FeedReaderInputUriHelperTests
    {
        [DataRow("https://app-t1dv-external-v2.azurewebsites.net", true)]
        [DataRow("https://app-t1te-external-v2.azurewebsites.net", true)]
        [DataRow("https://app-t1sb-external-v2.azurewebsites.net", true)]
        [DataRow("https://app-t1in-external-v2.azurewebsites.net", true)]
        [DataRow("https://pp-api-calculate-funding.education.gov.uk", true)]
        [DataRow("https://pds-mocks-dev.azurewebsites.net/", false)]
        [DataRow("", false)]
        [TestMethod]
        public void IsCFSUri_Success(string fundingsApiUri, bool expectedResult)
        {
            //Act
            var output = fundingsApiUri.IsCFSUri();

            //Assert
            output.Should().Be(expectedResult);
        }

        [DataRow("https://app-t1dv-external-v2.azurewebsites.net", false)]
        [DataRow("https://app-t1te-external-v2.azurewebsites.net", false)]
        [DataRow("https://app-t1sb-external-v2.azurewebsites.net", false)]
        [DataRow("https://app-t1in-external-v2.azurewebsites.net", false)]
        [DataRow("https://pp-api-calculate-funding.education.gov.uk", false)]
        [DataRow("https://pds-mocks-dev.azurewebsites.net/", true)]
        [DataRow("", false)]
        [TestMethod]
        public void IsMockUri_Success(string fundingsApiUri, bool expectedResult)
        {
            //Act
            var output = fundingsApiUri.IsMockUri();

            //Assert
            output.Should().Be(expectedResult);
        }

        [DataRow("https://app-t1dv-external-v2.azurewebsites.net/", "api/v3", null, "/funding/provider/{0}", "https://app-t1dv-external-v2.azurewebsites.net/api/v3/funding/provider/{0}")]
        [DataRow("https://app-t1dv-external-v2.azurewebsites.net", "api/v3", "", "funding/provider/{0}", "https://app-t1dv-external-v2.azurewebsites.net/api/v3/funding/provider/{0}")]
        [DataRow("https://app-t1dv-external-v2.azurewebsites.net", "api/v4", "statements", "funding/provider/{0}", "https://app-t1dv-external-v2.azurewebsites.net/api/v4/statements/funding/provider/{0}")]
        [DataRow("https://app-t1dv-external-v2.azurewebsites.net", "api/v4", "/statements/", "funding/provider/{0}", "https://app-t1dv-external-v2.azurewebsites.net/api/v4/statements/funding/provider/{0}")]
        [DataRow("https://app-t1dv-external-v2.azurewebsites.net", "/api/v4/", "statements", "funding/provider/{0}", "https://app-t1dv-external-v2.azurewebsites.net/api/v4/statements/funding/provider/{0}")]
        [DataRow("https://app-t1dv-external-v2.azurewebsites.net/", "api/v4", "statements/", "/funding/provider/{0}", "https://app-t1dv-external-v2.azurewebsites.net/api/v4/statements/funding/provider/{0}")]
        [DataRow("https://app-t1dv-external-v2.azurewebsites.net/", "api/v3", "/", "/funding/provider/{0}", "https://app-t1dv-external-v2.azurewebsites.net/api/v3/funding/provider/{0}")]
        [TestMethod]
        public void CombineUri_Success(string fundingsApiUri, string apiVersionText, string channel, string path, string expectedResult)
        {
            //Act
            var output = fundingsApiUri.CombineUri(apiVersionText, channel, path);

            //Assert
            output.Should().Be(expectedResult);
        }

        [DataRow("https://app-t1dv-external-v2.azurewebsites.net/", "api/v4", "statements", "funding/notifications{0}", 250, "1619", true, "https://app-t1dv-external-v2.azurewebsites.net/api/v4/statements/funding/notifications{0}?pageSize=250&fundingStreamIds=1619")]
        [DataRow("https://app-t1dv-external-v2.azurewebsites.net", "api/v4", "statements", "funding/notifications{0}", 250, "", true, "https://app-t1dv-external-v2.azurewebsites.net/api/v4/statements/funding/notifications{0}?pageSize=250")]
        [DataRow("https://app-t1dv-external-v2.azurewebsites.net/", "api/v4/", "statements", "funding/notifications{0}", 250, null, true, "https://app-t1dv-external-v2.azurewebsites.net/api/v4/statements/funding/notifications{0}?pageSize=250")]
        [DataRow("https://app-t1dv-external-v2.azurewebsites.net/", "/api/v4", "statements/", "funding/notifications{0}", 0, "1619", true, "https://app-t1dv-external-v2.azurewebsites.net/api/v4/statements/funding/notifications{0}?fundingStreamIds=1619")]
        [TestMethod]
        public void GetOriginalFundingUri_Success(
                                string fundingsApiUri,
                                string apiVersionText,
                                string channel,
                                string path,
                                int numberOfFundingsToRetrieveFromApi,
                                string fundingStream,
                                bool isCFSUri,
                                string expectedResult)
        {
            //Act
            var output = fundingsApiUri.GetOriginalFundingUri(
                                        new[] { apiVersionText, channel, path },
                                        numberOfFundingsToRetrieveFromApi,
                                        fundingStream,
                                        isCFSUri);

            //Assert
            output.Should().Be(expectedResult);
        }

        [DataRow("https://app-t1dv-external-v2.azurewebsites.net/api/v4/statements/funding/notifications{0}?pageSize=250", null, null, true, "https://app-t1dv-external-v2.azurewebsites.net/api/v4/statements/funding/notifications{0}?pageSize=250")]
        [DataRow("https://app-t1dv-external-v2.azurewebsites.net/api/v4/statements/funding/notifications{0}", null, null, true, "https://app-t1dv-external-v2.azurewebsites.net/api/v4/statements/funding/notifications{0}")]
        [DataRow("https://app-t1dv-external-v2.azurewebsites.net/api/v4/statements/funding/notifications{0}?pageSize=250", "AY-2223", null, true, "https://app-t1dv-external-v2.azurewebsites.net/api/v4/statements/funding/notifications{0}?pageSize=250&fundingPeriodIds=AY-2223")]
        [DataRow("https://app-t1dv-external-v2.azurewebsites.net/api/v4/statements/funding/notifications{0}?pageSize=250&fundingStreamIds=1619", "AY-2223", null, true, "https://app-t1dv-external-v2.azurewebsites.net/api/v4/statements/funding/notifications{0}?pageSize=250&fundingStreamIds=1619&fundingPeriodIds=AY-2223")]
        [DataRow("https://app-t1dv-external-v2.azurewebsites.net/api/v4/statements/funding/notifications{0}?pageSize=250&fundingStreamIds=1619", "AY-2223", "1", true, "https://app-t1dv-external-v2.azurewebsites.net/api/v4/statements/funding/notifications{0}?pageSize=250&fundingStreamIds=1619&fundingPeriodIds=AY-2223&scenarioId=1")]
        [DataRow("https://app-t1dv-external-v2.azurewebsites.net/api/v4/statements/funding/notifications{0}?pageSize=250&fundingStreamIds=1619", null, "1", true, "https://app-t1dv-external-v2.azurewebsites.net/api/v4/statements/funding/notifications{0}?pageSize=250&fundingStreamIds=1619&scenarioId=1")]
        [DataRow("https://app-t1dv-external-v2.azurewebsites.net/api/v4/statements/funding/notifications{0}?pageSize=250", "AY-2223", null, false, "https://app-t1dv-external-v2.azurewebsites.net/api/v4/statements/funding/notifications{0}?pageSize=250&fundingPeriod=AY-2223")]
        [DataRow("https://app-t1dv-external-v2.azurewebsites.net/api/v4/statements/funding/notifications{0}?pageSize=250&fundingStreamIds=1619", "AY-2223", null, false, "https://app-t1dv-external-v2.azurewebsites.net/api/v4/statements/funding/notifications{0}?pageSize=250&fundingStreamIds=1619&fundingPeriod=AY-2223")]
        [DataRow("https://app-t1dv-external-v2.azurewebsites.net/api/v4/statements/funding/notifications{0}?pageSize=250&fundingStreamIds=1619", "AY-2223", "1", false, "https://app-t1dv-external-v2.azurewebsites.net/api/v4/statements/funding/notifications{0}?pageSize=250&fundingStreamIds=1619&fundingPeriod=AY-2223&scenarioId=1")]
        [DataRow("https://app-t1dv-external-v2.azurewebsites.net/api/v4/statements/funding/notifications{0}?pageSize=250&fundingStreamIds=1619", "", "1", false, "https://app-t1dv-external-v2.azurewebsites.net/api/v4/statements/funding/notifications{0}?pageSize=250&fundingStreamIds=1619&scenarioId=1")]
        [TestMethod]
        public void GetFeedFundingUri_Success(
                                string originalFundingUri,
                                string fundingPeriod,
                                string scenarioId,
                                bool isCFSUri,
                                string expectedResult)
        {
            //Act
            var output = originalFundingUri.GetFeedFundingUri(
                                        fundingPeriod,
                                        scenarioId,
                                        isCFSUri);

            //Assert
            output.Should().Be(expectedResult);
        }
    }
}
