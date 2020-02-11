using System;
using System.ComponentModel;
using GitIssue.Converters;
using Newtonsoft.Json;

namespace GitIssue.Fields
{
    /// <summary>
    ///     Contains type information for fields
    /// </summary>
    [TypeConverter(typeof(FieldTypeTypeConverter))]
    public struct FieldType
    {
        /// <summary>
        ///     The field type used for <see cref="IField" />
        /// </summary>
        [JsonIgnore]
        public Type Type { get; }

        private FieldType(Type type)
        {
            Type = type;
        }

        /// <summary>
        ///     Tries to parse the label value
        /// </summary>
        /// <param name="value"></param>
        /// <param name="fieldType"></param>
        /// <returns></returns>
        public static bool TryParse(string value, out FieldType fieldType)
        {
            try
            {
                var type = Type.GetType(value);
                fieldType = FieldType.Create(type);
                return true;
            }
            catch (TypeLoadException)
            {
                fieldType = default;
            }
            return false;
        }

        /// <summary>
        ///     Creates a new FieldType
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static FieldType Create(Type type)
        {
            return new FieldType(type);
        }

        /// <summary>
        ///     Creates a new FieldType
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static FieldType Create<T>()
        {
            return new FieldType(typeof(T));
        }

        /// <summary>
        ///     DataType cast to underlying type
        /// </summary>
        /// <param name="fieldType"></param>
        public static explicit operator Type(FieldType fieldType)
        {
            return fieldType.Type;
        }

        /// <inheritdoc />
        public override string ToString() => this.Type.FullName;
    }
}