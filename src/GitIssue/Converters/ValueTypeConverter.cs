using System;
using System.ComponentModel;
using System.Globalization;
using GitIssue.Values;

namespace GitIssue.Converters
{
    /// <summary>
    /// Base class for value type converters
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ValueTypeConverter<T> : TypeConverter where T : IValue
    {
        /// <inheritdoc />
        public override bool CanConvertFrom(ITypeDescriptorContext context,
            Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;

            if (sourceType == typeof(ValueMetadata))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }

        /// <inheritdoc />
        public override object ConvertFrom(ITypeDescriptorContext context,
            CultureInfo culture, object value)
        {
            if (value is string str)
                if (this.TryParse(str, out T result))
                    return result;

            if (value is ValueMetadata valueMetadata)
                if (this.TryParse(valueMetadata, out T result))
                    return result;

            return base.ConvertFrom(context, culture, value);
        }

        /// <inheritdoc />
        public override object ConvertTo(ITypeDescriptorContext context,
            CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
                if (value is T result)
                    return result.ToString();

            return base.ConvertTo(context, culture, value, destinationType);
        }

        /// <summary>
        /// Tries to parse the value
        /// </summary>
        /// <param name="input">the input string</param>
        /// <param name="result">the resulting value</param>
        /// <returns></returns>
        public abstract bool TryParse(string input, out T result);

        /// <summary>
        /// Tries to parse a value including it's metadata
        /// </summary>
        /// <param name="input"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public abstract bool TryParse(ValueMetadata input, out T result);
    }
}