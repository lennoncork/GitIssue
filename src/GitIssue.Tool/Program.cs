using System;
using System.IO;
using System.Threading.Tasks;
using Autofac;
using CommandLine;
using GitIssue.Issues;
using GitIssue.Tool.Commands;
using GitIssue.Tool.Commands.Add;
using GitIssue.Tool.Commands.Changes;
using GitIssue.Tool.Commands.Comment;
using GitIssue.Tool.Commands.Commit;
using GitIssue.Tool.Commands.Create;
using GitIssue.Tool.Commands.Delete;
using GitIssue.Tool.Commands.Edit;
using GitIssue.Tool.Commands.Export;
using GitIssue.Tool.Commands.Fields;
using GitIssue.Tool.Commands.Find;
using GitIssue.Tool.Commands.Import;
using GitIssue.Tool.Commands.Init;
using GitIssue.Tool.Commands.Remove;
using GitIssue.Tool.Commands.Show;
using GitIssue.Tool.Commands.Track;
using Serilog;
using CommitOptions = GitIssue.Tool.Commands.Commit.CommitOptions;

namespace GitIssue.Tool
{
    internal class Program
    {
        private static ILogger? logger;

        private static void Main(string[] args)
        {
            logger = new LoggerConfiguration()
                .WriteTo.Console()
                .MinimumLevel.Debug()
                .CreateLogger();

            var parser = new Parser(with =>
            {
                with.EnableDashDash = true;
                with.AutoHelp = true;
                with.CaseSensitive = false;
                with.HelpWriter = Console.Error;
            });

            parser.ParseArguments<InitOptions, CreateOptions, DeleteOptions, FindOptions,
                    ShowOptions, AddOptions, RemoveOptions, EditOptions, TrackOptions,
                    FieldsOptions, CommitOptions, ExportOptions, ChangesOptions,
                    ImportOptions,
                    CommentOptions>(args)
                .WithParsed<InitOptions>(o => ExecAsync<InitCommand, InitOptions>(o).Wait())
                .WithParsed<CreateOptions>(o => ExecAsync<CreateCommand, CreateOptions>(o).Wait())
                .WithParsed<DeleteOptions>(o => ExecAsync<DeleteCommand, DeleteOptions>(o).Wait())
                .WithParsed<ImportOptions>(o => ExecAsync<ImportCommand, ImportOptions>(o).Wait())
                .WithParsed<ExportOptions>(o => ExecAsync<ExportCommand, ExportOptions>(o).Wait())
                .WithParsed<FindOptions>(o => ExecAsync<FindCommand, FindOptions>(o).Wait())
                .WithParsed<ShowOptions>(o => ExecAsync<ShowCommand, ShowOptions>(o).Wait())
                .WithParsed<AddOptions>(o => ExecAsync<AddCommand, AddOptions>(o).Wait())
                .WithParsed<RemoveOptions>(o => ExecAsync<RemoveCommand, RemoveOptions>(o).Wait())
                .WithParsed<EditOptions>(o => ExecAsync<EditCommand, EditOptions>(o).Wait())
                .WithParsed<CommentOptions>(o => ExecAsync<CommentCommand, CommentOptions>(o).Wait())
                .WithParsed<FieldsOptions>(o => ExecAsync<FieldsCommand, FieldsOptions>(o).Wait())
                .WithParsed<TrackOptions>(o => ExecAsync<TrackCommand, TrackOptions>(o).Wait())
                .WithParsed<CommitOptions>(o => ExecAsync<CommitCommand, CommitOptions>(o).Wait())
                .WithParsed<ChangesOptions>(o => ExecAsync<ChangesCommand, ChangesOptions>(o).Wait());
        }

        private static async Task ExecAsync<TC, T>(T options)
            where TC : Command<T>
            where T : Options
        {
            var builder = new ContainerBuilder();

            builder.Register(c => logger!)
                .As<ILogger>()
                .SingleInstance();

            builder.RegisterType<Configuration>()
                .As<IssueConfiguration>()
                .AsSelf()
                .SingleInstance();

            builder.Register(c =>
                {
                    // Initialize the repository root and save the configuration
                    InitCommand.Initializer onInitCommand = () =>
                    {
                        var config = new IssueConfiguration();
                        var root = RepositoryRoot.Create(options.Path, options.Name);
                        config.Save(root.ConfigFile);
                    };
                    return onInitCommand;
                })
                .As<InitCommand.Initializer>();

            builder.Register(c => RepositoryRoot.Open(options.Path, options.Name))
                .As<RepositoryRoot>()
                .SingleInstance();

            builder.RegisterType<TC>()
                .As<Command<T>>();

            builder.RegisterType<Editor>()
                .As<IEditor>()
                .AsSelf();

            builder.RegisterModule<GitIssueModule>();

            using var container = builder.Build();

            if (options is ITrackedOptions keyOptions)
                keyOptions.Tracked = TrackedIssue
                    .Read(Path.Combine(options.Path, options.Name, options.Tracking), logger);

            var command = container.Resolve<Command<T>>();
            await ExecAsync(command.Exec, options);
        }

        private static async Task ExecAsync<T>(Func<T, Task> func, T value)
            where T : Options
        {
            await Task.Run(async () =>
            {
                try
                {
                    Console.WriteLine();
                    await func(value);
                    Console.WriteLine();
                }
                catch (Exception e)
                {
                    logger?.Error($"Exception caught when executing command: {e.Message}", e);
                }
            });
        }
    }
}