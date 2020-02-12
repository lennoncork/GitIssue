using GitIssue.Values;

namespace GitIssue.Converters
{
    /// <summary>
    ///     Converts from a string to a label
    /// </summary>
    public class LabelTypeConverter : ValueTypeConverter<Label>
    {
        /// <inheritdoc />
        public override bool TryParse(string input, out Label result)
        {
            return Label.TryParse(input, out result);
        }

        /// <inheritdoc />
        public override bool TryParse(ValueMetadata input, out Label result)
        {
            return TryParse(input.Value, out result);
        }
    }
}