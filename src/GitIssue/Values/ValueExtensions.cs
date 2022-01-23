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
                var converter = TypeDescriptor.GetConverter(typeof(T2));
                if (converter.CanConvertFrom(typeof(T1)))
                {
                    if(input != null)
                    {
                        var converted = converter.ConvertFrom(input);
                        if (converted != null)
                        {
                            value = (T2)converted;
                            return true;
                        }
                    }
                }

                if (converter.CanConvertFrom(typeof(string)))
                {
                    var strvalue = input?.ToString();
                    if(strvalue != null)
                    {
                        var converted = converter.ConvertFrom(strvalue);
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

            value = default!;
            return false;
        }

        internal static bool TryParse<T1, T2>(T1 input, out T2 value, string metadata)
        {
            if (input is T2 result)
            {
                value = result;
                return true;
            }

            var str = input?.ToString();
            if (str != null && TryParse(new ValueMetadata(str, metadata), out T2 converted))
            {
                value = converted;
                return true;
            }

            value = default!;
            return false;
        }
    }
}