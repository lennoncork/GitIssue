using System;

namespace GitIssue.Fields.Value
{
    /// <summary>
    ///     Interface for value fields
    /// </summary>
    public interface IValueField : IField
    {
        /// <summary>
        ///     Gets the value type
        /// </summary>
        Type ValueType { get; }

        /// <summary>
        ///     Tries to parse the input into an array value
        /// </summary>
        /// <param name="input">the input to parse</param>
        /// <param name="value">the output value</param>
        /// <returns>true is successful, false otherwise</returns>
        bool TryParse(string input, out object? value);
    }

    /// <summary>
    ///     Generic interface for value fields
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IValueField<T> : IValueField
    {
        /// <summary>
        ///     Gets or sets the field's value
        /// </summary>
        T Value { get; set; }

        /// <summary>
        ///     Tries to parse the input into an array value
        /// </summary>
        /// <param name="input">the input to parse</param>
        /// <param name="value">the output value</param>
        /// <returns>true is successful, false otherwise</returns>
        bool TryParse(string input, out T value);
    }
}