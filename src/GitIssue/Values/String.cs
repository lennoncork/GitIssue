using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using GitIssue.Issues.Json;
using Newtonsoft.Json.Linq;

namespace GitIssue.Values
{
    /// <summary>
    ///     String value type
    /// </summary>
    [TypeConverter(typeof(StringTypeConverter))]
    [TypeAlias(nameof(String))]
    public struct String : IJsonValue, IEquatable<String>, IValue<string>
    {
        /// <summary>
        ///     Parses the string
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static String Parse(string value)
        {
            return new String(value);
        }

        /// <summary>
        ///     Tries to parse the string value
        /// </summary>
        /// <param name="value"></param>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool TryParse(string value, out String str)
        {
            str = new String(value);
            return true;
        }

        internal String(string value)
        {
            this.Item = value;
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

        /// <summary>
        ///     Implicit cast to string
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator string(String value)
        {
            return value.Item;
        }

        /// <summary>
        ///     Implicit cast to string
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator String(string value)
        {
            return new String(value);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is String str)
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
        public bool Equals([AllowNull] String other)
        {
            return this.Item == other.Item;
        }

        /// <inheritdoc />
        public bool Equals([AllowNull] string other)
        {
            return this.Item == other;
        }

        /// <inheritdoc />
        public string Item { get; }
    }
}