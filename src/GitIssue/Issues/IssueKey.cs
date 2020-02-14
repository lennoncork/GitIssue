using System;
using System.ComponentModel;
using GitIssue.Converters;

namespace GitIssue.Issues
{
    /// <summary>
    ///     Issue key (unique identifier)
    /// </summary>
    [TypeConverter(typeof(IssuedKeyTypeConverter))]
    public struct IssueKey : IEquatable<IssueKey>
    {
        private readonly string key;

        /// <summary>
        ///     Initialized a new instance of the key
        /// </summary>
        /// <param name="key">the key string</param>
        private IssueKey(string key)
        {
            if (key == nameof(None))
                this.key = string.Empty;
            else
                this.key = key;
        }

        /// <summary>
        ///     Creates a new instance of the key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static IssueKey Create(string key)
        {
            return new IssueKey(key);
        }

        /// <summary>
        ///     Creates an empty (none) key
        /// </summary>
        /// <returns></returns>
        public static IssueKey None => new IssueKey(string.Empty);

        /// <summary>
        ///     Equals comparison operator overload
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool operator ==(IssueKey x, IssueKey y)
        {
            if (string.IsNullOrEmpty(x.key) &&
                string.IsNullOrEmpty(y.key))
                return true;
            return x.key == y.key;
        }

        /// <summary>
        ///     None Equals comparison operator overload
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool operator !=(IssueKey x, IssueKey y)
        {
            return !(x == y);
        }

        /// <summary>
        ///     Implicit cast to string
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator string(IssueKey value) => value.ToString();

        /// <summary>
        ///     Implicit cast to IssueKey
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator IssueKey(string value) => IssueKey.Create(value);

        /// <inheritdoc />
        public override string ToString()
        {
            return string.IsNullOrEmpty(key) ? nameof(None) : key;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return key.GetHashCode();
        }

        /// <inheritdoc />
        public override bool Equals(object? obj) => key.Equals(obj);

        /// <inheritdoc />
        public bool Equals(IssueKey other) => key == other.key;
    }
}