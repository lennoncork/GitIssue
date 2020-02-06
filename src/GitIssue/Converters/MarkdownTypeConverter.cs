using System.Collections.Generic;
using System.Text;
using GitIssue.Fields;
using GitIssue.Values;

namespace GitIssue.Converters
{
    /// <summary>
    /// Converts from a string to a markdown value
    /// </summary>
    public class MarkdownTypeConverter: ValueTypeConverter<Markdown>
    {
        /// <inheritdoc/>
        public override bool TryParse(string input, out Markdown result) => Markdown.TryParse(input, out result);
    }
}
