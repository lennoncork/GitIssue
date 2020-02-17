namespace GitIssue.Values
{
    /// <summary>
    ///     Converts from a string to a markdown value
    /// </summary>
    public class MarkdownTypeConverter : ValueTypeConverter<Markdown>
    {
        /// <inheritdoc />
        public override bool TryParse(string input, out Markdown result)
        {
            return Markdown.TryParse(input, out result);
        }

        /// <inheritdoc />
        public override bool TryParse(ValueMetadata input, out Markdown result)
        {
            return TryParse(input.Value, out result);
        }
    }
}