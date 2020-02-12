using GitIssue.Values;

namespace GitIssue.Converters
{
    /// <summary>
    ///     Converts from a string to a version type
    /// </summary>
    public class EnumTypeConverter : ValueTypeConverter<Enumerated>
    {
        /// <inheritdoc />
        public override bool TryParse(string input, out Enumerated result)
        {
            return TryParse(new ValueMetadata(input, string.Empty), out result);
        }

        /// <inheritdoc />
        public override bool TryParse(ValueMetadata input, out Enumerated result)
        {
            return Enumerated.TryParse(input, out result);
        }
    }
}