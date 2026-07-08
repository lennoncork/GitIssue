using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using GitIssue.Issues.Json;
using Newtonsoft.Json.Linq;

namespace GitIssue.Values
{
    /// <summary>
    ///     Version value type
    /// </summary>
    [TypeConverter(typeof(MarkdownTypeConverter))]
    [TypeAlias(nameof(Markdown))]
    public struct Markdown : IJsonValue, IEquatable<Markdown>, IValue<string>
    {
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
            this.Item = value;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is Markdown str)
            {
                return this.Equals(str);
            }

            return base.Equals(obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return this.Item.GetHashCode();
        }

        /// <inheritdoc />
        public bool Equals([AllowNull] Markdown other)
        {
            return this.Item == other.Item;
        }

        /// <inheritdoc />
        public bool Equals([AllowNull] string other)
        {
            return this.Item == other;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.Item;
        }

        /// <inheritdoc />
        public JToken ToJson()
        {
            return new JValue(this.Item);
        }

        /// <inheritdoc />
        public string Item { get; }
    }
}