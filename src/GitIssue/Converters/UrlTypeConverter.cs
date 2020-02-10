using GitIssue.Values;

namespace GitIssue.Converters
{
    /// <summary>
    ///     Converts from a string to a version type
    /// </summary>
    public class UrlTypeConverter : ValueTypeConverter<Url>
    {
        /// <inheritdoc />
        public override bool TryParse(string input, out Url result)
        {
            return Url.TryParse(input, out result);
        }

        /// <inheritdoc />
        public override bool TryParse(ValueMetadata input, out Url result)
        {
            return TryParse(input.Value, out result);
        }
    }
}