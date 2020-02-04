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
    public class SimpleFormatter : IIssueFormatter, IFieldFormatter
    {
        private readonly string formatRegex = @"(\%[\w]*)";

        private readonly string format;

        private static readonly Dictionary<string, Func<IReadOnlyIssue, string>> formatters =
            new Dictionary<string, Func<IReadOnlyIssue, string>>
            {
                {"Key", issue => issue.Key.ToString()}
            };

        public SimpleFormatter() : this("%Key: %Title")
        {

        }

        public SimpleFormatter(string format)
        {
            this.format = format;
        }

        /// <summary>
        /// Gets the default formatter
        /// </summary>
        public static SimpleFormatter Default => new SimpleFormatter();

        /// <inheritdoc />
        public string Format(IReadOnlyIssue issue)
        {
            string result = this.format;
            var regex = new Regex(formatRegex, RegexOptions.Compiled);
            foreach (Match match in regex.Matches(format))
            {
                string property = match.Value.TrimStart('%');
                if (formatters.TryGetValue(property, out Func<IReadOnlyIssue, string> formatter))
                {
                    result = result.Replace(match.Value, formatter.Invoke(issue));
                    continue;
                }
                if (issue.TryGetValue(FieldKey.Create(property), out IField field))
                {
                    result = result.Replace(match.Value, field.ToString());
                    continue;
                }
            }
            return result;
        }

        /// <inheritdoc />
        public string Format(IField field) => $"{field.Key.ToString()}: {field}";

        /// <summary>
        /// Tries to match the input with the compiled regex
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
