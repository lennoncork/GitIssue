using System;

namespace GitIssue.Values
{
    /// <summary>
    ///     Interface for value types
    /// </summary>
    public interface IValue
    {
        
    }

    /// <summary>
    ///     Interface for value types with a backing variable
    /// </summary>
    public interface IValue<T> : IValue, IEquatable<T>
    {
        /// <summary>
        /// Gets the item backing the value
        /// </summary>
        T Item { get; }
    }
}