using System;
using System.IO;
using System.Linq;
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
using GitIssue.Tool.Commands.Sync;
using GitIssue.Tool.Commands.Track;
using Serilog;
using CommitOptions = GitIssue.Tool.Commands.Commit.CommitOptions;

namespace GitIssue.Tool
{
    internal class Program
    {
        private static ILogger? logger;

        private static async Task<int> execAsync<TC, T>(T options)
            where TC : Command<T>
            where T : Options
        {
            try
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

                builder.RegisterType<ProcessRunner>()
                    .As<IProcessRunner>()
                    .SingleInstance();

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
                return await Program.execAsync(command.Exec, options);
            }
            catch (Exception e)
            {
                Program.logger?.Error($"Exception caught when preparing command: {e.Message}", e);
                return 1;
            }
        }

        private static async Task<int> execAsync<T>(Func<T, Task> func, T value)
            where T : Options
        {
            try
            {
                Console.WriteLine();
                await func(value);
                Console.WriteLine();
                return 0;
            }
            catch (Exception e)
            {
                Program.logger?.Error($"Exception caught when executing command: {e.Message}", e);
                return 1;
            }
        }

        internal static int Main(string[] args)
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

            int exitCode = 1;
            parser.ParseArguments<InitOptions, CreateOptions, DeleteOptions, FindOptions,
                    ShowOptions, AddOptions, RemoveOptions, EditOptions, TrackOptions,
                    FieldsOptions, CommitOptions, ExportOptions, ChangesOptions,
                    ImportOptions, SyncOptions,
                    CommentOptions>(args)
                .WithParsed<InitOptions>(o => exitCode = Program.execAsync<InitCommand, InitOptions>(o).GetAwaiter().GetResult())
                .WithParsed<CreateOptions>(o => exitCode = Program.execAsync<CreateCommand, CreateOptions>(o).GetAwaiter().GetResult())
                .WithParsed<DeleteOptions>(o => exitCode = Program.execAsync<DeleteCommand, DeleteOptions>(o).GetAwaiter().GetResult())
                .WithParsed<ImportOptions>(o => exitCode = Program.execAsync<ImportCommand, ImportOptions>(o).GetAwaiter().GetResult())
                .WithParsed<SyncOptions>(o => exitCode = Program.execAsync<SyncCommand, SyncOptions>(o).GetAwaiter().GetResult())
                .WithParsed<ExportOptions>(o => exitCode = Program.execAsync<ExportCommand, ExportOptions>(o).GetAwaiter().GetResult())
                .WithParsed<FindOptions>(o => exitCode = Program.execAsync<FindCommand, FindOptions>(o).GetAwaiter().GetResult())
                .WithParsed<ShowOptions>(o => exitCode = Program.execAsync<ShowCommand, ShowOptions>(o).GetAwaiter().GetResult())
                .WithParsed<AddOptions>(o => exitCode = Program.execAsync<AddCommand, AddOptions>(o).GetAwaiter().GetResult())
                .WithParsed<RemoveOptions>(o => exitCode = Program.execAsync<RemoveCommand, RemoveOptions>(o).GetAwaiter().GetResult())
                .WithParsed<EditOptions>(o => exitCode = Program.execAsync<EditCommand, EditOptions>(o).GetAwaiter().GetResult())
                .WithParsed<CommentOptions>(o => exitCode = Program.execAsync<CommentCommand, CommentOptions>(o).GetAwaiter().GetResult())
                .WithParsed<FieldsOptions>(o => exitCode = Program.execAsync<FieldsCommand, FieldsOptions>(o).GetAwaiter().GetResult())
                .WithParsed<TrackOptions>(o => exitCode = Program.execAsync<TrackCommand, TrackOptions>(o).GetAwaiter().GetResult())
                .WithParsed<CommitOptions>(o => exitCode = Program.execAsync<CommitCommand, CommitOptions>(o).GetAwaiter().GetResult())
                .WithParsed<ChangesOptions>(o => exitCode = Program.execAsync<ChangesCommand, ChangesOptions>(o).GetAwaiter().GetResult())
                .WithNotParsed(errors =>
                {
                    if (errors.Any(error => error is HelpRequestedError ||
                            error is HelpVerbRequestedError ||
                            error is VersionRequestedError))
                    {
                        exitCode = 0;
                    }
                });

            return exitCode;
        }
    }
}