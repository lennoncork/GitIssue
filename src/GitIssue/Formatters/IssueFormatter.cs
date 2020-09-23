using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using GitIssue.Fields;
using GitIssue.Issues;

namespace GitIssue.Formatters
{
    /// <summary>
    ///     A simple formatter for showing the issue on a single line
    /// </summary>
    public class IssueFormatter : IIssueFormatter, IFieldFormatter
    {

        private readonly string matcher = @"([\%][\w\*]*)";

        private readonly string format;

        private readonly Dictionary<string, Func<IIssueFormatter, IReadOnlyIssue, string>> issueFormatters =
            new Dictionary<string, Func<IIssueFormatter, IReadOnlyIssue, string>>
            {
                {"Key", (t,i) => i.Key.ToString()},
                {"*",   (t,i) => i.Select(f => $"{f.Key,-16} {t.Format(f.Value)}").Aggregate((a, b) => $"{a}{Environment.NewLine}{b}") }
            };

        private readonly Dictionary<string, Func<IIssueFormatter, IReadOnlyField, string>> fieldFormatters =
            new Dictionary<string, Func<IIssueFormatter, IReadOnlyField, string>>
            {
                
            };

        /// <summary>
        ///     Initializes a new instance of the <see cref="IssueFormatter" /> class
        /// </summary>
        public IssueFormatter() : this("%Key: %Title")
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="IssueFormatter" /> class
        /// </summary>
        /// <param name="format">the output format</param>
        public IssueFormatter(string format)
        {
            this.format = format;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="IssueFormatter" /> class
        /// </summary>
        /// <param name="format">the output format</param>
        /// <param name="issueFormatters">the collection of issue formatters</param>
        /// <param name="fieldFormatters">the collection of issue formatters</param>
        public IssueFormatter(
            string format, 
            Dictionary<string, Func<IIssueFormatter, IReadOnlyIssue, string>> issueFormatters,
            Dictionary<string, Func<IIssueFormatter, IReadOnlyField, string>> fieldFormatters)
        {
            this.format = format;
            this.issueFormatters = issueFormatters;
            this.fieldFormatters = fieldFormatters;
        }

        /// <summary>
        ///     Gets the default formatter
        /// </summary>
        public static IssueFormatter Simple => new IssueFormatter();

        /// <summary>
        ///     Gets the default formatter
        /// </summary>
        public static IssueFormatter Detailed => new IssueFormatter("%*");

        /// <inheritdoc />
        public virtual string Format(IField field)
        {
            if (fieldFormatters.TryGetValue(field.Key, out var formatter))
            {
                return formatter.Invoke(this, field);
            }
            return field.ToString() ?? string.Empty;
        }

        /// <inheritdoc />
        public virtual string Format(IReadOnlyIssue issue)
        {
            var result = format;
            var regex = new Regex(matcher, RegexOptions.Compiled);
            foreach (Match? match in regex.Matches(format))
            {
                if (match == null) 
                    continue;

                var property = match.Value.TrimStart('%');
                if (issueFormatters.TryGetValue(property, out var formatter))
                {
                    result = result.Replace(match.Value, formatter.Invoke(this, issue));
                    continue;
                }

                if (issue.TryGetValue(FieldKey.Create(property), out var field))
                {
                    result = result.Replace(match.Value, this.Format(field));
                }
            }

            return result;
        }

        /// <summary>
        ///     Tries to match the input with the compiled regex
        /// </summary>
        /// <param name="regex"></param>
        /// <param name="input"></param>
        /// <param name="match"></param>
        /// <returns></returns>
        public static bool TryMatch(Regex regex, string input, out Match match)
        {
            match = regex.Match(input);
            return match.Success;
        }
    }
}