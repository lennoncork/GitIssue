using System.Collections.Generic;
using System.Text;
using GitIssue.Fields;
using GitIssue.Values;

namespace GitIssue.Converters
{
    /// <summary>
    /// Converts from a string to a version type
    /// </summary>
    public class EmailTypeConverter: ValueTypeConverter<Email>
    {
        /// <inheritdoc/>
        public override bool TryParse(string input, out Email result) => Email.TryParse(input, out result);

        /// <inheritdoc/>
        public override bool TryParse(ValueMetadata input, out Email result) => TryParse(input.Value, out result);
    }
}
