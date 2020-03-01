using System;
using System.ComponentModel;
using GitIssue.Issues.Json;
using Newtonsoft.Json.Linq;

namespace GitIssue.Values
{
    /// <summary>
    ///     Version value type
    /// </summary>
    [TypeAlias(nameof(Url))]
    [TypeConverter(typeof(UrlTypeConverter))]
    public struct Url : IJsonValue, IEquatable<Url>, IValue<string>
    {
        private readonly string value;

        /// <summary>
        ///     Parse a string to the url
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static Url Parse(string str)
        {
            return new Url(str);
        }

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
                if (url.StartsWith("www."))
                    url = "http://" + url;
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
        public bool Equals(Url other)
        {
            return value == other.value;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is Url url)
                return this.Equals(url);
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