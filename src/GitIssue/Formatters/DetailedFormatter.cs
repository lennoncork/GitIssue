using System;
using System.Collections.Generic;
using System.Text;
using GitIssue.Fields;

namespace GitIssue.Formatters
{
    /// <summary>
    /// A detailed formatter showing each field on it's own line
    /// </summary>
    public class DetailedFormatter : IIssueFormatter, IFieldFormatter
    {
        /// <inheritdoc />
        public string Format(IReadOnlyIssue issue)
        {
            StringBuilder builder = new StringBuilder();
            int fieldCount = 0;
            foreach (var kvp in issue)
            {
                if (fieldCount++ > 0) builder.Append(Environment.NewLine);
                builder.Append(kvp.Value.Format(this));
            }
            return builder.ToString();
        }

        /// <inheritdoc />
        public string Format(IField field) => $"{field.Key.ToString()}: {field}";
    }
}
