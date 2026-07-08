using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
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
            this.Type = type;
        }

        /// <summary>
        ///     Tries to create the type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool TryCreate<T>(out T result, params object[] args)
            where T : class
        {
            if (!typeof(T).IsAssignableFrom(this.Type))
            {
                result = null!;
                return false;
            }

            try
            {
                ConstructorInfo? info = this.Type.GetConstructor(
                    BindingFlags.Instance | BindingFlags.Public,
                    null,
                    args.Select(o => o.GetType()).ToArray(),
                    null);

                T? create = info?.Invoke(args) as T;
                result = create!;

                if (create == null)
                {
                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                result = null!;
                return false;
            }
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
                Type? type = TypeValue.GetType(value);
                fieldType = type != null ? TypeValue.Create(type) : default(TypeValue)!;
                return true;
            }
            catch (Exception)
            {
                fieldType = default(TypeValue);
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
            foreach (ITypeAlias fieldType in TypeAlias.Aliases)
            {
                if (fieldType.TryParse(value, out Type type))
                {
                    return type;
                }
            }

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
            if (TypeAlias.TryGetAliasAttribute(this.Type, out TypeAliasAttribute attribute))
            {
                return attribute.Alias;
            }

            if (TypeAlias.TryGetAlias(this.Type, out ITypeAlias alias))
            {
                if (alias.TryParse(this.Type, out string result))
                {
                    return result;
                }
            }

            return this.Type.FullName;
        }
    }
}