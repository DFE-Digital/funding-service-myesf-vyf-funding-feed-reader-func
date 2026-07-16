using Pds_azurefunction_fundingfeedreader.Attributes;
using System;
using System.Reflection;

namespace Pds_azurefunction_fundingfeedreader.Helpers
{
    /// <summary>
    /// The DisplayTextHelper class.
    /// </summary>
    public static class DisplayTextHelper
    {
        /// <summary>
        /// Helper extension method to collect the DisplayText attribute Text value from the enum.
        /// </summary>
        /// <param name="e">The enum.</param>
        /// <returns>The DisplayText attribute Text value if one is found or the string representation of the enum if DisplayText attribute is not found.</returns>
        public static string ToDisplayText(this Enum e)
        {
            Type type = e.GetType();

            MemberInfo[] memInfo = type.GetMember(e.ToString());

            if (memInfo != null && memInfo.Length > 0)
            {
                object[] attrs = memInfo[0].GetCustomAttributes(typeof(DisplayText), false);
                if (attrs != null && attrs.Length > 0)
                {
                    return ((DisplayText)attrs[0]).Text;
                }
            }

            return e.ToString();
        }
    }
}
