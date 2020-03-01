using System.IO;

namespace GitIssue.Syncs
{
    /// <summary>
    ///     Specifies the location on disk of an import source
    /// </summary>
    public struct SyncRoot
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SyncRoot" /> class.
        /// </summary>
        public SyncRoot(RepositoryRoot root, string source)
        {
            Root = root;
            Source = source;
        }

        /// <summary>
        ///     Gets the <see cref="RepositoryRoot" />
        /// </summary>
        public RepositoryRoot Root { get; }

        /// <summary>
        ///     Gets the source for the import 
        /// </summary>
        public string Source { get; }

        /// <summary>
        ///     Gets the <see cref="ImportPath" /> for the issue
        /// </summary>
        public string ImportPath => Path.Combine(Root.IssuesPath, Source);
    }
}