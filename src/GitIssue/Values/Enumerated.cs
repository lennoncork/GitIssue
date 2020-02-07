using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using GitIssue.Converters;
using GitIssue.Json;
using GitIssue.Values;
using Newtonsoft.Json.Linq;

namespace GitIssue.Fields
{
    /// <summary>
    /// An enumerated value
    /// </summary>
    [TypeConverter(typeof(EnumTypeConverter))]
    public struct Enumerated : IJsonValue
    {
        private static string regex = @"^\[(([\w]*)[,\s]*)*]$";
        private readonly string value;
        private readonly string[] values;

        /// <summary>
        /// Tries to parse a string to the email address
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="enumerated"></param>
        /// <returns></returns>
        public static bool TryParse(ValueMetadata meta, out Enumerated enumerated)
        {
            if (TryParseMetadata(meta, out string[] values))
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
            Match match = Regex.Match(metadata.Metadata, regex);
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

            values = default;
            return false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Label"/> struct
        /// </summary>
        /// <param name="value"></param>
        /// <param name="values"></param>
        public Enumerated(string value, string[] values)
        {
            this.value = value;
            this.values = values;
        }

        /// <summary>
        /// Gets the enumerated value
        /// </summary>
        public int Index => string.IsNullOrEmpty(this.value) ? 0 : Array.IndexOf(this.values, this.value);

        /// <summary>
        /// Gets the set of values
        /// </summary>
        public string[] Values => this.values;

        /// <inheritdoc/>
        public override string ToString() => this.value?.ToString();

        /// <inheritdoc/>
        public JToken ToJson() => new JValue(this.value);

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            if (obj is Enumerated enumerated)
            {
                if (enumerated.value == this.value)
                    return true;
            }
            return base.Equals(obj);
        }
    }
}
