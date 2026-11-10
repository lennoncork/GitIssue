using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Exceptionable;
using GitIssue.Issues;
using GitIssue.Issues.File;
using GitIssue.Issues.Markdown;
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
        private const FileStatus indexChangeFlags =
            FileStatus.NewInIndex |
            FileStatus.ModifiedInIndex |
            FileStatus.DeletedFromIndex |
            FileStatus.RenamedInIndex |
            FileStatus.TypeChangeInIndex;

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
        [Throws<ArgumentException>]
        [Throws<AggregateException>]
        public static IIssueManager Init()
        {
            return IssueManager.Init(new IssueConfiguration());
        }

        /// <summary>
        ///     Initializes a new issue manager
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        [Throws<ArgumentException>]
        [Throws<AggregateException>]
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
        [Throws<ArgumentException>]
        [Throws<AggregateException>]
        public static IIssueManager Init(string directory, string name)
        {
            return IssueManager.Init(new IssueConfiguration(), directory, name);
        }

        /// <summary>
        ///     Initializes a new issue manager
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        [Throws<ArgumentException>]
        [Throws<AggregateException>]
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
        [Throws<ArgumentException>]
        [Throws<AggregateException>]
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
        [Throws<ArgumentException>]
        [Throws<AggregateException>]
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
        [Throws<GitIssue.Exceptions.RepositoryNotFoundException>]
        public static IIssueManager Open(string directory)
        {
            return IssueManager.Open(directory, Paths.IssueRootFolderName);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="IssueManager" /> mass
        /// </summary>
        [Throws<GitIssue.Exceptions.RepositoryNotFoundException>]
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
        [Throws<SafeResultException>]
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

            string issuesRelativePath = this.getIssuesRelativePath();
            if (this.hasIndexChangesUnderIssues(issuesRelativePath))
            {
                this.logger?.Error("Cannot commit, another commit is in progress");
                return Task.FromResult(false);
            }

            List<PreservedIndexChange> preserved = this.captureIndexChangesOutsideIssues(issuesRelativePath);
            try
            {
                this.unstagePreservedChanges(preserved);
                this.unstageExcludedIssueFiles();
                this.stageIssueChanges(issuesRelativePath);
                this.Repository.Index.Write();

                string comments = this.Changes.GenerateComments();
                Configuration config = this.Repository.Config;
                Signature? author = config.BuildSignature(DateTimeOffset.Now);
                this.Repository.Commit(comments, author, author);
                this.Changes.Clear();
                this.Changes.Save(this.Root.ChangeLog);
                return Task.FromResult(true);
            }
            finally
            {
                this.restorePreservedChanges(preserved);
            }
        }

        /// <inheritdoc />
        [Throws<SafeResultException>]
        public IIssue Create(string title)
        {
            return this.CreateAsync(title)
                .WithSafeResult()
                .GetResult();
        }

        /// <inheritdoc />
        [Throws<SafeResultException>]
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
            IIssue issue = this.issueCreation(this.KeyProvider.Next(title));
            issue.Title = title;
            issue.Description = description;
            DateTimeOffset created = DateTimeOffset.Now;
            issue.Created = created.LocalDateTime;
            issue.Author = this.Repository.Config.BuildSignature(created);
            await issue.SaveAsync();
            this.Changes.Add(issue, ChangeType.Create);
            return issue;
        }

        /// <inheritdoc />
        [Throws<SafeResultException>]
        public IReadOnlyList<IssueRekeyResult> Rekey(bool dryRun = false)
        {
            return this.RekeyAsync(dryRun)
                .WithSafeResult()
                .GetResult();
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<IssueRekeyResult>> RekeyAsync(bool dryRun = false)
        {
            if (this.KeyProvider is not TemplateIssueKeyProvider provider)
            {
                throw new InvalidOperationException("Issue keys can only be regenerated when a template key provider is configured.");
            }

            List<IIssue> issues = new List<IIssue>();
            foreach ((IssueKey key, string relativePath) in provider.FindExisting())
            {
                IIssue? loaded = await this.loadIssue(key, relativePath);
                if (loaded != null)
                {
                    issues.Add(loaded);
                }
            }

            issues.Sort((left, right) => ((System.DateTime)left.Created).CompareTo((System.DateTime)right.Created));

            int nextIndex = 1;
            foreach (IIssue issue in issues)
            {
                if (provider.TryGetContext(issue.Key.ToString(), out IssueKeyContext existing) && (existing.Index > 0))
                {
                    nextIndex = Math.Max(nextIndex, existing.Index + 1);
                }
            }

            List<(Issue Issue, IssueKey From, IssueKey To, string FromPath)> planned = new List<(Issue, IssueKey, IssueKey, string)>();
            HashSet<string> used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (IIssue item in issues)
            {
                if (item is not Issue issue)
                {
                    continue;
                }

                IssueKeyContext context = provider.CreateContext(issue);
                if (provider.Template.HasIndex && (context.Index <= 0))
                {
                    context = new IssueKeyContext
                    {
                        Type = context.Type,
                        Created = context.Created,
                        Index = nextIndex++,
                        Uid = context.Uid,
                        Name = context.Name,
                    };
                }

                IssueKey to = provider.Create(context);
                if (!used.Add(to.ToString()))
                {
                    throw new InvalidOperationException($"Key template '{provider.TemplateSource}' produced duplicate key '{to}'.");
                }

                if (to == issue.Key)
                {
                    continue;
                }

                string toPath = Path.GetFullPath(Path.Combine(this.Root.IssuesPath, provider.GetIssuePath(to)));
                string fromPath = Path.GetFullPath(issue.Root.IssuePath);
                if (Directory.Exists(toPath) && !string.Equals(toPath, fromPath, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException($"Cannot rename '{issue.Key}' to '{to}' because that path already exists.");
                }

                planned.Add((issue, issue.Key, to, fromPath));
            }

            List<IssueRekeyResult> results = planned
                .Select(item => new IssueRekeyResult(item.From, item.To))
                .ToList();
            if (dryRun)
            {
                return results;
            }

            foreach ((Issue issue, IssueKey from, IssueKey to, string fromPath) in planned)
            {
                IssueRoot newRoot = new IssueRoot(this.Root, to, provider.GetIssuePath(to));
                issue.Relocate(newRoot);
                await issue.SaveAsync();
                this.Changes.Add(to, ChangeType.Rename);

                string newPath = Path.GetFullPath(issue.Root.IssuePath);
                if (!string.Equals(fromPath, newPath, StringComparison.OrdinalIgnoreCase) && Directory.Exists(fromPath))
                {
                    Directory.Delete(fromPath, true);
                    this.deleteEmptyParents(fromPath, this.Root.IssuesPath);
                }

                if (this.Tracked.Key == from)
                {
                    this.Tracked.Key = to;
                    await this.Tracked.SaveAsync(this.Root.Tracked, this.logger);
                }
            }

            return results;
        }

        /// <inheritdoc />
        [Throws<SafeResultException>]
        public bool Delete(string id)
        {
            return this.DeleteAsync(id)
                .WithSafeResult()
                .GetResult();
        }


        /// <inheritdoc />
        [Throws<SafeResultException>]
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
                return await this.DeleteAsync(key);
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
        [Throws<SafeResultException>]
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

        private static bool isIndexChange(StatusEntry entry)
        {
            return (entry.State & IssueManager.indexChangeFlags) != 0;
        }

        private List<PreservedIndexChange> captureIndexChangesOutsideIssues(string issuesRelativePath)
        {
            List<PreservedIndexChange> preserved = new List<PreservedIndexChange>();
            foreach (StatusEntry entry in this.Repository.RetrieveStatus())
            {
                if (!IssueManager.isIndexChange(entry))
                {
                    continue;
                }

                string? previousPath = entry.HeadToIndexRenameDetails?.OldFilePath;
                if (this.isUnderIssues(entry.FilePath, issuesRelativePath) ||
                    ((previousPath != null) && this.isUnderIssues(previousPath, issuesRelativePath)))
                {
                    continue;
                }

                IndexEntry? indexEntry = this.Repository.Index[entry.FilePath];
                if (indexEntry == null)
                {
                    preserved.Add(new PreservedIndexChange(entry.FilePath, null, null, Mode.Nonexistent));
                    continue;
                }

                Blob? blob = this.Repository.Lookup<Blob>(indexEntry.Id);
                if (blob == null)
                {
                    continue;
                }

                preserved.Add(new PreservedIndexChange(entry.FilePath, previousPath, blob, indexEntry.Mode));
            }

            return preserved;
        }

        private string getFullPath(string relativePath)
        {
            return Path.GetFullPath(Path.Combine(this.Root.RootPath, relativePath));
        }

        private string getIssuesRelativePath()
        {
            return Path.GetRelativePath(this.Root.RootPath, this.Root.IssuesPath)
                .Replace('\\', '/');
        }

        private bool hasIndexChangesUnderIssues(string issuesRelativePath)
        {
            foreach (StatusEntry entry in this.Repository.RetrieveStatus())
            {
                if (!IssueManager.isIndexChange(entry))
                {
                    continue;
                }

                if (this.isBlockingIssuePath(entry.FilePath, issuesRelativePath))
                {
                    return true;
                }

                string? previousPath = entry.HeadToIndexRenameDetails?.OldFilePath;
                if ((previousPath != null) && this.isBlockingIssuePath(previousPath, issuesRelativePath))
                {
                    return true;
                }
            }

            return false;
        }

        private bool isBlockingIssuePath(string relativePath, string issuesRelativePath)
        {
            return this.isUnderIssues(relativePath, issuesRelativePath) &&
                !this.isExcludedFromIssueCommit(relativePath);
        }

        private bool isExcludedFromIssueCommit(string relativePath)
        {
            string fullPath = this.getFullPath(relativePath);
            return fullPath.Equals(Path.GetFullPath(this.Root.ChangeLog), StringComparison.Ordinal) ||
                fullPath.Equals(Path.GetFullPath(this.Root.Tracked), StringComparison.Ordinal);
        }

        private bool isUnderIssues(string relativePath, string issuesRelativePath)
        {
            string path = relativePath.Replace('\\', '/');
            string root = issuesRelativePath.TrimEnd('/');
            if ((root == ".") || (root.Length == 0))
            {
                return true;
            }

            return path.Equals(root, StringComparison.Ordinal) ||
                path.StartsWith(root + "/", StringComparison.Ordinal);
        }

        private Task<IIssue?> loadIssue(IssueKey key, string relativePath)
        {
            IssueRoot issueRoot = new IssueRoot(this.Root, key, relativePath);
            if (this.Configuration.Storage == IssueStorage.Markdown)
            {
                return MarkdownIssue.ReadAsync(issueRoot, this.Configuration.Fields);
            }

            return FileIssue.ReadAsync(issueRoot, this.Configuration.Fields);
        }

        private void deleteEmptyParents(string path, string stopAt)
        {
            DirectoryInfo? current = Directory.GetParent(path);
            string stop = Path.GetFullPath(stopAt);
            while ((current != null) &&
                   current.Exists &&
                   (Path.GetFullPath(current.FullName) != stop) &&
                   !current.EnumerateFileSystemInfos().Any())
            {
                DirectoryInfo? parent = current.Parent;
                current.Delete();
                current = parent;
            }
        }

        private void restorePreservedChanges(List<PreservedIndexChange> preserved)
        {
            if (preserved.Count == 0)
            {
                return;
            }

            foreach (PreservedIndexChange change in preserved)
            {
                if ((change.PreviousPath != null) && (this.Repository.Index[change.PreviousPath] != null))
                {
                    this.Repository.Index.Remove(change.PreviousPath);
                }

                if (change.Blob == null)
                {
                    if (this.Repository.Index[change.Path] != null)
                    {
                        this.Repository.Index.Remove(change.Path);
                    }

                    continue;
                }

                this.Repository.Index.Add(change.Blob, change.Path, change.Mode);
            }

            this.Repository.Index.Write();
        }

        private void stageIssueChanges(string issuesRelativePath)
        {
            string issuesPathSpec = issuesRelativePath.TrimEnd('/') + "/*";
            foreach (StatusEntry item in this.Repository.RetrieveStatus(new StatusOptions
            {
                PathSpec = new[] { issuesPathSpec },
                IncludeIgnored = true,
                IncludeUntracked = true,
                RecurseIgnoredDirs = true,
                RecurseUntrackedDirs = true,
            }))
            {
                if (this.isExcludedFromIssueCommit(item.FilePath))
                {
                    continue;
                }

                if (this.Repository.Ignore.IsPathIgnored(item.FilePath) &&
                    ((item.State & FileStatus.Ignored) != 0))
                {
                    continue;
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
        }

        private void unstageExcludedIssueFiles()
        {
            this.unstageIfPresent(Path.GetRelativePath(this.Root.RootPath, this.Root.ChangeLog).Replace('\\', '/'));
            this.unstageIfPresent(Path.GetRelativePath(this.Root.RootPath, this.Root.Tracked).Replace('\\', '/'));
        }

        private void unstageIfPresent(string relativePath)
        {
            bool stagedInHead = !this.Repository.Info.IsHeadUnborn &&
                (this.Repository.Head.Tip[relativePath] != null);
            if ((this.Repository.Index[relativePath] == null) && !stagedInHead)
            {
                return;
            }

            Commands.Unstage(this.Repository, relativePath);
        }

        private void unstagePreservedChanges(List<PreservedIndexChange> preserved)
        {
            foreach (PreservedIndexChange change in preserved)
            {
                Commands.Unstage(this.Repository, change.Path);
                if (change.PreviousPath != null)
                {
                    Commands.Unstage(this.Repository, change.PreviousPath);
                }
            }
        }

        private readonly struct PreservedIndexChange
        {
            public PreservedIndexChange(string path, string? previousPath, Blob? blob, Mode mode)
            {
                this.Path = path;
                this.PreviousPath = previousPath;
                this.Blob = blob;
                this.Mode = mode;
            }

            public string Path { get; }

            public string? PreviousPath { get; }

            public Blob? Blob { get; }

            public Mode Mode { get; }
        }
    }
}
