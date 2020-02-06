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
    /// An enumerated value
    /// </summary>
    public struct Enum<T> : IJsonValue where T : Enum
    {
        private readonly T value;

        /// <summary>
        /// Initializes a new instance of the <see cref="Label"/> struct
        /// </summary>
        /// <param name="value"></param>
        public Enum(string value)
        {
            this.value = Parse(value);
        }

        /// <summary>
        /// Parses the value returning a valid label
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static T Parse(string value)
        {
            if(Enum.TryParse(typeof(T), value, out var result))
            {
                if (result is T parsed)
                {
                    return parsed;
                }
            }
            return default;
        }

        /// <inheritdoc/>
        public override string ToString() => this.value.ToString();

        /// <inheritdoc/>
        public JToken ToJson() => new JValue(this.value);
    }
}
