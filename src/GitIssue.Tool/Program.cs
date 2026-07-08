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

        private static async Task ExecAsync<TC, T>(T options)
            where TC : Command<T>
            where T : Options
        {
            ContainerBuilder builder = new ContainerBuilder();

            builder.Register(c => Program.logger!)
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
                        IssueConfiguration config = new IssueConfiguration();
                        RepositoryRoot root = RepositoryRoot.Create(options.Path, options.Name);
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

            using IContainer container = builder.Build();

            if (options is ITrackedOptions keyOptions)
            {
                keyOptions.Tracked = TrackedIssue
                    .Read(Path.Combine(options.Path, options.Name, options.Tracking), Program.logger);
            }

            Command<T> command = container.Resolve<Command<T>>();
            await Program.ExecAsync(command.Exec, options);
        }

        private static async Task ExecAsync<T>(Func<T, Task> func, T value)
            where T : Options
        {
            try
            {
                Console.WriteLine();
                await func(value);
                Console.WriteLine();
            }
            catch (Exception e)
            {
                Program.logger?.Error($"Exception caught when executing command: {e.Message}", e);
            }
        }

        private static void Main(string[] args)
        {
            Program.logger = new LoggerConfiguration()
                .WriteTo.Console()
                .MinimumLevel.Debug()
                .CreateLogger();

            Parser parser = new Parser(with =>
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
                .WithParsed<InitOptions>(o => Program.ExecAsync<InitCommand, InitOptions>(o).Wait())
                .WithParsed<CreateOptions>(o => Program.ExecAsync<CreateCommand, CreateOptions>(o).Wait())
                .WithParsed<DeleteOptions>(o => Program.ExecAsync<DeleteCommand, DeleteOptions>(o).Wait())
                .WithParsed<ImportOptions>(o => Program.ExecAsync<ImportCommand, ImportOptions>(o).Wait())
                .WithParsed<ExportOptions>(o => Program.ExecAsync<ExportCommand, ExportOptions>(o).Wait())
                .WithParsed<FindOptions>(o => Program.ExecAsync<FindCommand, FindOptions>(o).Wait())
                .WithParsed<ShowOptions>(o => Program.ExecAsync<ShowCommand, ShowOptions>(o).Wait())
                .WithParsed<AddOptions>(o => Program.ExecAsync<AddCommand, AddOptions>(o).Wait())
                .WithParsed<RemoveOptions>(o => Program.ExecAsync<RemoveCommand, RemoveOptions>(o).Wait())
                .WithParsed<EditOptions>(o => Program.ExecAsync<EditCommand, EditOptions>(o).Wait())
                .WithParsed<CommentOptions>(o => Program.ExecAsync<CommentCommand, CommentOptions>(o).Wait())
                .WithParsed<FieldsOptions>(o => Program.ExecAsync<FieldsCommand, FieldsOptions>(o).Wait())
                .WithParsed<TrackOptions>(o => Program.ExecAsync<TrackCommand, TrackOptions>(o).Wait())
                .WithParsed<CommitOptions>(o => Program.ExecAsync<CommitCommand, CommitOptions>(o).Wait())
                .WithParsed<ChangesOptions>(o => Program.ExecAsync<ChangesCommand, ChangesOptions>(o).Wait());
        }
    }
}