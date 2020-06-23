using System;
using System.Threading.Tasks;

namespace GitIssue.Fields
{
    /// <summary>
    ///     Field Interface
    /// </summary>
    public interface IReadOnlyField : IEquatable<IField>
    {
        /// <summary>
        ///     Gets the field's key
        /// </summary>
        FieldKey Key { get; }

        /// <summary>
        ///     Exports to a string
        /// </summary>
        /// <returns></returns>
        Task<string> ExportAsync();
    }
}