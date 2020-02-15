using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using GitIssue.Fields;
using GitIssue.Issues;

namespace GitIssue.Formatters
{
    /// <summary>
    ///     A simple formatter for showing the issue on a single line
    /// </summary>
    public class SimpleFormatter : IIssueFormatter, IFieldFormatter
    {
        private static readonly Dictionary<string, Func<IReadOnlyIssue, string>> formatters =
            new Dictionary<string, Func<IReadOnlyIssue, string>>
            {
                {"Key", issue => issue.Key.ToString()}
            };

        private readonly string format;
        private readonly string formatRegex = @"(\%[\w]*)";

        /// <summary>
        ///     Initializes a new instance of the <see cref="SimpleFormatter" /> class
        /// </summary>
        public SimpleFormatter() : this("%Key: %Title")
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SimpleFormatter" /> class
        /// </summary>
        /// <param name="format">the output format</param>
        public SimpleFormatter(string format)
        {
            this.format = format;
        }

        /// <summary>
        ///     Gets the default formatter
        /// </summary>
        public static SimpleFormatter Default => new SimpleFormatter();

        /// <inheritdoc />
        public string Format(IField field)
        {
            return $"{field.Key.ToString()}: {field}";
        }

        /// <inheritdoc />
        public string Format(IReadOnlyIssue issue)
        {
            var result = format;
            var regex = new Regex(formatRegex, RegexOptions.Compiled);
            foreach (Match? match in regex.Matches(format))
            {
                if (match == null) continue;

                var property = match.Value.TrimStart('%');

                if (formatters.TryGetValue(property, out var formatter))
                {
                    result = result.Replace(match.Value, formatter.Invoke(issue));
                    continue;
                }

                if (issue.TryGetValue(FieldKey.Create(property), out var field))
                    result = result.Replace(match.Value, field.ToString());
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