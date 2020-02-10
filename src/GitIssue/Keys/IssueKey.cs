using System;

namespace GitIssue.Keys
{
    /// <summary>
    ///     Issue key (unique identifier)
    /// </summary>
    public struct IssueKey : IEquatable<IssueKey>
    {
        private readonly string key;

        /// <summary>
        ///     Initialized a new instance of the key
        /// </summary>
        /// <param name="key">the key string</param>
        private IssueKey(string key)
        {
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
        public static IssueKey None()
        {
            return new IssueKey(string.Empty);
        }

        /// <summary>
        ///     Equals comparison operator overload
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool operator ==(IssueKey x, IssueKey y)
        {
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

        /// <inheritdoc />
        public override string ToString()
        {
            return key;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return key.GetHashCode();
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return key.Equals(obj);
        }

        /// <inheritdoc />
        public bool Equals(IssueKey other)
        {
            return key == other.key;
        }
    }
}