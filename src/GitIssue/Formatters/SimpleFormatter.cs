using System;
using System.Collections.Generic;
using System.Text;
using GitIssue.Fields;

namespace GitIssue.Formatters
{
    /// <summary>
    /// A simple formatter for showing the issue on a single line
    /// </summary>
    public class SimpleFormatter : IIssueFormatter, IFieldFormatter
    {
        /// <inheritdoc />
        public string Format(IReadOnlyIssue issue) => $"{issue.Key.ToString()}: {issue.Title}";
        
        /// <inheritdoc />
        public string Format(IField field) => $"{field.Key.ToString()}: {field}";
    }
}
