using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using GitIssue.Json;
using Newtonsoft.Json.Linq;

namespace GitIssue.Values
{
    /// <summary>
    /// Version value type
    /// </summary>
    public class Url : IJsonValue
    {
        private readonly Uri uri;

        /// <summary>
        /// Tries to parse a string to the email address
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
                this.uri = new Uri(url);
                this.IsValid = true;
            }
            catch (UriFormatException)
            {

            }
        }

        /// <summary>
        /// Gets the value determining if the version is valid
        /// </summary>
        public bool IsValid { get; protected set; } = false;

        /// <inheritdoc/>
        public override string ToString() => this.uri?.ToString();

        /// <inheritdoc />
        public JToken ToJson() => new JValue(this.ToString());
    }
}
