using System.Collections.Generic;
using System.Text;
using GitIssue.Fields;
using GitIssue.Values;
using Version = GitIssue.Values.Version;

namespace GitIssue.Converters
{
    /// <summary>
    /// Converts from a string to a version type
    /// </summary>
    public class VersionTypeConverter: ValueTypeConverter<Version>
    {
        /// <inheritdoc/>
        public override bool TryParse(string input, out Version result) => Version.TryParse(input, out result);

        /// <inheritdoc/>
        public override bool TryParse(ValueMetadata input, out Version result) => TryParse(input.Value, out result);
    }
}
