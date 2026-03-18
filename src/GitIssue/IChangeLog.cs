using System.Collections.Generic;
using GitIssue.Issues;

namespace GitIssue
{
    /// <summary>
    ///     Represents the issue manager's change log
    /// </summary>
    public interface IChangeLog
    {
        /// <summary>
        ///     Gets the dictionary of changes
        /// </summary>
        Dictionary<IssueKey, List<string>> Log { get; set; }

        /// <summary>
        ///     Records a new change in the log
        /// </summary>
        /// <param name="key"></param>
        /// <param name="change"></param>
        void Add(IssueKey key, ChangeType change);

        /// <summary>
        ///     Records a new change in the log
        /// </summary>
        /// <param name="key"></param>
        /// <param name="change"></param>
        /// <param name="summary"></param>
        void Add(IssueKey key, ChangeType change, string summary);

        /// <summary>
        ///     Records a new change in the log
        /// </summary>
        /// <param name="issue"></param>
        /// <param name="change"></param>
        void Add(IIssue issue, ChangeType change);

        /// <summary>
        ///     Records a new change in the log
        /// </summary>
        /// <param name="issue"></param>
        /// <param name="change"></param>
        /// <param name="summary"></param>
        void Add(IIssue issue, ChangeType change, string summary);

        /// <summary>
        ///     Clears all changes in the log
        /// </summary>
        void Clear();

        /// <summary>
        ///     Saved the change log to a file
        /// </summary>
        /// <param name="file"></param>
        void Save(string file);
    }
}