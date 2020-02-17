using System;
using System.ComponentModel;
using System.Globalization;

namespace GitIssue.Values
{
    /// <summary>
    ///     TypeConverter for FieldType and String
    /// </summary>
    public class TypeValueTypeConverter : TypeConverter
    {
        /// <inheritdoc />
        public override bool CanConvertFrom(ITypeDescriptorContext context,
            Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }

        /// <inheritdoc />
        public override object? ConvertFrom(ITypeDescriptorContext context,
            CultureInfo culture, object value)
        {
            if (value is string str)
                if (TypeValue.TryParse(str, out var type))
                    return type;

            return base.ConvertFrom(context, culture, value);
        }

        /// <inheritdoc />
        public override object? ConvertTo(ITypeDescriptorContext context,
            CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
                if (value is TypeValue type)
                    return type.ToString();
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}