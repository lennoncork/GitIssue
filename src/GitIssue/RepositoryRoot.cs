using System;
using System.Diagnostics.Contracts;
using System.IO;
using Exceptionable;
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
        public bool IsOwnedRepository => RepositoryRoot.isRepositoryRoot(this.Name);

        /// <summary>
        ///     Gets the repository path
        /// </summary>
        public string RootPath { get; }

        /// <summary>
        ///     Gets the issue path
        /// </summary>
        public string IssuesPath => RepositoryRoot.isRepositoryRoot(this.Name) ? this.RootPath : Path.Combine(this.RootPath, this.Name!);

        /// <summary>
        ///     Gets the repository name
        /// </summary>
        public string? Name { get; }

        /// <summary>
        ///     Gets the path for the config file
        /// </summary>
        public string ConfigFile => Path.Combine(this.RootPath, Paths.ConfigFileName);

        /// <summary>
        ///     Gets the path to the change log
        /// </summary>
        public string ChangeLog => Path.Combine(this.RootPath, Paths.ChangeLogFileName);

        /// <summary>
        ///     Gets the path to the tracked file
        /// </summary>
        public string Tracked => Path.Combine(this.RootPath, Paths.TrackedIssueFileName);

        /// <summary>
        ///     Gets the issue configuration
        /// </summary>
        [Throws<AggregateException>]
        public IssueConfiguration Configuration => IssueConfiguration.Read(this.ConfigFile);

        /// <summary>
        ///     Initializes a new instance of the <see cref="RepositoryRoot" /> struct
        /// </summary>
        /// <param name="directory">the directory of the repository</param>
        internal RepositoryRoot(string directory)
        {
            this.RootPath = directory;
            this.Name = null;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RepositoryRoot" /> struct
        /// </summary>
        /// <param name="directory">the directory of the repository</param>
        /// <param name="name">the name of the repository</param>
        internal RepositoryRoot(string directory, string name)
        {
            this.RootPath = directory;
            this.Name = name;
        }

        /// <summary>
        ///     Creates a new repository root, initializes the root name
        /// </summary>
        /// <param name="directory">the directory of the repository</param>
        /// <param name="name">the name of the repository</param>
        /// <returns>the repository root</returns>
        public static RepositoryRoot Create(string directory, string? name)
        {
            DirectoryInfo info = new DirectoryInfo(directory);
            if (RepositoryRoot.isRepositoryRoot(name))
            {
                if (!info.Exists)
                {
                    info.Create();
                }

                return new RepositoryRoot(directory);
            }

            info.CreateSubdirectory(name!);
            return new RepositoryRoot(directory, name!);
        }

        /// <summary>
        ///     Opens an existing repository root
        /// </summary>
        /// <param name="directory">the directory of the repository</param>
        /// <param name="name">the name of the repository</param>
        /// <returns>the repository root</returns>
        public static RepositoryRoot Open(string directory, string? name)
        {
            DirectoryInfo current = new DirectoryInfo(directory);
            if (!current.Exists)
            {
                throw new RepositoryNotFoundException($"Failed to open directory {directory} as issue root");
            }

            if (RepositoryRoot.isRepositoryRoot(name))
            {
                return new RepositoryRoot(current.FullName);
            }

            if (RepositoryRoot.IsIssueRoot(current, name!, out DirectoryInfo issues))
            {
                return new RepositoryRoot(current.FullName, name!);
            }

            throw new RepositoryNotFoundException($"Failed to open directory {directory} as issue root");
        }

        /// <summary>
        ///     Locates a repository
        /// </summary>
        /// <param name="directory">the directory of the repository</param>
        /// <param name="name">the name of the repository</param>
        /// <returns>the repository root</returns>
        public static RepositoryRoot Locate(string directory, string? name)
        {
            DirectoryInfo current = new DirectoryInfo(directory);
            while (current.Exists)
            {
                if (RepositoryRoot.isRepositoryRoot(name))
                {
                    if (File.Exists(Path.Combine(current.FullName, Paths.ConfigFileName)))
                    {
                        return new RepositoryRoot(current.FullName);
                    }
                }
                else if (RepositoryRoot.IsIssueRoot(current, name!, out DirectoryInfo issues))
                {
                    return new RepositoryRoot(current.FullName, name!);
                }

                if (!RepositoryRoot.tryGetParent(current, out DirectoryInfo parent))
                {
                    break;
                }

                current = parent;
            }

            return RepositoryRoot.None;
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

        private static bool isRepositoryRoot(string? name)
        {
            return string.IsNullOrEmpty(name) || name.Equals(".", StringComparison.Ordinal);
        }

        /// <summary>
        ///     Tries to get the parent of the directory
        /// </summary>
        /// <param name="directory">the current directory</param>
        /// <param name="parent">the parent directory</param>
        /// <returns>true if the parent directory exists</returns>
        private static bool tryGetParent(DirectoryInfo directory, out DirectoryInfo parent)
        {
            parent = directory.Parent!;
            return directory.Exists && (directory.Parent != null);
        }

        /// <summary>
        ///     Gets the GIT repository at the root
        /// </summary>
        /// <returns></returns>
        [Pure]
        public IRepository GetRepository()
        {
            return new Repository(this.RootPath);
        }
    }
}
