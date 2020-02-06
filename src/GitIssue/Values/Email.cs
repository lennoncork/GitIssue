using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using GitIssue.Json;
using Newtonsoft.Json.Linq;

namespace GitIssue.Values
{
    /// <summary>
    /// Version value type
    /// </summary>
    public class Email : IJsonValue
    {
        private readonly MailAddress email;

        /// <summary>
        /// Tries to parse a string to the email address
        /// </summary>
        /// <param name="str"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        public static bool TryParse(string str, out Email email)
        {
            email = new Email(str);
            return email.IsValid;
        }

        internal Email(string email)
        {
            try
            {
                this.email = new MailAddress(email);
                this.IsValid = true;
            }
            catch (FormatException)
            {

            }
        }

        /// <summary>
        /// Gets the value determining if the version is valid
        /// </summary>
        public bool IsValid { get; protected set; } = false;

        /// <inheritdoc/>
        public override string ToString() => this.email?.ToString();

        /// <inheritdoc />
        public JToken ToJson() => new JValue(this.ToString());
    }
}
