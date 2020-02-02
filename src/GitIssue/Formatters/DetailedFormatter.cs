using System;
using System.Collections.Generic;
using System.Text;

namespace GitIssue.Formatters
{
    /// <summary>
    /// A simple formatter for showing the issue on a single line
    /// </summary>
    public class DetailedFormatter : IIssueFormatter
    {
        /// <inheritdoc />
        public string Format(IReadOnlyIssue issue)
        {
            StringBuilder builder = new StringBuilder();
            IFieldFormatter formatter = new SimpleFormatter();
            int fieldCount = 0;
            foreach (var kvp in issue)
            {
                if (fieldCount++ > 0) builder.Append(Environment.NewLine);
                builder.Append(kvp.Value.Format(formatter));
            }
            return builder.ToString();
        }
    }
}
