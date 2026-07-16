using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pds_azurefunction_fundingfeedreader.Enums;
using Pds_azurefunction_fundingfeedreader.Helpers;

namespace Test.UnitTests
{
    [TestClass, TestCategory("Unit")]
    public class DisplayTextHelperTests
    {
        [TestMethod]
        [DataRow(RunTrigger.ServiceBus, "Service bus message")]
        [DataRow(RunTrigger.Http, "Manual request")]
        [DataRow(RunTrigger.Timer, "Auto pull")]
        [DataRow(RunTrigger.None, "None")]
        public void OutputDisplayTextAttribute_ExpectedResult(RunTrigger trigger, string expected)
        {
            //Arrange
            //Act
            //Assert
            trigger.ToDisplayText().Should().Be(expected);
        }
    }
}
