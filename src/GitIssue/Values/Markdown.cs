using System.ComponentModel;
using GitIssue.Issues.Json;
using Newtonsoft.Json.Linq;

namespace GitIssue.Values
{
    /// <summary>
    ///     Version value type
    /// </summary>
    [TypeConverter(typeof(MarkdownTypeConverter))]
    [TypeAlias(nameof(Markdown))]
    public struct Markdown : IJsonValue
    {
        private readonly string value;

        /// <summary>
        ///     Tries to parse the markdown value
        /// </summary>
        /// <param name="value"></param>
        /// <param name="markdown"></param>
        /// <returns></returns>
        public static bool TryParse(string value, out Markdown markdown)
        {
            markdown = new Markdown(value);
            return true;
        }

        internal Markdown(string value)
        {
            this.value = value;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return value;
        }

        /// <inheritdoc />
        public JToken ToJson()
        {
            return new JValue(value);
        }
    }
}