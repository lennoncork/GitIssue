using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using GitIssue.Fields;
using GitIssue.Keys;

namespace GitIssue.Formatters
{
    /// <summary>
    /// A simple formatter for showing the issue on a single line
    /// </summary>
    public class DelegateFormatter : IIssueFormatter, IFieldFormatter
    {
        private readonly Func<IReadOnlyIssue, string> issueFormatter = i => i.ToString();

        private readonly Func<IField, string> fieldFormatter = f => f.ToString();

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateFormatter"/> class
        /// </summary>
        public DelegateFormatter()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateFormatter"/> class
        /// </summary>
        public DelegateFormatter(Func<IReadOnlyIssue, string> issueFormatter)
        {
            this.issueFormatter = issueFormatter;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateFormatter"/> class
        /// </summary>
        public DelegateFormatter(Func<IField, string> fieldFormatter)
        {
            this.fieldFormatter = fieldFormatter;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateFormatter"/> class
        /// </summary>
        public DelegateFormatter(Func<IReadOnlyIssue, string> issueFormatter, Func<IField, string> fieldFormatter)
        {
            this.issueFormatter = issueFormatter;
            this.fieldFormatter = fieldFormatter;
        }

        /// <summary>
        /// Gets the default formatter
        /// </summary>
        public static DelegateFormatter Default => new DelegateFormatter();

        /// <inheritdoc />
        public string Format(IReadOnlyIssue issue) => this.issueFormatter?.Invoke(issue);

        /// <inheritdoc />
        public string Format(IField field) => this.fieldFormatter?.Invoke(field);
    }
}
