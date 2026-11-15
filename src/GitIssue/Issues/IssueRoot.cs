using Exceptionable;

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
        /// <param name="documentFile">the document file name when it is not the key</param>
        public IssueRoot(RepositoryRoot root, IssueKey key, string? issuePath, string? documentFile = null)
        {
            this.issuePath = issuePath;
            this.Root = root;
            this.Key = key;
            this.DocumentFile = documentFile;
        }

        /// <summary>
        ///     Gets the document file name when the file is not named after the key.
        /// </summary>
        public string? DocumentFile { get; }

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
        [Throws<System.ArgumentException>]
        public string IssuePath => Paths.GetContainedPath(this.Root.IssuesPath, this.issuePath ?? this.Key.ToString());
    }
}
