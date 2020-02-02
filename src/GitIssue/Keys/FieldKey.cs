using System;
using System.ComponentModel;
using GitIssue.Converters;

namespace GitIssue.Keys
{
    /// <summary>
    /// The unique key of a issue field
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
        /// Creates a new field key
        /// </summary>
        /// <param name="key"></param>
        /// <returns>the new field key</returns>
        public static FieldKey Create(string key) => new FieldKey(key);

        /// <inheritdoc/>
        public bool Equals(FieldKey other)
        {
            return this.key == other.key;
        }

        /// <inheritdoc/>
        public override string ToString() => key;
    }
}
