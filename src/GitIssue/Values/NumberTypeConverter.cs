namespace GitIssue.Values
{
    /// <summary>
    ///     Converts from a string to a version type
    /// </summary>
    public class NumberTypeConverter : ValueTypeConverter<Number>
    {
        /// <inheritdoc />
        public override bool TryParse(string input, out Number result)
        {
            return Number.TryParse(input, out result);
        }

        /// <inheritdoc />
        public override bool TryParse(ValueMetadata input, out Number result)
        {
            return TryParse(input.Value, out result);
        }
    }
}