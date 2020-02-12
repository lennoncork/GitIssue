using System;
using System.Collections.Generic;

namespace GitIssue.Fields.Array
{
    /// <summary>
    ///     Interface for array fields
    /// </summary>
    public interface IArrayField : IField
    {
        /// <summary>
        ///     Gets the value type
        /// </summary>
        Type ValueType { get; }
    }

    /// <summary>
    ///     Generic interface for array fields
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IArrayField<T> : IArrayField, IEnumerable<T>
    {
        /// <summary>
        ///     Gets or sets the fields array of value
        /// </summary>
        T[] Values { get; set; }
    }
}