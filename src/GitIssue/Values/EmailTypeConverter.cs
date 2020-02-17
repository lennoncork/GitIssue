namespace GitIssue.Values
{
    /// <summary>
    ///     Converts from a string to a version type
    /// </summary>
    public class EmailTypeConverter : ValueTypeConverter<Email>
    {
        /// <inheritdoc />
        public override bool TryParse(string input, out Email result)
        {
            return Email.TryParse(input, out result);
        }

        /// <inheritdoc />
        public override bool TryParse(ValueMetadata input, out Email result)
        {
            return TryParse(input.Value, out result);
        }
    }
}