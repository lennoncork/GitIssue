using GitIssue.Fields;

namespace GitIssue.Formatters
{
    /// <summary>
    ///     Interface defining a field formatter
    /// </summary>
    public interface IFieldFormatter
    {
        /// <summary>
        ///     Formats the field
        /// </summary>
        /// <param name="field">the issue field</param>
        /// <returns>a formatted string for the field</returns>
        string? Format(IField field);
    }
}