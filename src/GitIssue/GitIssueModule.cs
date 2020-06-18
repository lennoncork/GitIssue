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

            // Key providers
            builder.RegisterType<FileIssueKeyProvider>().AsSelf();
        }
    }
}
