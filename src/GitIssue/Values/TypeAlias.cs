using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GitIssue.Fields;

namespace GitIssue.Values
{
    /// <summary>
    ///     Abstract base class for any type aliases
    /// </summary>
    public class TypeAlias : ITypeAlias
    {
        /// <summary>
        /// The collection of aliases found
        /// </summary>
        public static ITypeAlias[] Aliases;

        static TypeAlias()
        {
            var aliases = new List<ITypeAlias>();

            aliases.AddRange(typeof(TypeValue).Assembly.GetTypes()
                .Where(IsAlias)
                .Where(IsParameterless)
                .Select(t => (ITypeAlias)Activator.CreateInstance(t)!)
                .Where(IsNotNull));

            aliases.AddRange(typeof(TypeValue).Assembly.GetTypes()
                .Where(IsFieldType)
                .Select(FromType)
                .Where(IsNotNull)
                .Select(t => t!));

            aliases.AddRange(typeof(TypeValue).Assembly.GetTypes()
                .Where(IsValueType)
                .Select(FromType)
                .Where(IsNotNull)
                .Select(t => t!));

            Aliases = aliases.ToArray();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TypeAlias" /> class
        /// </summary>
        /// <param name="alias"></param>
        protected TypeAlias(string alias)
        {
            Alias = alias;
            Type = GetType();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TypeAlias" /> class
        /// </summary>
        /// <param name="type"></param>
        /// <param name="alias"></param>
        protected TypeAlias(Type type, string alias)
        {
            Alias = alias;
            Type = type;
        }

        /// <summary>
        ///     Gets the alias
        /// </summary>
        public string Alias { get; protected set; }

        /// <summary>
        ///     Gets the type
        /// </summary>
        public Type Type { get; protected set; }

        /// <inheritdoc />
        public bool TryParse(string alias, out Type type)
        {
            if (alias == Alias)
            {
                type = Type;
                return true;
            }

            type = null!;
            return false;
        }

        /// <inheritdoc />
        public bool TryParse(Type type, out string str)
        {
            if (type == Type)
            {
                str = Alias;
                return true;
            }

            str = null!;
            return false;
        }

        /// <summary>
        ///     Gets the alias from a type using it's attribute
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static TypeAlias? FromType(Type type)
        {
            if (TryGetAliasAttribute(type, out var attribute)) 
                return new TypeAlias(type, attribute.Alias);
            return null;
        }


        /// <summary>
        ///     Gets a value indicating if the type is a value type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsValueType(Type type)
        {
            return typeof(IValue).IsAssignableFrom(type);
        }

        /// <summary>
        ///     Gets a value indicating if the type is a field type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsFieldType(Type type)
        {
            return typeof(IField).IsAssignableFrom(type);
        }

        /// <summary>
        ///     Checks if the object is null
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsNotNull(object? obj)
        {
            return obj != null;
        }

        /// <summary>
        ///     Gets a value determining if the type is an alias
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsAlias(Type type)
        {
            return typeof(ITypeAlias).IsAssignableFrom(type) && type.IsAbstract == false;
        }

        /// <summary>
        ///     Gets a value determining if the type has a parameterless constructor
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsParameterless(Type type)
        {
            return type.GetConstructor(Type.EmptyTypes) != null;
        }

        /// <summary>
        ///     Gets the alias attribute if it exists
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static TypeAliasAttribute? GetAliasAttribute(Type type) => type.GetCustomAttribute<TypeAliasAttribute>();

        /// <summary>
        ///     Tries to get an alias attribute from the type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public static bool TryGetAliasAttribute(Type type, out TypeAliasAttribute attribute)
        {
            attribute = GetAliasAttribute(type)!;
            return attribute != null;
        }

        /// <summary>
        ///     Tries to get the alias value
        /// </summary>
        /// <param name="type"></param>
        /// <param name="alias"></param>
        /// <returns></returns>
        public static bool TryGetAlias(Type type, out ITypeAlias alias)
        {
            if (IsAlias(type) && IsParameterless(type))
                try
                {
                    alias = (ITypeAlias) Activator.CreateInstance(type)!;
                    return true;
                }
                catch (Exception)
                {
                    // Ignore
                }

            alias = default!;
            return false;
        }
    }
}