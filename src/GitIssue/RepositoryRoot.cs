using System.IO;
using GitIssue.Exceptions;

namespace GitIssue
{
    /// <summary>
    /// The location of the repository
    /// </summary>
    public struct RepositoryRoot
    {
        /// <summary>
        /// Gets the repository path
        /// </summary>
        public string RootPath { get; }

        /// <summary>
        /// Gets the issue path
        /// </summary>
        public string IssuesPath => Path.Combine(this.RootPath, this.Name);

        /// <summary>
        /// Gets the repository name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the path for the config file
        /// </summary>
        public string ConfigFile => Path.Combine(this.IssuesPath, Paths.ConfigFileName);

        /// <summary>
        /// Gets the issue configuration
        /// </summary>
        public IssueConfiguration Configuration => IssueConfiguration.Read(this.ConfigFile);

        /// <summary>
        /// Initializes a new instance of the <see cref="RepositoryRoot"/> struct
        /// </summary>
        /// <param name="directory">the directory of the repository</param>
        /// <param name="name">the name of the repository</param>
        internal RepositoryRoot(string directory, string name)
        {
            this.RootPath = directory;
            this.Name = name;
        }

        /// <summary>
        /// Creates a new repository root, initializes the root name
        /// </summary>
        /// <param name="directory">the directory of the repository</param>
        /// <param name="name">the name of the repository</param>
        /// <returns>the repository root</returns>
        public static RepositoryRoot Create(string directory, string name)
        {
            DirectoryInfo info = new DirectoryInfo(directory);
            string created = info.CreateSubdirectory(name).FullName;
            return new RepositoryRoot(directory, name);
        }

        /// <summary>
        /// Opens an existing repository root
        /// </summary>
        /// <param name="directory">the directory of the repository</param>
        /// <param name="name">the name of the repository</param>
        /// <returns>the repository root</returns>
        public static RepositoryRoot Open(string directory, string name)
        {
            DirectoryInfo current = new DirectoryInfo(directory);
            if (IsIssueRoot(current, name, out DirectoryInfo issues))
            {
                return new RepositoryRoot(current.FullName, name);
            }
            throw new RepositoryNotFoundException($"Failed to open directory {directory} as issue root");
        }

        /// <summary>
        /// Locates a repository
        /// </summary>
        /// <param name="directory">the directory of the repository</param>
        /// <param name="name">the name of the repository</param>
        /// <returns>the repository root</returns>
        public static RepositoryRoot Locate(string directory, string name)
        {
            DirectoryInfo current = new DirectoryInfo(directory);
            while (TryGetParent(current, out DirectoryInfo parent))
            {
                if (IsIssueRoot(current, name, out DirectoryInfo issues))
                {
                    return new RepositoryRoot(current.FullName, name);
                }
                current = parent;
            }
            return RepositoryRoot.None();
        }

        /// <summary>
        /// The none repository
        /// </summary>
        /// <returns>the repository root</returns>
        public static RepositoryRoot None()
        {
            return new RepositoryRoot(null, null);
        }

        /// <summary>
        /// Determines if the directory is an repository root
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
        /// Tries to get the parent of the directory
        /// </summary>
        /// <param name="directory">the current directory</param>
        /// <param name="parent">the parent directory</param>
        /// <returns>true if the parent directory exists</returns>
        private static bool TryGetParent(DirectoryInfo directory, out DirectoryInfo parent)
        {
            parent = directory.Parent;
            return directory.Exists && directory.Parent != null;
        }
    }
}
