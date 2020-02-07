using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using GitIssue.Fields;
using GitIssue.Values;

namespace GitIssue.Converters
{
    /// <summary>
    /// Converts from a string to a label
    /// </summary>
    public class LabelTypeConverter : ValueTypeConverter<Label>
    {
        /// <inheritdoc />
        public override bool TryParse(string input, out Label result) => Label.TryParse(input, out result);

        /// <inheritdoc/>
        public override bool TryParse(ValueMetadata input, out Label result) => TryParse(input.Value, out result);
    }
}
