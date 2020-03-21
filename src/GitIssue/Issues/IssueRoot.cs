using System.IO;

namespace GitIssue.Issues
{
    /// <summary>
    ///     Specifies the location on disk of a given issue
    /// </summary>
    public struct IssueRoot
    {
        private readonly string? issuePath;

        /// <summary>
        ///     Initializes a new instance of the <see cref="IssueRoot" /> class.
        /// </summary>
        /// <param name="root">the repository root</param>
        /// <param name="issuePath">this path to locate the issue</param>
        /// <param name="key">this issue key</param>
        public IssueRoot(RepositoryRoot root, IssueKey key, string? issuePath)
        {
            this.issuePath = issuePath;
            Root = root;
            Key = key;
        }

        /// <summary>
        ///     Gets the <see cref="RepositoryRoot" />
        /// </summary>
        public RepositoryRoot Root { get; }

        /// <summary>
        ///     Gets the <see cref="IssueKey" /> for the issue
        /// </summary>
        public IssueKey Key { get; }

        /// <summary>
        ///     Gets the <see cref="IssuePath" /> for the issue
        /// </summary>
        public string IssuePath => Path.Combine(Root.IssuesPath, this.issuePath ?? Key.ToString());
    }
}