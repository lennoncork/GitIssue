using System;
using System.ComponentModel;
using System.Globalization;

namespace GitIssue.Issues
{
    /// <summary>
    ///     TypeConverter for FieldKey and String
    /// </summary>
    public class IssuedKeyTypeConverter : TypeConverter
    {
        /// <inheritdoc />
        public override bool CanConvertFrom(ITypeDescriptorContext context,
            Type sourceType)
        {
            if (sourceType == typeof(string)) return true;
            return base.CanConvertFrom(context, sourceType);
        }

        /// <inheritdoc />
        public override object ConvertFrom(ITypeDescriptorContext context,
            CultureInfo culture, object value)
        {
            if (value is string str) return IssueKey.Create(str);
            return base.ConvertFrom(context, culture, value);
        }

        /// <inheritdoc />
        public override object ConvertTo(ITypeDescriptorContext context,
            CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
                if (value is IssueKey key)
                    return key.ToString();
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}