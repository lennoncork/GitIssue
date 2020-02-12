using System;

namespace GitIssue.Values
{
    /// <summary>
    ///     Attribute for simplifying application of aliases
    /// </summary>
    public class TypeAliasAttribute : Attribute
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TypeAliasAttribute" />
        /// </summary>
        /// <param name="alias"></param>
        public TypeAliasAttribute(string alias)
        {
            Alias = alias;
        }

        /// <summary>
        ///     Gets the alias
        /// </summary>
        public string Alias { get; }
    }
}