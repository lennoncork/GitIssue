using GitIssue.Formatters;
using GitIssue.Issues;
using Pastel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using GitIssue.Fields;

namespace GitIssue.Tool
{
    public class TerminalFormatter : IssueFormatter
    {
        private static Dictionary<string, Func<IIssueFormatter, IReadOnlyIssue, string>> terminalIssueFormatters = new Dictionary<string, Func<IIssueFormatter, IReadOnlyIssue, string>>
        {
            {"*",       (t, i) => i
                            .Select(f => $"{f.Key.ToString().PadRight(20,' ').Pastel(Color.FromArgb(102, 255, 102))} {t.Format(f.Value)}")
                            .Aggregate((a, b) => $"{a}{Environment.NewLine}{b}") }
        };

        private static Dictionary<string, Func<IIssueFormatter, IReadOnlyField, string>> terminalFieldFormatters = new Dictionary<string, Func<IIssueFormatter, IReadOnlyField, string>>
        {
            {"Key",     (t, f) => $"{f.ToString()?.PadRight(20,' ').Pastel(Color.FromArgb(165, 229, 250))}"},
        };

        public TerminalFormatter(string format) : base(format, terminalIssueFormatters, terminalFieldFormatters)
        {

        }

        public new static TerminalFormatter Simple => new TerminalFormatter("%Key %Title");

        public new static TerminalFormatter Detailed => new TerminalFormatter("%*");
    }
}
