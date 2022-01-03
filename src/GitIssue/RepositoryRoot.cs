using System.Diagnostics.Contracts;
using System.IO;
using LibGit2Sharp;
using RepositoryNotFoundException = GitIssue.Exceptions.RepositoryNotFoundException;

namespace GitIssue
{
    /// <summary>
    ///     The location of the repository
    /// </summary>
    public struct RepositoryRoot
    {
        /// <summary>
        ///     Gets a value indicating if the issue directory is shared with the repository root
        /// </summary>
        public bool IsOwnedRepository => this.Name == null;

        /// <summary>
        ///     Gets the repository path
        /// </summary>
        public string RootPath { get; }

        /// <summary>
        ///     Gets the issue path
        /// </summary>
        public string IssuesPath => this.Name == null ?
            this.RootPath :
            Path.Combine(this.RootPath, Name);

        /// <summary>
        ///     Gets the repository name
        /// </summary>
        public string? Name { get; }

        /// <summary>
        ///     Gets the path for the config file
        /// </summary>
        public string ConfigFile => Path.Combine(IssuesPath, Paths.ConfigFileName);

        /// <summary>
        ///     Gets the path to the change log
        /// </summary>
        public string ChangeLog => Path.Combine(IssuesPath, Paths.ChangeLogFileName);

        /// <summary>
        ///     Gets the path to the tracked file
        /// </summary>
        public string Tracked => Path.Combine(IssuesPath, Paths.TrackedIssueFileName);

        /// <summary>
        ///     Gets the issue configuration
        /// </summary>
        public IssueConfiguration Configuration => IssueConfiguration.Read(ConfigFile);

        /// <summary>
        ///     Initializes a new instance of the <see cref="RepositoryRoot" /> struct
        /// </summary>
        /// <param name="directory">the directory of the repository</param>
        internal RepositoryRoot(string directory)
        {
            RootPath = directory;
            Name = null;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RepositoryRoot" /> struct
        /// </summary>
        /// <param name="directory">the directory of the repository</param>
        /// <param name="name">the name of the repository</param>
        internal RepositoryRoot(string directory, string name)
        {
            RootPath = directory;
            Name = name;
        }

        /// <summary>
        ///     Creates a new repository root, initializes the root name
        /// </summary>
        /// <param name="directory">the directory of the repository</param>
        /// <param name="name">the name of the repository</param>
        /// <returns>the repository root</returns>
        public static RepositoryRoot Create(string directory, string name)
        {
            var info = new DirectoryInfo(directory);
            var created = info.CreateSubdirectory(name).FullName;
            return new RepositoryRoot(directory, name);
        }

        /// <summary>
        ///     Opens an existing repository root
        /// </summary>
        /// <param name="directory">the directory of the repository</param>
        /// <param name="name">the name of the repository</param>
        /// <returns>the repository root</returns>
        public static RepositoryRoot Open(string directory, string? name)
        {
            var current = new DirectoryInfo(directory);

            if (name == null)
                return new RepositoryRoot(current.FullName);

            if (IsIssueRoot(current, name, out var issues))
                return new RepositoryRoot(current.FullName, name);

            throw new RepositoryNotFoundException($"Failed to open directory {directory} as issue root");
        }

        /// <summary>
        ///     Locates a repository
        /// </summary>
        /// <param name="directory">the directory of the repository</param>
        /// <param name="name">the name of the repository</param>
        /// <returns>the repository root</returns>
        public static RepositoryRoot Locate(string directory, string name)
        {
            var current = new DirectoryInfo(directory);
            while (TryGetParent(current, out var parent))
            {
                if (IsIssueRoot(current, name, out var issues))
                    return new RepositoryRoot(current.FullName, name);
                current = parent;
            }
            return None;
        }

        /// <summary>
        ///     The none repository
        /// </summary>
        /// <returns>the repository root</returns>
        public static RepositoryRoot None => new RepositoryRoot(null!, null!);

        /// <summary>
        ///     Determines if the directory is an repository root
        /// </summary>
        /// <param name="directory">the directory of the repository</param>
        /// <param name="name">the name of the repository</param>
        /// <param name="root">the directory info</param>
        /// <returns>true if the directory is the repository root</returns>
        public static bool IsIssueRoot(DirectoryInfo directory, string name, out DirectoryInfo root)
        {
            root = new DirectoryInfo(Path.Combine(directory.FullName, name));
            return root.Exists;
        }

        /// <summary>
        ///     Tries to get the parent of the directory
        /// </summary>
        /// <param name="directory">the current directory</param>
        /// <param name="parent">the parent directory</param>
        /// <returns>true if the parent directory exists</returns>
        private static bool TryGetParent(DirectoryInfo directory, out DirectoryInfo parent)
        {
            parent = directory.Parent!;
            return directory.Exists && directory.Parent != null;
        }

        /// <summary>
        /// Gets the GIT repository at the root
        /// </summary>
        /// <returns></returns>
        [Pure]
        public IRepository GetRepository()
        {
            return new Repository(this.RootPath);
        }
    }
}