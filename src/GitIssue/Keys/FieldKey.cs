using System;
using System.ComponentModel;
using GitIssue.Converters;

namespace GitIssue.Keys
{
    /// <summary>
    ///     The unique key of a issue field
    /// </summary>
    [TypeConverter(typeof(FieldKeyTypeConverter))]
    public struct FieldKey : IEquatable<FieldKey>
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
        public static FieldKey Create(string key)
        {
            return new FieldKey(key);
        }

        /// <summary>
        ///     Returns the 'none' special key
        /// </summary>
        public static FieldKey None => new FieldKey(string.Empty);

        /// <inheritdoc />
        public bool Equals(FieldKey other)
        {
            if (string.IsNullOrEmpty(key) &&
                string.IsNullOrEmpty(other.key))
                return true;

            return key == other.key;
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

        /// <inheritdoc />
        public override string ToString()
        {
            return key;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is FieldKey fieldKey)
                return Equals(fieldKey);
            if (obj is string str) return Equals(Create(str));
            return base.Equals(obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return key.GetHashCode();
        }
    }
}