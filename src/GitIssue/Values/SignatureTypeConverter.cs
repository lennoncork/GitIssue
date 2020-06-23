namespace GitIssue.Values
{
    /// <summary>
    ///     Converts from a string to a version type
    /// </summary>
    public class SignatureTypeConverter : ValueTypeConverter<Signature>
    {
        /// <inheritdoc />
        public override bool TryParse(string input, out Signature result)
        {
            return Signature.TryParse(input, out result);
        }

        /// <inheritdoc />
        public override bool TryParse(ValueMetadata input, out Signature result)
        {
            return TryParse(input.Value, out result);
        }
    }
}