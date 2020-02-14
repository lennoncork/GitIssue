using System;

namespace GitIssue.Values
{
    /// <summary>
    /// Interface for providing an alias (name) for a given type
    /// </summary>
    public interface ITypeAlias
    {
        /// <summary>
        /// Tries to parse the alias
        /// </summary>
        /// <param name="alias">the alias to parse</param>
        /// <param name="type">the type that matches the alias</param>
        /// <returns>true if parsed, false otherwise</returns>
        public bool TryParse(string alias, out Type type);

        /// <summary>
        /// Tries to parse the type
        /// </summary>
        /// <param name="type">the type to parse</param>
        /// <param name="alias">the alias of the type</param>
        /// <returns>true if parsed, false otherwise</returns>
        public bool TryParse(Type type, out string alias);
    }
}