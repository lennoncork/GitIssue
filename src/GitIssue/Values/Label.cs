using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using GitIssue.Issues.Json;
using Newtonsoft.Json.Linq;

namespace GitIssue.Values
{
    /// <summary>
    ///     A simple label class
    /// </summary>
    [TypeConverter(typeof(LabelTypeConverter))]
    [TypeAlias(nameof(Label))]
    public struct Label : IJsonValue, IValue<string>, IEquatable<Label>
    {
        private readonly string value;

        /// <summary>
        ///     Tries to parse the label value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Label Parse(string value)
        {
            return new Label(value);
        }

        /// <summary>
        ///     Tries to parse the label value
        /// </summary>
        /// <param name="value"></param>
        /// <param name="label"></param>
        /// <returns></returns>
        public static bool TryParse(string value, out Label label)
        {
            label = new Label(value);
            return true;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Label" /> struct
        /// </summary>
        /// <param name="value"></param>
        public Label(string value)
        {
            this.value = !string.IsNullOrWhiteSpace(value) ? value.Trim().Split()[0].ToLowerInvariant() : string.Empty;
        }

        /// <inheritdoc />
        public override string? ToString()
        {
            return value;
        }

        /// <inheritdoc />
        public JToken ToJson()
        {
            return new JValue(value);
        }

        /// <inheritdoc />
        public bool Equals([AllowNull] Label other)
        {
            return value == other.value;
        }

        /// <inheritdoc />
        public bool Equals([AllowNull] string other)
        {
            return value == other;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is Label label)
                return this.Equals(label);
            return base.Equals(obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        /// <inheritdoc />
        public string Item => this.value;
    }
}