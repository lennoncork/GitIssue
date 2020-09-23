using System;
using System.ComponentModel;
using System.Net.Mail;
using System.Text.RegularExpressions;
using GitIssue.Issues.Json;
using Newtonsoft.Json.Linq;

namespace GitIssue.Values
{
    /// <summary>
    ///     Version value type
    /// </summary>
    [TypeConverter(typeof(SignatureTypeConverter))]
    [TypeAlias(nameof(Signature))]
    public struct Signature : IJsonValue, IEquatable<Signature>, IValue
    {
        private static readonly string invalid = "Unknown";

        private static readonly string regex = @"^([\w\s\d]*) <([\w\d.]*@[\w\d.]*)>$";
        private string Username { get; }

        private string Email { get; }

        /// <summary>
        /// An invalid signature
        /// </summary>
        public static Signature Invalid => new Signature(null!, null!);

        /// <summary>
        ///     Parses the value as an Signature
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Signature Parse(string value)
        {
            return new Signature(value);
        }

        /// <summary>
        ///     Tries to parse the Signature value
        /// </summary>
        /// <param name="value"></param>
        /// <param name="signature"></param>
        /// <returns></returns>
        public static bool TryParse(string value, out Signature signature)
        {
            signature = new Signature(value);
            return signature.IsValid;
        }

        internal Signature(string signature)
        {
            Match match = Regex.Match(signature, regex);
            if (match.Success && match.Groups.Count == 3)
            {
                this.Username = match.Groups[1].Value;
                this.Email = match.Groups[2].Value;
                this.IsValid = true;
                return;
            }
            this.Username = null!;
            this.Email = null!;
            this.IsValid = false;
        }

        internal Signature(string username, string email)
        {
            this.Username = username;
            this.Email = email;
            this.IsValid = username != null && email != null;
        }

        /// <summary>
        ///     Gets the value determining if the email is valid
        /// </summary>
        public bool IsValid { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            if (this.IsValid)
            {
                return $"{this.Username} <{this.Email}>";
            }
            return invalid;
        }

        /// <summary>
        ///     Implicit cast to System.DateTime
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator Signature(LibGit2Sharp.Signature value)
        {
            if(value == null || value.Name == null || value.Email == null)
                return Signature.Invalid;
            return new Signature(value.Name, value.Email);
        }

        /// <inheritdoc />
        public JToken ToJson()
        {
            return new JValue(this.ToString());
        }

        /// <inheritdoc />
        public bool Equals(Signature other)
        {
            return this.Username == other.Username && this.Email == other.Email;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is Signature signature)
                return this.Equals(signature);
            return base.Equals(obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Username.GetHashCode() + Email.GetHashCode();
        }
    }
}