using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;
using GitIssue.Converters;
using GitIssue.Json;
using Newtonsoft.Json.Linq;

namespace GitIssue.Values
{
    /// <summary>
    /// Version value type
    /// </summary>
    [TypeConverter(typeof(MarkdownTypeConverter))]
    public class Markdown : IJsonValue
    {
        private readonly string content;

        /// <summary>
        /// Tries to parse the markdown string
        /// </summary>
        /// <param name="str"></param>
        /// <param name="markdown"></param>
        /// <returns></returns>
        public static bool TryParse(string str, out Markdown markdown)
        {
            markdown = new Markdown(str);
            return true;
        }

        internal Markdown(string content)
        {
            this.content = content;
        }

        /// <inheritdoc/>
        public override string ToString() => this.content;

        /// <inheritdoc />
        public JToken ToJson() => new JValue(this.ToString());
    }
}
