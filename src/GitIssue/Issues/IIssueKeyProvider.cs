using System.Collections.Generic;

namespace GitIssue.Issues
{
    /// <summary>
    ///     Issue key provider interface
    /// </summary>
    public interface IIssueKeyProvider
    {
        /// <summary>
        ///     The set of issue keys
        /// </summary>
        IEnumerable<IssueKey> Keys { get; }

        /// <summary>
        ///     Gets the path for the issue key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        string GetIssuePath(IssueKey key);

        /// <summary>
        ///     The next, unique, issue key
        /// </summary>
        /// <returns></returns>
        IssueKey Next();

        /// <summary>
        ///     The next unique issue key, using the supplied name when the provider supports named keys
        /// </summary>
        /// <param name="name">the issue name or title</param>
        /// <returns></returns>
        IssueKey Next(string name);

        /// <summary>
        ///     Tries to get the specified key from a string
        /// </summary>
        /// <param name="value"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        bool TryGetKey(string value, out IssueKey key);
    }
}
