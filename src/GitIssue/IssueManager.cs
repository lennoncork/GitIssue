using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GitIssue.Issues;
using GitIssue.Providers;
using LibGit2Sharp;
using Serilog;

namespace GitIssue
{
    /// <summary>
    /// Issue manager class
    /// </summary>
    public class IssueManager : IIssueManager
    {
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="IssueManager"/> mass 
        /// </summary>
        public IssueManager() : this(Environment.CurrentDirectory)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IssueManager"/> mass 
        /// </summary>
        /// <param name="path">the repository path</param>
        public IssueManager(string path) : this(new Repository(path), path, Paths.IssueRootFolderName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IssueManager"/> mass 
        /// </summary>
        /// <param name="path">the repository path</param>
        /// <param name="name">the issues directory name</param>
        public IssueManager(string path, string name) : this(new Repository(path), path, name)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IssueManager"/> mass 
        /// </summary>
        /// <param name="path">the repository path</param>
        /// <param name="logger">the logger</param>
        public IssueManager(string path, ILogger logger) : this(new Repository(path), path, Paths.IssueRootFolderName)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IssueManager"/> mass 
        /// </summary>
        /// <param name="repository">the git repository</param>
        /// <param name="path">the path for the repository path</param>
        /// <param name="name">the issues directory name</param>
        public IssueManager(IRepository repository, string path, string name)
        {
            this.Repository = repository;
            this.WorkingDirectory = path;
            this.FolderName = name;
            this.Root = RepositoryRoot.Open(this.WorkingDirectory, this.FolderName);
            this.KeyProvider = new FileIssueKeyProvider(this.Root);
            this.Configuration = IssueConfiguration.Read(this.Root.ConfigFile);
        }

        /// <summary>
        /// Gets or sets the issue name
        /// </summary>
        public string FolderName { get; protected set; }

        /// <summary>
        /// Gets or sets the working directory
        /// </summary>
        public string WorkingDirectory { get; protected set; }

        /// <summary>
        /// Gets or sets the key provider
        /// </summary>
        public IIssueKeyProvider KeyProvider { get; protected set; }

        /// <summary>
        /// Gets or sets the repository root
        /// </summary>
        public RepositoryRoot Root { get; protected set; }

        /// <inheritdoc/>
        public IssueConfiguration Configuration { get; protected set; }

        /// <inheritdoc/>
        public IRepository Repository { get; protected set; }

        /// <summary>
        /// Initializes a new issue manager
        /// </summary>
        /// <returns>the issue manager</returns>
        public static IIssueManager Init() => Init(new IssueConfiguration());

        /// <summary>
        /// Initializes a new issue manager
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        public static IIssueManager Init(string directory) => 
            Init(new IssueConfiguration(), directory);

        /// <summary>
        /// Initializes a new issue manager
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IIssueManager Init(string directory, string name) =>
            Init(new IssueConfiguration(), directory, name);

        /// <summary>
        /// Initializes a new issue manager
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IIssueManager Init(IssueConfiguration configuration) =>
            Init(configuration, Environment.CurrentDirectory);

        /// <summary>
        /// Initializes a new issue manager
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="directory"></param>
        /// <returns></returns>
        public static IIssueManager Init(IssueConfiguration configuration, string directory) =>
            Init(configuration, directory, Paths.IssueRootFolderName);

        /// <summary>
        /// Initializes a new issue manager
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="directory"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IIssueManager Init(IssueConfiguration configuration, string directory, string name)
        {
            if(configuration == null) throw new ArgumentException(nameof(configuration));
            if(directory == null) throw new ArgumentException(nameof(directory));
            if(name == null) throw new ArgumentException(nameof(name));

            RepositoryRoot root = RepositoryRoot.Create(directory, name);
            configuration.Save(root.ConfigFile);

            return new IssueManager(directory, name)
            {
                Root = root,
                Configuration = configuration,
                Repository = new Repository(directory)
            };
        }

        /// <inheritdoc/>
        public async Task<IIssue> CreateAsync(string title) =>
            await this.CreateAsync(title, string.Empty);

        /// <inheritdoc/>
        public async Task<IIssue> CreateAsync(string title, string description)
        {
            Issue issue = new FileIssue(new IssueRoot(this.Root, this.KeyProvider.Next()), this.Configuration.Fields)
            {
                Title = title,
                Description = description,
            };
            await issue.SaveAsync();
            return issue;
        }

        /// <inheritdoc/>
        public IEnumerable<IIssue> Find(Func<IIssue, bool> predicate)
        {
            List<IIssue> issues = new List<IIssue>();
            Task.Run(async () =>
            {
                await foreach (var issue in this.FindAsync(predicate))
                {
                    issues.Add(issue);
                }
            }).Wait();
            return issues;
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<IIssue> FindAsync(Func<IIssue, bool> predicated)
        {
            foreach (var key in this.KeyProvider.Keys)
            {
                IIssue issue = await FileIssue.ReadAsync(new IssueRoot(this.Root, key), this.Configuration.Fields);
                if (predicated.Invoke(issue))
                    yield return issue;
            }
        }

        void IIssueManager.Delete(string id)
        {
            throw new NotImplementedException();
        }

        void IIssueManager.Delete(IssueKey key)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteAsync(string id)
        {
            if (this.KeyProvider.TryGetKey(id, out IssueKey key))
            {
                await this.DeleteAsync(key);
            }
            this.logger?.Debug($"Failed to get issue key from {id}");
        }

        public async Task DeleteAsync(IssueKey key)
        {
            await FileIssue.DeleteAsync(new IssueRoot(this.Root, key));
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Repository?.Dispose();
        }

        public IIssue Create(string title)
        {
            throw new NotImplementedException();
        }

        public IIssue Create(string title, string description)
        {
            throw new NotImplementedException();
        }
    }
}
