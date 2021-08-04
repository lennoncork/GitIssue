using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using GitIssue.Issues.Json;
using Newtonsoft.Json.Linq;

namespace GitIssue.Values
{
    /// <summary>
    ///     An enumerated value
    /// </summary>
    [TypeConverter(typeof(EnumTypeConverter))]
    [TypeAlias(nameof(Enumerated))]
    public struct Enumerated : IJsonValue, IEquatable<Enumerated>, IValue<string>
    {
        private static readonly string regex = @"^\[(([\w]*)[,\s]*)*]$";
        private readonly string value;

        /// <summary>
        ///     Tries to parse the enumerated value
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="enumerated"></param>
        /// <returns></returns>
        public static bool TryParse(ValueMetadata meta, out Enumerated enumerated)
        {
            if (TryParseMetadata(meta, out var values))
            {
                if (values.Contains(meta.Value))
                {
                    enumerated = new Enumerated(meta.Value, values);
                    return true;
                }

                enumerated = default;
                return false;
            }

            enumerated = default;
            return false;
        }

        private static bool TryParseMetadata(ValueMetadata metadata, out string[] values)
        {
            var match = Regex.Match(metadata.Metadata, regex);
            if (match.Success)
            {
                values = match.Groups[2]
                    .Captures
                    .Select(c => c.Value)
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Distinct()
                    .ToArray();
                return true;
            }

            values = default!;
            return false;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Enumerated" /> struct
        /// </summary>
        /// <param name="value"></param>
        /// <param name="values"></param>
        public Enumerated(string value, string[] values)
        {
            this.value = value;
            Values = values;
        }

        /// <summary>
        ///     Gets the enumerated value
        /// </summary>
        public int Index => string.IsNullOrEmpty(value) ? 0 : Array.IndexOf(Values, value);

        /// <summary>
        ///     Gets the set of values
        /// </summary>
        public string[] Values { get; }

        /// <inheritdoc />
        public string Item => this.value;

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

        /// <inheritdoc />
        public bool Equals(Enumerated other)
        {
            return value == other.value;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is Enumerated enumerated)
                return this.Equals(enumerated);
            return base.Equals(obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        /// <inheritdoc />
        public bool Equals([AllowNull] string other)
        {
            return value == other;
        }
    }
}