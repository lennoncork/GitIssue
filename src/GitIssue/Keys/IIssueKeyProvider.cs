using System.Collections.Generic;

namespace GitIssue.Keys
{
    /// <summary>
    /// Issue key provider interface
    /// </summary>
    public interface IIssueKeyProvider
    {
        /// <summary>
        /// The set of issue keys
        /// </summary>
        IEnumerable<IssueKey> Keys { get; }

        /// <summary>
        /// Tries to get the specified key from a string
        /// </summary>
        /// <param name="value"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        bool TryGetKey(string value, out IssueKey key);

        /// <summary>
        /// The next, unique, issue key
        /// </summary>
        /// <returns></returns>
        IssueKey Next();
    }
}