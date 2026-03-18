using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using GitIssue.Issues;
using LibGit2Sharp;
using Serilog;

namespace GitIssue
{
    /// <summary>
    ///     Delegates for callback on disposal
    /// </summary>
    public delegate void IssueManagerDisposal(IIssueManager manager);

    /// <summary>
    ///     Delegate for issue creation
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public delegate IIssue IssueCreation(IssueKey key);

    /// <summary>
    ///     Delegate for issue deletion
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public delegate Task<bool> IssueDeletion(IssueKey key);


    /// <summary>
    ///     Delegate for issue deletion
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public delegate Task<IIssue?> IssueLoading(IssueKey key);

    /// <summary>
    ///     Issue manager class
    /// </summary>
    public class IssueManager : IIssueManager
    {
        private readonly IssueCreation issueCreation;

        private readonly IssueDeletion issueDeletion;

        private readonly IssueLoading issueLoading;

        private readonly ILogger? logger;
        private bool disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="IssueManager" /> mass
        /// </summary>
        public IssueManager(
            ILogger logger,
            RepositoryRoot root,
            IRepository repository,
            IIssueKeyProvider provider,
            IIssueConfiguration configuration,
            IChangeLog changeLog,
            ITrackedIssue tracked,
            IssueCreation issueCreation,
            IssueDeletion issueDeletion,
            IssueLoading issueLoading)
        {
            this.logger = logger;
            this.Repository = repository;
            this.WorkingDirectory = root.RootPath;
            this.KeyProvider = provider;
            this.Root = root;
            this.Configuration = configuration;
            this.Changes = changeLog;
            this.Tracked = tracked;
            this.issueCreation = issueCreation;
            this.issueDeletion = issueDeletion;
            this.issueLoading = issueLoading;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            this.Changes.Save(this.Root.ChangeLog);
            this.disposed = true;
        }

        /// <inheritdoc />
        public IChangeLog Changes { get; protected set; }

        /// <inheritdoc />
        public IIssueConfiguration Configuration { get; protected set; }

        /// <summary>
        ///     Gets or sets the key provider
        /// </summary>
        public IIssueKeyProvider KeyProvider { get; protected set; }

        /// <inheritdoc />
        public IRepository Repository { get; protected set; }

        /// <summary>
        ///     Gets or sets the repository root
        /// </summary>
        public RepositoryRoot Root { get; protected set; }

        /// <inheritdoc />
        public ITrackedIssue Tracked { get; protected set; }

        /// <summary>
        ///     Gets or sets the working directory
        /// </summary>
        public string WorkingDirectory { get; protected set; }

        /// <summary>
        ///     Initializes a new issue manager
        /// </summary>
        /// <returns>the issue manager</returns>
        public static IIssueManager Init()
        {
            return IssueManager.Init(new IssueConfiguration());
        }

        /// <summary>
        ///     Initializes a new issue manager
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        public static IIssueManager Init(string directory)
        {
            return IssueManager.Init(new IssueConfiguration(), directory);
        }

        /// <summary>
        ///     Initializes a new issue manager
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IIssueManager Init(string directory, string name)
        {
            return IssueManager.Init(new IssueConfiguration(), directory, name);
        }

        /// <summary>
        ///     Initializes a new issue manager
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IIssueManager Init(IssueConfiguration configuration)
        {
            return IssueManager.Init(configuration, Environment.CurrentDirectory);
        }

        /// <summary>
        ///     Initializes a new issue manager
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="directory"></param>
        /// <returns></returns>
        public static IIssueManager Init(IssueConfiguration configuration, string directory)
        {
            return IssueManager.Init(configuration, directory, Paths.IssueRootFolderName);
        }

