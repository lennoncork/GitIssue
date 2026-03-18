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
    [TypeConverter(typeof(NumberTypeConverter))]
    [TypeAlias(nameof(Number))]
    public struct Number : IJsonValue, IEquatable<Number>, IValue<double>
    {
        /// <inheritdoc />
        public double Item { get; }

        /// <summary>
        ///     Tries to parse the number value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Number Parse(string value)
        {
            return new Number(double.Parse(value));
        }

        /// <summary>
        ///     Tries to parse the number value
        /// </summary>
        /// <param name="value"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        public static bool TryParse(string value, out Number number)
        {
            if (double.TryParse(value, out double result))
            {
                number = new Number(result);
                return true;
            }

            number = default(Number)!;
            return false;
        }

        internal Number(double value)
        {
            this.Item = value;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.Item.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        ///     Implicit cast to System.DateTime
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator double(Number value)
        {
            return value.Item;
        }

        /// <summary>
        ///     Implicit cast to System.DateTime
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator Number(double value)
        {
            return new Number(value);
        }

        /// <inheritdoc />
        public JToken ToJson()
        {
            return new JValue(this.Item);
        }

        /// <inheritdoc />
        public bool Equals(Number other)
        {
            return this.Item == other.Item;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is Number number)
            {
                return this.Equals(number);
            }

            return base.Equals(obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return this.Item.GetHashCode();
        }

        /// <inheritdoc />
        public bool Equals([AllowNull] double other)
        {
            return this.Item == other;
        }
    }
}