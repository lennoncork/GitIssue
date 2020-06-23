using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using GitIssue.Issues;
using GitIssue.Issues.File;
using LibGit2Sharp;
using Serilog;
using Serilog.Core;

namespace GitIssue
{
    /// <summary>
    /// Dependency Injection Registrations
    /// </summary>
    public class GitIssueModule : Module
    {
        /// <inheritdoc/>
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
                    IssueCreation creation = (key) => new FileIssue(new IssueRoot(root, key, provider.GetIssuePath(key)), configuration.Fields);
                    return creation;
                })
                .AsSelf()
                .SingleInstance()
                .IfNotRegistered(typeof(IssueCreation));
            builder.Register(c =>
                {
                    RepositoryRoot root = c.Resolve<RepositoryRoot>();
                    IIssueKeyProvider provider = c.Resolve<IIssueKeyProvider>();
                    IssueDeletion deletion = (key) => FileIssue.DeleteAsync(new IssueRoot(root, key, provider.GetIssuePath(key)));
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
                    IssueLoading read = (key) => FileIssue.ReadAsync(new IssueRoot(root, key, provider.GetIssuePath(key)), configuration.Fields);
                    return read;
                })
                .AsSelf()
                .SingleInstance()
                .IfNotRegistered(typeof(IssueLoading));

            // Key providers
            builder.RegisterType<FileIssueKeyProvider>().AsSelf();
        }
    }
}
