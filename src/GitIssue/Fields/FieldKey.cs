using System;
using System.ComponentModel;
using GitIssue.Values;

namespace GitIssue.Fields
{
    /// <summary>
    ///     The unique key of a issue field
    /// </summary>
    [TypeConverter(typeof(FieldKeyTypeConverter))]
    [TypeAlias(nameof(FieldKey))]
    public struct FieldKey : IValue, IEquatable<FieldKey>
    {
        private readonly string key;

        private FieldKey(string key)
        {
            this.key = key;
        }

        /// <summary>
        ///     Creates a new field key
        /// </summary>
        /// <param name="key"></param>
        /// <returns>the new field key</returns>
        public static FieldKey Create(string? key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return FieldKey.None;
            }

            return new FieldKey(key);
        }

        /// <summary>
        ///     Returns the 'none' special key
        /// </summary>
        public static FieldKey None => new FieldKey(string.Empty);

        /// <inheritdoc />
        public bool Equals(FieldKey other)
        {
            if (string.IsNullOrEmpty(this.key) &&
                string.IsNullOrEmpty(other.key))
            {
                return true;
            }

            return this.key == other.key;
        }

        /// <summary>
        ///     Equality comparison
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static bool operator ==(FieldKey lhs, FieldKey rhs)
        {
            return lhs.Equals(rhs);
        }

        /// <summary>
        ///     Inequality comparison
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static bool operator !=(FieldKey lhs, FieldKey rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>
        ///     Implicit cast to string
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator string(FieldKey value)
        {
            return value.ToString();
        }

        /// <summary>
        ///     Implicit cast to FieldKey
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator FieldKey(string value)
        {
            return FieldKey.Create(value);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.key;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is FieldKey fieldKey)
            {
                return this.Equals(fieldKey);
            }

            if (obj is string str)
            {
                return this.Equals(FieldKey.Create(str));
            }

            return base.Equals(obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return this.key.GetHashCode();
        }
    }
}