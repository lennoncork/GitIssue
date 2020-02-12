using System;
using GitIssue.Fields;
using GitIssue.Issues;

namespace GitIssue.Formatters
{
    /// <summary>
    ///     A simple formatter for showing the issue on a single line
    /// </summary>
    public class DelegateFormatter : IIssueFormatter, IFieldFormatter
    {
        private readonly Func<IField, string> fieldFormatter = f => f.ToString();
        private readonly Func<IReadOnlyIssue, string> issueFormatter = i => i.ToString();

        /// <summary>
        ///     Initializes a new instance of the <see cref="DelegateFormatter" /> class
        /// </summary>
        public DelegateFormatter()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DelegateFormatter" /> class
        /// </summary>
        public DelegateFormatter(Func<IReadOnlyIssue, string> issueFormatter)
        {
            this.issueFormatter = issueFormatter;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DelegateFormatter" /> class
        /// </summary>
        public DelegateFormatter(Func<IField, string> fieldFormatter)
        {
            this.fieldFormatter = fieldFormatter;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DelegateFormatter" /> class
        /// </summary>
        public DelegateFormatter(Func<IReadOnlyIssue, string> issueFormatter, Func<IField, string> fieldFormatter)
        {
            this.issueFormatter = issueFormatter;
            this.fieldFormatter = fieldFormatter;
        }

        /// <summary>
        ///     Gets the default formatter
        /// </summary>
        public static DelegateFormatter Default => new DelegateFormatter();

        /// <inheritdoc />
        public string Format(IField field)
        {
            return fieldFormatter?.Invoke(field);
        }

        /// <inheritdoc />
        public string Format(IReadOnlyIssue issue)
        {
            return issueFormatter?.Invoke(issue);
        }
    }
}