using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace GitIssue.Issues
{
    /// <summary>
    ///     The base abstract key provider
    /// </summary>
    public abstract class IssueKeyProvider : IIssueKeyProvider
    {
        /// <summary>
        ///     The default number of hexadecimal characters in a generated unique id
        /// </summary>
        protected const int UniqueIdLength = 8;

        /// <inheritdoc />
        public abstract IEnumerable<IssueKey> Keys { get; }

        /// <inheritdoc />
        public abstract string GetIssuePath(IssueKey key);

        /// <inheritdoc />
        public abstract IssueKey Next();

        /// <inheritdoc />
        public virtual IssueKey Next(string name)
        {
            return this.Next();
        }

        /// <inheritdoc />
        public abstract bool TryGetKey(string value, out IssueKey key);

        /// <summary>
        ///     Creates a unique hexadecimal identifier
        /// </summary>
        /// <param name="length">the number of hexadecimal characters to return</param>
        /// <returns>the unique identifier</returns>
        protected static string GetUniqueId(int length)
        {
            using SHA256 sha = SHA256.Create();
            byte[] checksum = sha.ComputeHash(Guid.NewGuid().ToByteArray());
            return BitConverter.ToString(checksum)
                .Replace("-", string.Empty)
                .ToUpperInvariant()
                .Substring(0, length);
        }
    }
}
