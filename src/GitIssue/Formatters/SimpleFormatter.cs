using System;
using System.Collections.Generic;
using System.Text;

namespace GitIssue.Formatters
{
    /// <summary>
    /// A simple formatter for showing the issue on a single line
    /// </summary>
    public class SimpleFormatter : IIssueFormatter
    {
        /// <inheritdoc />
        public string Format(IReadOnlyIssue issue) => $"{issue.Key.ToString()}: {issue.Title}";
    }
}
