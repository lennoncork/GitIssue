namespace GitIssue.Issues
{
    /// <summary>
    ///     The result of regenerating an issue key from the current template
    /// </summary>
    public sealed class IssueRekeyResult
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="IssueRekeyResult" /> class
        /// </summary>
        /// <param name="from">the previous key</param>
        /// <param name="to">the regenerated key</param>
        public IssueRekeyResult(IssueKey from, IssueKey to)
        {
            this.From = from;
            this.To = to;
        }

        /// <summary>
        ///     Gets the previous issue key
        /// </summary>
        public IssueKey From { get; }

        /// <summary>
        ///     Gets the regenerated issue key
        /// </summary>
        public IssueKey To { get; }
    }
}
