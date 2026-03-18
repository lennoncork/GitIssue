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
        ///     The collection of aliases found
        /// </summary>
        public static ITypeAlias[] Aliases;

        static TypeAlias()
        {
            List<ITypeAlias> aliases = new List<ITypeAlias>();

            aliases.AddRange(typeof(TypeValue).Assembly.GetTypes()
                .Where(TypeAlias.IsAlias)
                .Where(TypeAlias.IsParameterless)
                .Select(t => (ITypeAlias)Activator.CreateInstance(t)!)
                .Where(TypeAlias.IsNotNull));

            aliases.AddRange(typeof(TypeValue).Assembly.GetTypes()
                .Where(TypeAlias.IsFieldType)
                .Select(TypeAlias.FromType)
                .Where(TypeAlias.IsNotNull)
                .Select(t => t!));

            aliases.AddRange(typeof(TypeValue).Assembly.GetTypes()
                .Where(TypeAlias.IsValueType)
                .Select(TypeAlias.FromType)
                .Where(TypeAlias.IsNotNull)
                .Select(t => t!));

            TypeAlias.Aliases = aliases.ToArray();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TypeAlias" /> class
        /// </summary>
        /// <param name="alias"></param>
        protected TypeAlias(string alias)
        {
            this.Alias = alias;
            this.Type = this.GetType();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TypeAlias" /> class
        /// </summary>
        /// <param name="type"></param>
        /// <param name="alias"></param>
        protected TypeAlias(Type type, string alias)
        {
            this.Alias = alias;
            this.Type = type;
        }

        /// <summary>
        ///     Gets the alias
        /// </summary>
        public string Alias { get; protected set; }

        /// <summary>
        ///     Gets the type
        /// </summary>
        public Type Type { get; protected set; }

        /// <summary>
        ///     Gets the alias from a type using it's attribute
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static TypeAlias? FromType(Type type)
        {
            if (TypeAlias.TryGetAliasAttribute(type, out TypeAliasAttribute attribute))
            {
                return new TypeAlias(type, attribute.Alias);
            }

            return null;
        }

        /// <summary>
        ///     Gets the alias attribute if it exists
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static TypeAliasAttribute? GetAliasAttribute(Type type)
        {
            return type.GetCustomAttribute<TypeAliasAttribute>();
        }

        /// <summary>
        ///     Gets a value determining if the type is an alias
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsAlias(Type type)
        {
            return typeof(ITypeAlias).IsAssignableFrom(type) && !type.IsAbstract;
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
        ///     Gets a value determining if the type has a parameterless constructor
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsParameterless(Type type)
        {
            return type.GetConstructor(Type.EmptyTypes) != null;
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
        ///     Tries to get the alias value
        /// </summary>
        /// <param name="type"></param>
        /// <param name="alias"></param>
        /// <returns></returns>
        public static bool TryGetAlias(Type type, out ITypeAlias alias)
        {
            if (TypeAlias.IsAlias(type) && TypeAlias.IsParameterless(type))
            {
                try
                {
                    alias = (ITypeAlias)Activator.CreateInstance(type)!;
                    return true;
                }
                catch (Exception)
                {
                    // Ignore
                }
            }

            alias = default(ITypeAlias)!;
            return false;
        }

        /// <summary>
        ///     Tries to get an alias attribute from the type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public static bool TryGetAliasAttribute(Type type, out TypeAliasAttribute attribute)
        {
            attribute = TypeAlias.GetAliasAttribute(type)!;
            return attribute != null;
        }

        /// <inheritdoc />
        public bool TryParse(string alias, out Type type)
        {
            if (alias == this.Alias)
            {
                type = this.Type;
                return true;
            }

            type = null!;
            return false;
        }

        /// <inheritdoc />
        public bool TryParse(Type type, out string str)
        {
            if (type == this.Type)
            {
                str = this.Alias;
                return true;
            }

            str = null!;
            return false;
        }
    }
}