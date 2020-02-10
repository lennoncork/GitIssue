namespace GitIssue.Formatters
{
    /// <summary>
    ///     Field defining an issue formatter
    /// </summary>
    public interface IIssueFormatter
    {
        /// <summary>
        ///     Formats the issue
        /// </summary>
        /// <param name="issue">the issue</param>
        /// <returns>a formatted string for the field</returns>
        string Format(IReadOnlyIssue issue);
    }
}