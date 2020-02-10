using System;
using System.Text;
using GitIssue.Fields;

namespace GitIssue.Formatters
{
    /// <summary>
    ///     A detailed formatter showing each field on it's own line
    /// </summary>
    public class DetailedFormatter : IIssueFormatter, IFieldFormatter
    {
        /// <inheritdoc />
        public string Format(IField field)
        {
            return $"{field.Key.ToString()}: {field}";
        }

        /// <inheritdoc />
        public string Format(IReadOnlyIssue issue)
        {
            var builder = new StringBuilder();
            var fieldCount = 0;
            foreach (var kvp in issue)
            {
                if (fieldCount++ > 0) builder.Append(Environment.NewLine);
                builder.Append(kvp.Value.Format(this));
            }

            return builder.ToString();
        }
    }
}