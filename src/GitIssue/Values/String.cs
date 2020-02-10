using GitIssue.Json;
using Newtonsoft.Json.Linq;

namespace GitIssue.Values
{
    /// <summary>
    ///     String value type
    /// </summary>
    public struct String : IJsonValue
    {
        private readonly string value;

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

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is String str)
                if (str.value == value)
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