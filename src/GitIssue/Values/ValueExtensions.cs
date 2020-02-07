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
        internal static bool TryParse<T1, T2>(T1 input, out T2 value)
        {
            if (input is T2 result)
            {
                value = result;
                return true;
            }

            try
            {
                var converter = TypeDescriptor.GetConverter(typeof(T2));
                if (converter.CanConvertFrom(typeof(T1)))
                {
                    value = (T2)converter.ConvertFrom(input);
                    return true;
                }
                if (converter.CanConvertFrom(typeof(string)))
                {
                    value = (T2)converter.ConvertFrom(input.ToString());
                    return true;
                }
            }
            catch (Exception)
            {
                // ignored conversion errors
            }

            value = default;
            return false;
        }

        internal static bool TryParse<T1, T2>(T1 input, out T2 value, string metadata)
        {
            if (input is T2 result)
            {
                value = result;
                return true;
            }

            if (TryParse(new ValueMetadata(input?.ToString(), metadata), out T2 converted))
            {
                value = converted;
                return true;
            }

            value = default;
            return false;
        }
    }
}
