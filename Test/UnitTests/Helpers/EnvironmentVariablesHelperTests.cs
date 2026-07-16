using Domain.Interfaces;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Pds_azurefunction_fundingfeedreader.Helpers;
using Pds_azurefunction_fundingfeedreader.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.UnitTests.Helpers
{
    [TestClass]
    public class EnvironmentVariablesHelperTests
    {
        [TestMethod]
        public void ValidateLocalSettingModel_Success()
        {
            //Arange
            SetupEnvironment(false);
            IEnvironmentVariablesModel localSettingsModel = new EnvironmentVariablesModel();

            //Act
            var output = localSettingsModel.ValidateLocalSettingModel();

            //Assert
            output.Should().Be((true, "No missing Properties"));
        }

        [DataRow("AppInsightsConnectionString, CosmosAuditCollectionName", "APPLICATIONINSIGHTS_CONNECTION_STRING", "cdb:auditCollectionName")]
        [DataRow("AppInsightsConnectionString", "APPLICATIONINSIGHTS_CONNECTION_STRING", "")]
        [TestMethod]
        public void ValidateLocalSettingModel_Failure(string expectedMessage, string missoutSettingKey1, string missoutSettingKey2)
        {
            //Arange
            SetupEnvironment(false, missoutSettingKey1, missoutSettingKey2);
            IEnvironmentVariablesModel localSettingsModel = new EnvironmentVariablesModel();

            //Act
            var output = localSettingsModel.ValidateLocalSettingModel();

            //Assert
            output.Should().Be((false, expectedMessage));
        }

        [TestMethod]
        public void ValidateLocalSettingModel_FailureNoSettings()
        {
            //Arange
            SetupEnvironment(true);
            IEnvironmentVariablesModel localSettingsModel = new EnvironmentVariablesModel();
            string expectedMessage = "Environment, AppInsightsConnectionString, AsName, AsAdminKey, CosmosDbEndPoint, CosmosDbKey, CosmosDbName, CosmosFundingGroupCollectionName, CosmosProviderFundingCollectionName, CosmosAuditCollectionName, AuthAuthority, AuthTenantId, AuthClientId, AuthClientSecret, AuthAppIdUri, VyfBaseUri, AutoPullEndpointUri, VyfApiKey, FundingsApiUri, RunMode";

            //Act
            var output = localSettingsModel.ValidateLocalSettingModel();

            //Assert
            output.Should().Be((false, expectedMessage));
        }

        [DataRow("Test", "Test Value")]
        [DataRow("Test1", null)]
        [TestMethod]
        public void GetSettings_Success(string key, string value)
        {
            //Arange
            Environment.SetEnvironmentVariable(key, value);

            //Act
            var output = EnvironmentVariablesHelper.GetSettings(key);

            //Assert
            output.Should().Be(value);
        }

        [DataRow("Test", "Test Value", "Default Value", "Test Value")]
        [DataRow("Test1", null, "Default Value", "Default Value")]
        [DataRow("DummyKey", "Test", "Default Value", "Default Value")]
        [TestMethod]
        public void GetSettings_DefaultValue(string key, string value, string defaultValue, string expectedValue)
        {
            //Arange
            if (key != "DummyKey")
            {
                Environment.SetEnvironmentVariable(key, value);
            }

            //Act
            var output = EnvironmentVariablesHelper.GetSettings(key, defaultValue);

            //Assert
            output.Should().Be(expectedValue);
        }


        [TestMethod]
        public void GetSettings_NonValue()
        {
            //Act
            var output = EnvironmentVariablesHelper.GetSettings("Dummyvalue");

            //Assert
            output.Should().BeNull();
        }

        [TestMethod]
        public void GetSettings_Exception()
        {
            //Act
            Action act = () => EnvironmentVariablesHelper.GetSettings(null);

            //Assert
            act.Should().Throw<ArgumentException>();
        }

        private void SetupEnvironment(bool missoutall = false, params string[] missoutSettingsKeys)
        {
            string basePath = Path.GetFullPath(@"..\..\..\..\pds-azurefunction-fundingfeedreader");
            var settings = JsonConvert.DeserializeObject<LocalSettings>(
                File.ReadAllText(basePath + "\\local.settings.json"));

            foreach (var setting in settings.Values)
            {
                if (missoutall == false && (missoutSettingsKeys == null || !missoutSettingsKeys.Contains(setting.Key)))
                {
                    Environment.SetEnvironmentVariable(setting.Key, setting.Value);
                }
                else
                {
                    Environment.SetEnvironmentVariable(setting.Key, null);
                }
            }
        }

        private class LocalSettings
        {
            public bool IsEncrypted { get; set; }

            /// <summary>
            /// Gets or Sets of Values.
            /// </summary>
            public Dictionary<string, string> Values { get; set; }
        }
    }
}
