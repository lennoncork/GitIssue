using System.Collections.Generic;
using System.Text;
using GitIssue.Fields;
using GitIssue.Values;

namespace GitIssue.Converters
{
    /// <summary>
    /// Converts from a string to a version type
    /// </summary>
    public class UrlTypeConverter: ValueTypeConverter<Url>
    {
        /// <inheritdoc/>
        public override bool TryParse(string input, out Url result) => Url.TryParse(input, out result);
    }
}
