using System;
using System.Text;
using GitIssue.Fields;
using GitIssue.Keys;

namespace GitIssue.Formatters
{
    /// <summary>
    ///     A detailed formatter showing each field on it's own line
    /// </summary>
    public class DetailedFormatter : IIssueFormatter, IFieldFormatter
    {
        /// <summary>
        /// Gets the newline string
        /// </summary>
        public virtual string NewLine => Environment.NewLine;

        /// <summary>
        /// Gets or sets the formatter for the issue key
        /// </summary>
        public Func<IssueKey, string> IssueKeyFormatter { get; set; } = k => $"Key: {k}";

        /// <summary>
        /// Gets or sets the formatter for the field key
        /// </summary>
        public Func<FieldKey, string> FieldKeyFormatter { get; set; } = k => $"{k}";

        /// <inheritdoc />
        public virtual string Format(IField field)
        {
            return $"{this.FieldKeyFormatter(field.Key)}: {field}";
        }

        /// <inheritdoc />
        public virtual string Format(IReadOnlyIssue issue)
        {
            var builder = new StringBuilder();
            builder.Append(this.IssueKeyFormatter(issue.Key));
            builder.Append(this.NewLine);
            var fieldCount = 0;
            foreach (var kvp in issue)
            {
                if (fieldCount++ > 0) builder.Append(this.NewLine);
                builder.Append(kvp.Value.Format(this));
            }

            return builder.ToString();
        }
    }
}