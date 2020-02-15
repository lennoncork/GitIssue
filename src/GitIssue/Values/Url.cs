using System;
using GitIssue.Issues.Json;
using Newtonsoft.Json.Linq;

namespace GitIssue.Values
{
    /// <summary>
    ///     Version value type
    /// </summary>
    [TypeAlias(nameof(Url))]
    public struct Url : IJsonValue
    {
        private readonly string value;

        /// <summary>
        ///     Tries to parse a string to the email address
        /// </summary>
        /// <param name="str"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static bool TryParse(string str, out Url url)
        {
            url = new Url(str);
            return url.IsValid;
        }

        internal Url(string url)
        {
            try
            {
                value = new Uri(url).ToString();
                IsValid = true;
            }
            catch (UriFormatException)
            {
                value = string.Empty;
                IsValid = false;
            }
        }

        /// <summary>
        ///     Gets the value determining if the version is valid
        /// </summary>
        public bool IsValid { get; }

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
            if (obj is Url url)
                if (value == url.value)
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