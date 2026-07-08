using System;
using System.ComponentModel;

namespace GitIssue.Values
{
    /// <summary>
    ///     Extension methods for values
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
                TypeConverter converter = TypeDescriptor.GetConverter(typeof(T2));
                if (converter.CanConvertFrom(typeof(T1)))
                {
                    if (input != null)
                    {
                        object? converted = converter.ConvertFrom(input);
                        if (converted != null)
                        {
                            value = (T2)converted;
                            return true;
                        }
                    }
                }

                if (converter.CanConvertFrom(typeof(string)))
                {
                    string? strvalue = input?.ToString();
                    if (strvalue != null)
                    {
                        object? converted = converter.ConvertFrom(strvalue);
                        if (converted != null)
                        {
                            value = (T2)converted;
                            return true;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // ignored conversion errors
            }

            value = default(T2)!;
            return false;
        }

        internal static bool TryParse<T1, T2>(T1 input, out T2 value, string metadata)
        {
            if (input is T2 result)
            {
                value = result;
                return true;
            }

            string? str = input?.ToString();
            if ((str != null) && ValueExtensions.TryParse(new ValueMetadata(str, metadata), out T2 converted))
            {
                value = converted;
                return true;
            }

            value = default(T2)!;
            return false;
        }
    }
}