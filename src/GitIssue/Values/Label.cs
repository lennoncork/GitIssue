using System.ComponentModel;
using GitIssue.Converters;
using GitIssue.Issues.Json;
using Newtonsoft.Json.Linq;

namespace GitIssue.Values
{
    /// <summary>
    ///     A simple label class
    /// </summary>
    [TypeConverter(typeof(LabelTypeConverter))]
    [TypeAlias(nameof(Label))]
    public struct Label : IJsonValue
    {
        private readonly string value;

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
            this.value = !string.IsNullOrWhiteSpace(value) ? value.Split()[0].ToLowerInvariant() : null;
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

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is Label label)
                if (value == label.value)
                    return true;

            return base.Equals(obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return value.GetHashCode();
        }
    }
}