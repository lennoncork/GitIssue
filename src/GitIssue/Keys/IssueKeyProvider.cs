using System.Collections.Generic;

namespace GitIssue.Keys
{
    /// <summary>
    ///     The base abstract key provider
    /// </summary>
    public abstract class IssueKeyProvider : IIssueKeyProvider
    {
        /// <inheritdoc />
        public abstract IEnumerable<IssueKey> Keys { get; }

        /// <inheritdoc />
        public abstract IssueKey Next();

        /// <inheritdoc />
        public abstract bool TryGetKey(string value, out IssueKey key);
    }
}