        /// <summary>
        ///     Initializes a new issue manager
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="directory"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IIssueManager Init(IssueConfiguration configuration, string directory, string name)
        {
            if (configuration == null)
            {
                throw new ArgumentException(nameof(configuration));
            }

            if (directory == null)
            {
                throw new ArgumentException(nameof(directory));
            }

            if (name == null)
            {
                throw new ArgumentException(nameof(name));
            }

            RepositoryRoot root = RepositoryRoot.Create(directory, name);
            configuration.Save(root.ConfigFile);

            ContainerBuilder builder = new ContainerBuilder();
            builder.Register(c => root).As<RepositoryRoot>().SingleInstance();
            builder.Register(c => configuration).As<IIssueConfiguration>().SingleInstance();
            builder.RegisterModule<GitIssueModule>();
            IContainer container = builder.Build();

            return container.Resolve<IIssueManager>();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="IssueManager" /> mass
        /// </summary>
        public static IIssueManager Open(string directory)
        {
            return IssueManager.Open(directory, Paths.IssueRootFolderName);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="IssueManager" /> mass
        /// </summary>
        public static IIssueManager Open(string directory, string name)
        {
            ContainerBuilder builder = new ContainerBuilder();
            RepositoryRoot root = RepositoryRoot.Open(directory, name);
            builder.Register(c => root).As<RepositoryRoot>().SingleInstance();
            return IssueManager.Open(builder);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="IssueManager" /> mass
        /// </summary>
        public static IIssueManager Open(ContainerBuilder builder)
        {
            builder.RegisterModule<GitIssueModule>();
            IContainer container = builder.Build();
            IssueManager manager = container.Resolve<IssueManager>();
            return manager;
        }

        /// <inheritdoc />
        public ValueTask DisposeAsync()
        {
            try
            {
                this.Dispose();
                return default(ValueTask);
            }
            catch (Exception exception)
            {
                return new ValueTask(Task.FromException(exception));
            }
        }

        /// <inheritdoc />
        public bool Commit()
        {
            return this.CommitAsync()
                .WithSafeResult()
                .GetResult();
        }

        /// <inheritdoc />
        public Task<bool> CommitAsync()
        {
            if (!this.Repository.Index.IsFullyMerged)
            {
                this.logger?.Error("Cannot commit, requires merge");
                return Task.FromResult(false);
            }

            if (this.Repository.RetrieveStatus().Staged.Any())
            {
                this.logger?.Error("Cannot commit, another commit is in progress");
                return Task.FromResult(false);
            }

            foreach (StatusEntry? item in this.Repository.RetrieveStatus(new StatusOptions
                     {
                         PathSpec = new[] { $"{Path.GetRelativePath(this.Root.RootPath, this.Root.IssuesPath)}/*" },
                         IncludeIgnored = true,
                         IncludeUntracked = true,
                         RecurseIgnoredDirs = true,
                         RecurseUntrackedDirs = true,
                     }))
            {
                bool ignored = this.Repository.Ignore.IsPathIgnored(item.FilePath);
                string? relative = item.FilePath;

                if (Path.GetFullPath(relative).Equals(Path.GetFullPath(this.Root.ChangeLog)))
                {
                    continue;
                }

                if (Path.GetFullPath(relative).Equals(Path.GetFullPath(this.Root.Tracked)))
                {
                    continue;
                }

                if (ignored && ((item.State & FileStatus.Ignored) != 0))
                {
                    if (Directory.Exists(Path.GetFullPath(item.FilePath)))
                    {
                        continue;
                    }

                    this.Repository.Index.Add(item.FilePath);
                }

                if (((item.State & FileStatus.NewInWorkdir) != 0) ||
                    ((item.State & FileStatus.ModifiedInWorkdir) != 0))
                {
                    this.Repository.Index.Add(item.FilePath);
                }

                if ((item.State & FileStatus.DeletedFromWorkdir) != 0)
                {
                    this.Repository.Index.Remove(item.FilePath);
                }
            }

            this.Repository.Index.Write();

            string comments = this.Changes.GenerateComments();
            Configuration? config = this.Repository.Config;
            Signature? author = config.BuildSignature(DateTimeOffset.Now);
            this.Repository.Commit(comments, author, author);
            this.Changes.Clear();
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public IIssue Create(string title)
        {
            return this.CreateAsync(title)
                .WithSafeResult()
                .GetResult();
        }

        /// <inheritdoc />
        public IIssue Create(string title, string description)
        {
            return this.CreateAsync(title, description)
                .WithSafeResult()
                .GetResult();
        }

        /// <inheritdoc />
        public async Task<IIssue> CreateAsync(string title)
        {
            return await this.CreateAsync(title, string.Empty);
        }

        /// <inheritdoc />
        public async Task<IIssue> CreateAsync(string title, string description)
        {
            IIssue issue = this.issueCreation(this.KeyProvider.Next());
            issue.Title = title;
            issue.Description = description;
            issue.Author = this.Repository.Config.BuildSignature(issue.Created.Item);
            await issue.SaveAsync();
            this.Changes.Add(issue, ChangeType.Create);
            return issue;
        }


        /// <inheritdoc />
        public bool Delete(string id)
        {
            return this.DeleteAsync(id)
                .WithSafeResult()
                .GetResult();
        }


        /// <inheritdoc />
        public bool Delete(IssueKey key)
        {
            return this.DeleteAsync(key)
                .WithSafeResult()
                .GetResult();
        }


        /// <inheritdoc />
        public async Task<bool> DeleteAsync(string id)
        {
            if (this.KeyProvider.TryGetKey(id, out IssueKey key))
            {
                await this.DeleteAsync(key);
                return true;
            }

            this.logger?.Error($"Failed to get issue key from {id}");
            return false;
        }


        /// <inheritdoc />
        public async Task<bool> DeleteAsync(IssueKey key)
        {
            bool success = await this.issueDeletion(key);
            if (success)
            {
                this.Changes.Add(key, ChangeType.Delete);
            }

            return success;
        }

        /// <inheritdoc />
        public IEnumerable<IIssue> Find(Func<IIssue, bool> predicate)
        {
            List<IIssue> issues = new List<IIssue>();
            Task.Run(async () =>
            {
                await foreach (IIssue issue in this.FindAsync(predicate))
                {
                    issues.Add(issue);
                }
            }).Wait();
            return issues;
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<IIssue> FindAsync(Func<IIssue, bool> predicated)
        {
            foreach (IssueKey key in this.KeyProvider.Keys)
            {
                IIssue? issue = await this.issueLoading(key);
                if ((issue != null) && predicated.Invoke(issue))
                {
                    yield return issue;
                }
            }
        }

        /// <inheritdoc />
        public bool Track(IssueKey key)
        {
            return this.TrackAsync(key)
                .WithSafeResult()
                .GetResult();
        }

        /// <inheritdoc />
        public async Task<bool> TrackAsync(IssueKey key)
        {
            if (this.Tracked.Key != key)
            {
                this.Tracked = new TrackedIssue(key);
                await this.Tracked.SaveAsync(this.Root.Tracked, this.logger);
                return false;
            }

            return true;
        }
    }
}