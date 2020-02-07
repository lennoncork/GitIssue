using System;
using System.Collections.Generic;
using System.Text;
using GitIssue.Fields;
using GitIssue.Values;

namespace GitIssue.Converters
{
    /// <summary>
    /// Converts from a string to a version type
    /// </summary>
    public class EnumTypeConverter: ValueTypeConverter<Enumerated>
    {
        /// <inheritdoc/>
        public override bool TryParse(string input, out Enumerated result) => TryParse(new ValueMetadata(input, String.Empty), out result);

        /// <inheritdoc/>
        public override bool TryParse(ValueMetadata input, out Enumerated result) => Enumerated.TryParse(input, out result);
    }
}
