using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using GitIssue.Issues.Json;
using Newtonsoft.Json.Linq;

namespace GitIssue.Values
{
    /// <summary>
    ///     DateTime value type
    /// </summary>
    [TypeConverter(typeof(DateTimeTypeConverter))]
    [TypeAlias(nameof(DateTime))]
    public struct DateTime : IJsonValue, IEquatable<DateTime>, IValue<System.DateTime>
    {
        private readonly System.DateTime value;

        /// <summary>
        ///     Tries to parse the datetime value
        /// </summary>
        /// <param name="value"></param>
        /// <param name="datetime"></param>
        /// <returns></returns>
        public static bool TryParse(string value, out DateTime datetime)
        {
            datetime = new DateTime(value);
            return datetime.IsValid;
        }

        internal DateTime(System.DateTime value)
        {
            this.value = value;
            IsValid = true;
        }

        internal DateTime(string datetime)
        {
            try
            {
                value = System.DateTime.Parse(datetime);
                IsValid = true;
            }
            catch (FormatException)
            {
                value = System.DateTime.MinValue;
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
            return value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        ///     Implicit cast to System.DateTime
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator System.DateTime(DateTime value)
        {
            return value.value;
        }

        /// <summary>
        ///     Implicit cast to System.DateTime
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator DateTime(System.DateTime value)
        {
            return new DateTime(value);
        }

        /// <inheritdoc />
        public JToken ToJson()
        {
            return new JValue(value);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is DateTime datetime)
                return this.Equals(datetime);
            return base.Equals(obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        /// <inheritdoc />
        public bool Equals([AllowNull] DateTime other)
        {
            return value == other.value;
        }

        /// <inheritdoc />
        public bool Equals([AllowNull] System.DateTime other)
        {
            return value == other;
        }

        /// <inheritdoc />
        public System.DateTime Item => this.value;
    }
}