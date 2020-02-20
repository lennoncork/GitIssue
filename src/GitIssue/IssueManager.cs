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
            throw new NotImplementedException();
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

            var files = Directory.EnumerateFiles(Root.IssuesPath, "*.*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var relative = Path.GetRelativePath(Root.RootPath, file);

                if (Repository.Ignore.IsPathIgnored(relative))
                    continue;

                if (Path.GetFullPath(relative).Equals(Path.GetFullPath(Root.ChangeLog)))
                    continue;

                Repository.Index.Add(relative);
                Repository.Index.Write();
            }

            var builder = new StringBuilder();
            var count = 0;
            foreach (var changes in Changes.Log)
            {
                if (count++ > 0) builder.AppendLine();
                builder.AppendLine($"Issue: {changes.Key}");
                foreach (var change in changes.Value) builder.AppendLine($" - {change}");
            }

            var config = Repository.Config;
            var author = config.BuildSignature(DateTimeOffset.Now);
            Repository.Commit(builder.ToString(), author, author);
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
            Issue issue = new FileIssue(this, new IssueRoot(Root, KeyProvider.Next()), Configuration.Fields)
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
            throw new NotImplementedException();
        }


        /// <inheritdoc />
        public bool Delete(IssueKey key)
        {
            throw new NotImplementedException();
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
            await FileIssue.DeleteAsync(new IssueRoot(Root, key));
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
            return Task.Run(() => CreateAsync(title)).Result;
        }

        /// <inheritdoc />
        public IIssue Create(string title, string description)
        {
            return Task.Run(() => CreateAsync(title, description)).Result;
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
    }
}