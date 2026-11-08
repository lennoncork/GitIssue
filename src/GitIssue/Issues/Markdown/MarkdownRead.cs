using System.Collections.Generic;
using GitIssue.Fields;
using GitIssue.Validation;

namespace GitIssue.Issues.Markdown
{
    /// <summary>
    ///     The result of reading a Markdown issue document
    /// </summary>
    public sealed class MarkdownRead
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MarkdownRead" /> class
        /// </summary>
        /// <param name="errors">the validation errors</param>
        /// <param name="values">the values that parsed successfully</param>
        public MarkdownRead(IReadOnlyList<IssueValidationError> errors, IReadOnlyDictionary<FieldKey, object?> values)
        {
            this.Errors = errors;
            this.Values = values;
        }

        /// <summary>
        ///     Gets the validation errors
        /// </summary>
        public IReadOnlyList<IssueValidationError> Errors { get; }

        /// <summary>
        ///     Gets the parsed field values
        /// </summary>
        public IReadOnlyDictionary<FieldKey, object?> Values { get; }
    }
}