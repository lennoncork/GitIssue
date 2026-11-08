using GitIssue.Fields;
using GitIssue.Issues;

namespace GitIssue.Validation
{
    /// <summary>
    ///     A single field or document validation failure
    /// </summary>
    public sealed class IssueValidationError
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="IssueValidationError" /> class
        /// </summary>
        /// <param name="issue">the issue key</param>
        /// <param name="field">the field key, or <see cref="FieldKey.None" /> for a document error</param>
        /// <param name="line">the 1-based source line, when the format provides one</param>
        /// <param name="message">the error message</param>
        public IssueValidationError(IssueKey issue, FieldKey field, int? line, string message)
        {
            this.Issue = issue;
            this.Field = field;
            this.Line = line;
            this.Message = message;
        }

        /// <summary>
        ///     Gets the issue key
        /// </summary>
        public IssueKey Issue { get; }

        /// <summary>
        ///     Gets the field key
        /// </summary>
        public FieldKey Field { get; }

        /// <summary>
        ///     Gets the 1-based source line
        /// </summary>
        public int? Line { get; }

        /// <summary>
        ///     Gets the error message
        /// </summary>
        public string Message { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            string field = string.IsNullOrEmpty(this.Field.ToString()) ? string.Empty : this.Field.ToString();
            string line = this.Line.HasValue ? $" (line {this.Line.Value})" : string.Empty;
            if (string.IsNullOrEmpty(field))
            {
                return $"{line.Trim()} {this.Message}".Trim();
            }

            return $"{field}{line}: {this.Message}";
        }
    }
}
