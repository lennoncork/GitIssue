using System;

namespace GitIssue.Fields
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
    }
}