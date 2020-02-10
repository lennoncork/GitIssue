using GitIssue.Values;

namespace GitIssue.Converters
{
    /// <summary>
    ///     Converts from a string to a version type
    /// </summary>
    public class VersionTypeConverter : ValueTypeConverter<Version>
    {
        /// <inheritdoc />
        public override bool TryParse(string input, out Version result)
        {
            return Version.TryParse(input, out result);
        }

        /// <inheritdoc />
        public override bool TryParse(ValueMetadata input, out Version result)
        {
            return TryParse(input.Value, out result);
        }
    }
}