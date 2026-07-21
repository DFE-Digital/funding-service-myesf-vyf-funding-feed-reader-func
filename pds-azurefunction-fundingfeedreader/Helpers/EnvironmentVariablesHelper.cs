using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pds_azurefunction_fundingfeedreader.Helpers
{
    /// <summary>
    /// A Helper class to Local Settings.
    /// </summary>
    public static class EnvironmentVariablesHelper
    {
        /// <summary>
        /// To get settings from Environment.
        /// </summary>
        /// <param name="key">key value.</param>
        /// <param name="defaultValue">Default Value incase Environment value is missing.</param>
        /// <exception cref="ArgumentNullException">Throws ArgumentNullException if key is null.</exception>
        /// <returns>Return Settings details.</returns>
        public static string GetSettings(string key, string defaultValue = null)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var value = Environment.GetEnvironmentVariable(key);

            if (string.IsNullOrWhiteSpace(value))
            {
                value = defaultValue;
            }

            return value;
        }

        /// <summary>
        /// To get settings from Environment.
        /// </summary>
        /// <param name="key">key value.</param>
        /// <param name="defaultValue">Default value.</param>
        /// <exception cref="ArgumentNullException">Throws ArgumentNullException if key is null.</exception>
        /// <returns>Return Settings details.</returns>
        public static int GetSettingsAsInt(string key, int defaultValue)
        {
            return int.TryParse(GetSettings(key), out var value) ? value : defaultValue;
        }

        /// <summary>
        /// To get settings from Environment.
        /// </summary>
        /// <param name="key">key value.</param>
        /// <param name="defaultValue">Default value.</param>
        /// <exception cref="ArgumentNullException">Throws ArgumentNullException if key is null.</exception>
        /// <returns>Return Settings details.</returns>
        public static bool GetSettingsAsBool(string key, bool defaultValue)
        {
            return bool.TryParse(GetSettings(key), out var value) ? value : defaultValue;
        }

        /// <summary>
        /// To validate the LocalSettingModel.
        /// </summary>
        /// <param name="localSettingsModel">The LocalSettingModel object.</param>
        /// <returns>
        ///     isValid: true if all mandatory properties are set in the LocalSettingModel.
        ///     missingProperties: All String Properties whose values are null/Empty/whitespace.
        /// </returns>
        public static (bool isValid, string missingProperties) ValidateLocalSettingModel(this IEnvironmentVariablesModel localSettingsModel)
        {
            var missingProperties = new List<string>();
            foreach (var pi in localSettingsModel.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
            {
                if (pi.PropertyType == typeof(string) && string.IsNullOrWhiteSpace(Convert.ToString(pi.GetValue(localSettingsModel))))
                {
                    missingProperties.Add(pi.Name);
                }
            }

            bool isValid = missingProperties.Count() == 0;
            string strMissingProperties = isValid == true ? "No missing Properties" : missingProperties?.Aggregate((a, b) => $"{a}, {b}");

            return (isValid, strMissingProperties);
        }
    }
}
