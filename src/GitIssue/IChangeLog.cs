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
        public Dictionary<IssueKey, List<string>> Changes { get; set; }

        /// <summary>
        ///     Clears all changes in the log
        /// </summary>
        public void Clear();

        /// <summary>
        ///     Records a new change in the log
        /// </summary>
        /// <param name="issue"></param>
        /// <param name="change"></param>
        public void Add(IIssue issue, ChangeType change);

        /// <summary>
        ///     Records a new change in the log
        /// </summary>
        /// <param name="issue"></param>
        /// <param name="change"></param>
        /// <param name="summary"></param>
        public void Add(IIssue issue, ChangeType change, string summary);
    }
}