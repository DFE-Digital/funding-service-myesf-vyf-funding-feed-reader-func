using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pds_azurefunction_fundingfeedreader.Helpers;

namespace Test.UnitTests
{
    [TestClass, TestCategory("Unit")]
    public class UrlHelperTests
    {
        [DataRow("http://url?test", true, "1619", "http://url?test&fundingStreamIds=1619")]
        [DataRow("http://url?test", true, "GAG", "http://url?test&fundingStreamIds=GAG")]
        [DataRow("http://url?test", false, "1619,GAG", "http://url?test&fundingStreamCodes=1619,GAG")]
        [DataRow("http://url?test", false, "GAG", "http://url?test&fundingStreamCodes=GAG")]
        [TestMethod]
        public void AddFundingStreamCodesToOriginalFundingUri_ExpectedResult(
            string originalUrl,
            bool isCfs,
            string fundingStreamCodes,
            string expected)
        {
            // Arrange
            // Act
            var result = originalUrl.AddFundingStreamCodeToOriginalFundingUri(fundingStreamCodes, isCfs);

            // Assert
            result.Should().Be(expected);
        }

        [DataRow("http://url?test", "&", "fundingStreamCodes", "1619,GAG", "http://url?test&fundingStreamCodes=1619,GAG")]
        [DataRow("http://url?test", "&", "fundingStreamCodes", "GAG", "http://url?test&fundingStreamCodes=GAG")]
        [DataRow("http://url?test", "&", "", "GAG", "http://url?test")]
        [DataRow("http://url?test", "&", "fundingStreamCodes", "", "http://url?test")]
        [TestMethod]
        public void AddQuerystringParams_ExpectedResult(
            string inputString,
            string separator,
            string key,
            string value,
            string expected)
        {
            // Arrange
            // Act
            var result = UrlHelper.AddQuerystringParams(inputString, separator, key, value);

            // Assert
            result.Should().Be(expected);
        }
    }
}