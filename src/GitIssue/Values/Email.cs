using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Net.Mail;
using GitIssue.Issues.Json;
using Newtonsoft.Json.Linq;

namespace GitIssue.Values
{
    /// <summary>
    ///     Version value type
    /// </summary>
    [TypeConverter(typeof(EmailTypeConverter))]
    [TypeAlias(nameof(Email))]
    public struct Email : IJsonValue, IEquatable<Email>, IValue<string>
    {
        /// <summary>
        ///     Parses the value as an email address
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Email Parse(string value)
        {
            return new Email(value);
        }

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
                MailAddress parsed = new MailAddress(email);
                this.Item = parsed.ToString();
                this.IsValid = true;
            }
            catch (FormatException)
            {
                this.Item = string.Empty;
                this.IsValid = false;
            }
        }

        /// <summary>
        ///     Gets the value determining if the email is valid
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
        public bool Equals(Email other)
        {
            return this.Item == other.Item;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is Email email)
            {
                return this.Equals(email);
            }

            return base.Equals(obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return this.Item.GetHashCode();
        }

        /// <inheritdoc />
        public bool Equals([AllowNull] string other)
        {
            return this.Item == other;
        }

        /// <inheritdoc />
        public string Item { get; }
    }
}