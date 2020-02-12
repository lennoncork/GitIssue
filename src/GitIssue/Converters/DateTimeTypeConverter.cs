using GitIssue.Values;

namespace GitIssue.Converters
{
    /// <summary>
    ///     Converts from a string to a version type
    /// </summary>
    public class DateTimeTypeConverter : ValueTypeConverter<DateTime>
    {
        /// <inheritdoc />
        public override bool TryParse(string input, out DateTime result)
        {
            return DateTime.TryParse(input, out result);
        }

        /// <inheritdoc />
        public override bool TryParse(ValueMetadata input, out DateTime result)
        {
            return TryParse(input.Value, out result);
        }
    }
}