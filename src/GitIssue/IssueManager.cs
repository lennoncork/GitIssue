using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GitIssue.Issues;
using GitIssue.Issues.File;
using LibGit2Sharp;
using Serilog;
using Serilog.Core;

namespace GitIssue
{
    /// <summary>
    ///     Issue manager class
    /// </summary>
    public class IssueManager : IIssueManager
    {
        private readonly ILogger? logger;

        /// <summary>
        ///     Initializes a new instance of the <see cref="IssueManager" /> mass
        /// </summary>
        public IssueManager(string directory) : this(directory, Paths.IssueRootFolderName)
        {

        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="IssueManager" /> mass
        /// </summary>
        public IssueManager(string directory, string name)
        {
            this.logger = Logger.None;
            this.Root = RepositoryRoot.Open(directory, name);
            this.Repository = new Repository(directory);
            this.WorkingDirectory = this.Root.RootPath;
            this.KeyProvider = new FileIssueKeyProvider(this.Root);
            this.Configuration = IssueConfiguration.Read(Root.ConfigFile);
            this.Changes = ChangeLog.Read(Root.ChangeLog);
            this.Tracked = TrackedIssue.Read(Root.Tracked);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="IssueManager" /> mass
        /// </summary>
        public IssueManager(
            ILogger? logger, 
            RepositoryRoot root, 
            IRepository repository, 
            IIssueKeyProvider keyProvider, 
            IChangeLog changeLog,
            IIssueConfiguration configuration, 
            ITrackedIssue tracked)
        {
            this.logger = logger;
            this.Repository = repository;
            this.WorkingDirectory = root.RootPath;
            this.Root = root;
            this.KeyProvider = keyProvider;
            this.Configuration = configuration;
            this.Changes = changeLog;
            this.Tracked = tracked;
        }

        /// <summary>
        ///     Gets or sets the key provider
        /// </summary>
        public IIssueKeyProvider KeyProvider { get; protected set; }

        /// <summary>
        ///     Gets or sets the repository root
        /// </summary>
        public RepositoryRoot Root { get; protected set; }

        /// <summary>
        ///     Gets or sets the working directory
        /// </summary>
        public string WorkingDirectory { get; protected set; }

        /// <inheritdoc />
        public IIssueConfiguration Configuration { get; protected set; }

        /// <inheritdoc />
        public IChangeLog Changes { get; protected set; }

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
            if (Repository.Index.IsFullyMerged == false)
            {
                logger?.Error("Cannot commit, requires merge");
                return Task.FromResult(false);
            }

            if (Repository.RetrieveStatus().Staged.Any())
            {
                logger?.Error("Cannot commit, another commit is in progress");
                return Task.FromResult(false);
            }

            foreach (var item in Repository.RetrieveStatus(new StatusOptions()
            {
                PathSpec = new[] { $"{Path.GetRelativePath(Root.RootPath, Root.IssuesPath)}/*"  },
                IncludeIgnored = true,
                IncludeUntracked = true,
                RecurseIgnoredDirs = true,
                RecurseUntrackedDirs = true,
            }))
            {
                bool ignored = Repository.Ignore.IsPathIgnored(Root.Name);
                var relative = item.FilePath;

                if (Path.GetFullPath(relative).Equals(Path.GetFullPath(Root.ChangeLog)))
                    continue;

                if (Path.GetFullPath(relative).Equals(Path.GetFullPath(Root.Tracked)))
                    continue;

                if (ignored && (item.State & FileStatus.Ignored) != 0)
                {
                    if (Directory.Exists(Path.GetFullPath(item.FilePath)))
                        continue;
                    Repository.Index.Add(item.FilePath);
                }

                if ((item.State & FileStatus.NewInWorkdir) != 0 ||
                    (item.State & FileStatus.ModifiedInWorkdir) != 0)
                    Repository.Index.Add(item.FilePath);

                if ((item.State & FileStatus.DeletedFromWorkdir) != 0)
                    Repository.Index.Remove(item.FilePath);
            }

            Repository.Index.Write();

            string comments = Changes.GenerateComments();
            var config = Repository.Config;
            var author = config.BuildSignature(DateTimeOffset.Now);
            Repository.Commit(comments, author, author);
            Changes.Clear();
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public IRepository Repository { get; protected set; }

        /// <inheritdoc />
        public ITrackedIssue Tracked { get; protected set; }

        /// <inheritdoc />
        public async Task<IIssue> CreateAsync(string title)
        {
            return await CreateAsync(title, string.Empty);
        }

        /// <inheritdoc />
        public async Task<IIssue> CreateAsync(string title, string description)
        {
            Issue issue = new FileIssue(this, KeyProvider.Next(), Configuration.Fields)
            {
                Title = title,
                Description = description
            };
            await issue.SaveAsync();
            Changes.Add(issue, ChangeType.Create);
            return issue;
        }

        /// <inheritdoc />
        public IEnumerable<IIssue> Find(Func<IIssue, bool> predicate)
        {
            var issues = new List<IIssue>();
            Task.Run(async () =>
            {
                await foreach (var issue in FindAsync(predicate)) issues.Add(issue);
            }).Wait();
            return issues;
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<IIssue> FindAsync(Func<IIssue, bool> predicated)
        {
            foreach (var key in KeyProvider.Keys)
            {
                var issue = await FileIssue.ReadAsync(this, new IssueRoot(Root, key), Configuration.Fields);
                if (issue != null && predicated.Invoke(issue))
                    yield return issue;
            }
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
            if (KeyProvider.TryGetKey(id, out var key))
            {
                await DeleteAsync(key);
                return true;
            }

            logger?.Error($"Failed to get issue key from {id}");
            return false;
        }


        /// <inheritdoc />
        public async Task<bool> DeleteAsync(IssueKey key)
        {
            if (await FileIssue.DeleteAsync(new IssueRoot(Root, key)))
            {
                this.Changes.Add(key, ChangeType.Delete);
            }
            return true;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Repository?.Dispose();
            Changes.Save(Root.ChangeLog);
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
        public ValueTask DisposeAsync()
        {
            try
            {
                Dispose();
                return default;
            }
            catch (Exception exception)
            {
                return new ValueTask(Task.FromException(exception));
            }
        }

        /// <summary>
        ///     Initializes a new issue manager
        /// </summary>
        /// <returns>the issue manager</returns>
        public static IIssueManager Init()
        {
            return Init(new IssueConfiguration());
        }

        /// <summary>
        ///     Initializes a new issue manager
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        public static IIssueManager Init(string directory)
        {
            return Init(new IssueConfiguration(), directory);
        }

        /// <summary>
        ///     Initializes a new issue manager
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IIssueManager Init(string directory, string name)
        {
            return Init(new IssueConfiguration(), directory, name);
        }

        /// <summary>
        ///     Initializes a new issue manager
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IIssueManager Init(IssueConfiguration configuration)
        {
            return Init(configuration, Environment.CurrentDirectory);
        }

        /// <summary>
        ///     Initializes a new issue manager
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="directory"></param>
        /// <returns></returns>
        public static IIssueManager Init(IssueConfiguration configuration, string directory)
        {
            return Init(configuration, directory, Paths.IssueRootFolderName);
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
            if (configuration == null) throw new ArgumentException(nameof(configuration));
            if (directory == null) throw new ArgumentException(nameof(directory));
            if (name == null) throw new ArgumentException(nameof(name));

            var root = RepositoryRoot.Create(directory, name);
            configuration.Save(root.ConfigFile);
            var logger = Logger.None;
            var repository = new Repository(directory);
            var keyProvider = new FileIssueKeyProvider(root);
            var changeLog = new ChangeLog();
            var tracked = TrackedIssue.Read(root.Tracked);

            return new IssueManager(logger, root, repository, keyProvider, changeLog, configuration, tracked);
        }

        /// <inheritdoc />
        public async Task<bool> TrackAsync(IssueKey key)
        {
            if (this.Tracked.Key != key)
            {
                this.Tracked = new TrackedIssue(key);
                await this.Tracked.SaveAsync(Root.Tracked, this.logger);
                return false;
            }
            return true;
        }

        /// <inheritdoc />
        public bool Track(IssueKey key)
        {
            return this.TrackAsync(key)
                .WithSafeResult()
                .GetResult();
        }
    }
}