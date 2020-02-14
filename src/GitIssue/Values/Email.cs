using System;
using System.ComponentModel;
using System.Net.Mail;
using GitIssue.Converters;
using GitIssue.Issues.Json;
using Newtonsoft.Json.Linq;

namespace GitIssue.Values
{
    /// <summary>
    ///     Version value type
    /// </summary>
    [TypeConverter(typeof(EmailTypeConverter))]
    [TypeAlias(nameof(Email))]
    public struct Email : IJsonValue
    {
        private readonly string value;

        /// <summary>
        ///     Tries to parse the email value
        /// </summary>
        /// <param name="value"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        public static bool TryParse(string value, out Email email)
        {
            email = new Email(value);
            return email.IsValid;
        }

        internal Email(string email)
        {
            try
            {
                var parsed = new MailAddress(email);
                value = parsed.ToString();
                IsValid = true;
            }
            catch (FormatException)
            {
                value = string.Empty;
                IsValid = false;
            }
        }

        /// <summary>
        ///     Gets the value determining if the email is valid
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
            if (obj is Email email)
                if (value == email.value)
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