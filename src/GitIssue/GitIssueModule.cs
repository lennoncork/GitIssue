using System.IO;
using Autofac;
using GitIssue.Issues;
using GitIssue.Issues.File;
using GitIssue.Issues.Json;
using GitIssue.Issues.Markdown;
using LibGit2Sharp;
using Serilog;
using Serilog.Core;

namespace GitIssue
{
    /// <summary>
    ///     Dependency Injection Registrations
    /// </summary>
    public class GitIssueModule : Module
    {
        /// <inheritdoc />
        protected override void Load(ContainerBuilder builder)
        {
            // Issue manager
            builder.RegisterType<IssueManager>()
                .As<IIssueManager>()
                .AsSelf()
                .SingleInstance();

            // Core services
            builder.Register(c => RepositoryRoot.Open(Directory.GetCurrentDirectory(), Paths.IssueRootFolderName))
                .As<RepositoryRoot>()
                .SingleInstance()
                .IfNotRegistered(typeof(RepositoryRoot));
            builder.Register(c => new Repository(c.Resolve<RepositoryRoot>().RootPath))
                .As<IRepository>()
                .SingleInstance()
                .IfNotRegistered(typeof(IRepository));
            builder.Register(c => IssueConfiguration.Read(c.Resolve<RepositoryRoot>().ConfigFile))
                .As<IIssueConfiguration>()
                .SingleInstance()
                .IfNotRegistered(typeof(IIssueConfiguration));
            builder.Register(c => ChangeLog.Read(c.Resolve<RepositoryRoot>().ChangeLog))
                .As<IChangeLog>()
                .SingleInstance()
                .IfNotRegistered(typeof(IChangeLog));
            builder.Register(c => TrackedIssue.Read(c.Resolve<RepositoryRoot>().Tracked))
                .As<ITrackedIssue>()
                .SingleInstance()
                .IfNotRegistered(typeof(ITrackedIssue));
            builder.Register(c => c.Resolve(c.Resolve<IIssueConfiguration>().KeyProvider.Type))
                .As<IIssueKeyProvider>()
                .SingleInstance()
                .IfNotRegistered(typeof(IIssueKeyProvider));
            builder.Register(c => Logger.None)
                .As<ILogger>()
                .SingleInstance()
                .IfNotRegistered(typeof(ILogger));

            // Issue delegates
            builder.Register(c =>
                {
                    RepositoryRoot root = c.Resolve<RepositoryRoot>();
                    IIssueKeyProvider provider = c.Resolve<IIssueKeyProvider>();
                    IIssueConfiguration configuration = c.Resolve<IIssueConfiguration>();
                    TypedIssueCreation creation = (key, issueType) =>
                    {
                        IssueRoot issueRoot = new IssueRoot(root, key, provider.GetIssuePath(key), provider.GetDocumentFileName(key));
                        IIssue issue = configuration.Storage == IssueStorage.Markdown
                            ? new MarkdownIssue(issueRoot, configuration.GetFields(issueType))
                            : new FileIssue(issueRoot, configuration.GetFields(issueType));
                        ((JsonIssue)issue).SetIssueType(issueType);
                        return issue;
                    };
                    return creation;
                })
                .AsSelf()
                .SingleInstance()
                .IfNotRegistered(typeof(TypedIssueCreation));
            builder.Register(c =>
                {
                    IIssueConfiguration configuration = c.Resolve<IIssueConfiguration>();
                    TypedIssueCreation typed = c.Resolve<TypedIssueCreation>();
                    IssueCreation creation = key => typed(key, configuration.IssueType);
                    return creation;
                })
                .AsSelf()
                .SingleInstance()
                .IfNotRegistered(typeof(IssueCreation));
            builder.Register(c =>
                {
                    RepositoryRoot root = c.Resolve<RepositoryRoot>();
                    IIssueKeyProvider provider = c.Resolve<IIssueKeyProvider>();
                    IIssueConfiguration configuration = c.Resolve<IIssueConfiguration>();
                    IssueDeletion deletion = key =>
                    {
                        IssueRoot issueRoot = new IssueRoot(root, key, provider.GetIssuePath(key), provider.GetDocumentFileName(key));
                        if (configuration.Storage == IssueStorage.Markdown)
                        {
                            return MarkdownIssue.DeleteAsync(issueRoot);
                        }

                        return FileIssue.DeleteAsync(issueRoot);
                    };
                    return deletion;
                })
                .AsSelf()
                .SingleInstance()
                .IfNotRegistered(typeof(IssueDeletion));
            builder.Register(c =>
                {
                    RepositoryRoot root = c.Resolve<RepositoryRoot>();
                    IIssueKeyProvider provider = c.Resolve<IIssueKeyProvider>();
                    IIssueConfiguration configuration = c.Resolve<IIssueConfiguration>();
                    IssueLoading read = async key =>
                    {
                        IssueRoot issueRoot = new IssueRoot(root, key, provider.GetIssuePath(key), provider.GetDocumentFileName(key));
                        if (!Directory.Exists(issueRoot.IssuePath))
                        {
                            return null;
                        }

                        string type = await IssueTypeReader.ReadAsync(issueRoot, configuration);
                        IIssue? issue;
                        if (configuration.Storage == IssueStorage.Markdown)
                        {
                            issue = await MarkdownIssue.ReadAsync(issueRoot, configuration.GetFields(type));
                        }
                        else
                        {
                            issue = await FileIssue.ReadAsync(issueRoot, configuration.GetFields(type));
                        }

                        if (issue is JsonIssue jsonIssue)
                        {
                            jsonIssue.SetIssueType(type);
                        }

                        return issue;
                    };
                    return read;
                })
                .AsSelf()
                .SingleInstance()
                .IfNotRegistered(typeof(IssueLoading));

            // Key providers
            builder.RegisterType<TemplateIssueKeyProvider>().AsSelf();
            builder.RegisterType<DateIssueKeyProvider>().AsSelf();
            builder.RegisterType<NameIssueKeyProvider>().AsSelf();
#pragma warning disable CS0618
            builder.RegisterType<FileIssueKeyProvider>().AsSelf();
#pragma warning restore CS0618
        }
    }
}
