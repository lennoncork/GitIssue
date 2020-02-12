using GitIssue.Values;

namespace GitIssue.Converters
{
    /// <summary>
    ///     Converts from a string to a version type
    /// </summary>
    public class StringTypeConverter : ValueTypeConverter<String>
    {
        /// <inheritdoc />
        public override bool TryParse(string input, out String result)
        {
            return String.TryParse(input, out result);
        }

        /// <inheritdoc />
        public override bool TryParse(ValueMetadata input, out String result)
        {
            return TryParse(input.Value, out result);
        }
    }
}