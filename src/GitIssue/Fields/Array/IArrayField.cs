using System;
using System.Collections;
using System.Collections.Generic;

namespace GitIssue.Fields.Array
{
    /// <summary>
    ///     Interface for array fields
    /// </summary>
    public interface IArrayField : IField, IList
    {
        /// <summary>
        ///     Gets the value type
        /// </summary>
        Type ValueType { get; }

        /// <summary>
        /// Gets or sets the value
        /// </summary>
        object[]? Values { get; set; }

        /// <summary>
        ///     Tries to parse the input into an array value
        /// </summary>
        /// <param name="input">the input to parse</param>
        /// <param name="value">the output value</param>
        /// <returns>true is successful, false otherwise</returns>
        bool TryParse(string input, out object? value);
    }

    /// <summary>
    ///     Generic interface for array fields
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IArrayField<T> : IArrayField, IList<T>
    {
        /// <summary>
        ///     Gets or sets the fields array of value
        /// </summary>
        new T[] Values { get; set; }

        /// <summary>
        ///     Tries to parse the input into an array value
        /// </summary>
        /// <param name="input">the input to parse</param>
        /// <param name="value">the output value</param>
        /// <returns>true is successful, false otherwise</returns>
        bool TryParse(string input, out T value);
    }
}