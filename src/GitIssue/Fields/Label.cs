using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using GitIssue.Converters;
using GitIssue.Json;
using Newtonsoft.Json.Linq;

namespace GitIssue.Fields
{
    /// <summary>
    /// A simple label class
    /// </summary>
    [TypeConverter(typeof(LabelTypeConverter))]
    public struct Label : IJsonValue
    {
        private readonly string value;

        /// <summary>
        /// Initializes a new instance of the <see cref="Label"/> struct
        /// </summary>
        /// <param name="value"></param>
        public Label(string value)
        {
            this.value = Parse(value);
        }

        /// <summary>
        /// Parses the value returning a valid label
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static string Parse(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;
            return value.Split()[0].ToLowerInvariant();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;
            return this.value;
        }

        /// <inheritdoc/>
        public JToken ToJson() => new JValue(this.value);
    }
}
