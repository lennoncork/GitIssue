using System.ComponentModel;
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
    public struct Number : IJsonValue
    {
        private readonly double value;

        /// <summary>
        ///     Tries to parse the number value
        /// </summary>
        /// <param name="value"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        public static bool TryParse(string value, out Number number)
        {
            if (double.TryParse(value, out var result))
            {
                number = new Number(result);
                return true;
            }

            number = default!;
            return false;
        }

        internal Number(double value)
        {
            this.value = value;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        ///     Implicit cast to System.DateTime
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator double(Number value)
        {
            return value.value;
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
            return new JValue(value);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is Number number)
                if (value == number.value)
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