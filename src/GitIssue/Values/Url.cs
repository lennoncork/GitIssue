using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
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
                {
                    url = "http://" + url;
                }

                this.Item = new Uri(url).ToString();
                this.IsValid = true;
            }
            catch (UriFormatException)
            {
                this.Item = string.Empty;
                this.IsValid = false;
            }
        }

        /// <summary>
        ///     Gets the value determining if the version is valid
        /// </summary>
        public bool IsValid { get; }

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

        /// <inheritdoc />
        public bool Equals([AllowNull] Url other)
        {
            return this.Item == other.Item;
        }


        /// <inheritdoc />
        public bool Equals([AllowNull] string other)
        {
            return this.Item == other;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is Url url)
            {
                return this.Equals(url);
            }

            return base.Equals(obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return this.Item.GetHashCode();
        }

        /// <inheritdoc />
        public string Item { get; }
    }
}