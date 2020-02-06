using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace GitIssue.Values
{
    /// <summary>
    /// Extension methods for values
    /// </summary>
    public static class ValueExtensions
    {
        /// <summary>
        ///     Tries to parse the string input to the output value
        /// </summary>
        /// <param name="input">the input string</param>
        /// <param name="value">the output value</param>
        /// <returns></returns>
        internal static bool TryParse<T>(string input, out T value)
        {
            try
            {
                var converter = TypeDescriptor.GetConverter(typeof(T));
                if (converter.CanConvertFrom(typeof(string)))
                {
                    value = (T) converter.ConvertFrom(input);
                    return true;
                }

            }
            catch (Exception e)
            {
                // ignored conversion errors
            }
            value = default;
            return false;
        }
    }
}
