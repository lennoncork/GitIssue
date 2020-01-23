using System;
using System.ComponentModel;
using GitIssue.Converters;
using Newtonsoft.Json;

namespace GitIssue.Fields
{
    /// <summary>
    /// Contains type information for fields
    /// </summary>
    [TypeConverter(typeof(FieldTypeTypeConverter))]
    public struct FieldType
    {
        /// <summary>
        /// The field type used for <see cref="IField"/>
        /// </summary>
        [JsonIgnore]
        public Type Type { get; }

        private FieldType(Type type)
        {
            this.Type = type;
        }

        /// <summary>
        /// Creates a new FieldType
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static FieldType Create(Type type) => new FieldType(type);

        /// <summary>
        /// Creates a new FieldType
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static FieldType Create<T>() => new FieldType(typeof(T));

        /// <summary>
        /// DataType cast to underlying type
        /// </summary>
        /// <param name="fieldType"></param>
        public static explicit operator Type(FieldType fieldType)
        {
            return fieldType.Type;
        }
    }
}
