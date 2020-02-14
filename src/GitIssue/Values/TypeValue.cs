using System;
using System.ComponentModel;
using GitIssue.Converters;
using GitIssue.Fields;
using Newtonsoft.Json;

namespace GitIssue.Values
{
    /// <summary>
    ///     Contains type information for fields
    /// </summary>
    [TypeConverter(typeof(TypeValueTypeConverter))]
    public struct TypeValue
    {
        /// <summary>
        ///     The field type used for <see cref="IField" />
        /// </summary>
        [JsonIgnore]
        public Type Type { get; }

        private TypeValue(Type type)
        {
            Type = type;
        }

        /// <summary>
        ///     Tries to parse the label value
        /// </summary>
        /// <param name="value"></param>
        /// <param name="fieldType"></param>
        /// <returns></returns>
        public static bool TryParse(string value, out TypeValue fieldType)
        {
            try
            {
                var type = GetType(value);
                fieldType = type != null ? Create(type) : default!;
                return true;
            }
            catch (Exception)
            {
                fieldType = default;
            }

            return false;
        }

        /// <summary>
        ///     Gets the type using an alias or the type name
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Type? GetType(string value)
        {
            foreach (var fieldType in TypeAlias.Aliases)
                if (fieldType.TryParse(value, out var type))
                    return type;
            return Type.GetType(value);
        }

        /// <summary>
        ///     Creates a new FieldType
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static TypeValue Create(Type type)
        {
            return new TypeValue(type);
        }

        /// <summary>
        ///     Creates a new FieldType
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static TypeValue Create<T>()
        {
            return new TypeValue(typeof(T));
        }

        /// <summary>
        ///     DataType cast to underlying type
        /// </summary>
        /// <param name="fieldType"></param>
        public static explicit operator Type(TypeValue fieldType)
        {
            return fieldType.Type;
        }

        /// <inheritdoc />
        public override string? ToString()
        {
            if (TypeAlias.TryGetAliasAttribute(Type, out var attribute))
                return attribute.Alias;

            if (TypeAlias.TryGetAlias(Type, out var alias))
                if (alias.TryParse(Type, out var result))
                    return result;

            return Type.FullName;
        }
    }
}