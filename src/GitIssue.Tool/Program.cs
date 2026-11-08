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
using GitIssue.Tool.Commands.Convert;
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
using GitIssue.Tool.Commands.Validate;
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
                            if (options is InitOptions initOptions)
                            {
                                if (string.IsNullOrWhiteSpace(initOptions.Storage) ||
                                    !Enum.TryParse(initOptions.Storage, true, out IssueStorage storage))
                                {
                                    throw new ArgumentException($"Storage '{initOptions.Storage}' is not Json or Markdown.");
                                }

                                config.Storage = storage;
                            }

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
                return await Program.execAsync(command, options);
            }
            catch (Exception e)
            {
                Program.logger?.Error($"Exception caught when preparing command: {e.Message}", e);
                return 1;
            }
        }

        private static async Task<int> execAsync<T>(Command<T> command, T value)
            where T : Options
        {
            try
            {
                Console.WriteLine();
                await command.Exec(value);
                Console.WriteLine();
                return command.ExitCode;
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
            Type[] commands =
            {
                typeof(InitOptions),
                typeof(CreateOptions),
                typeof(DeleteOptions),
                typeof(FindOptions),
                typeof(ShowOptions),
                typeof(AddOptions),
                typeof(RemoveOptions),
                typeof(EditOptions),
                typeof(TrackOptions),
                typeof(FieldsOptions),
                typeof(CommitOptions),
                typeof(ExportOptions),
                typeof(ChangesOptions),
                typeof(ImportOptions),
                typeof(SyncOptions),
                typeof(CommentOptions),
                typeof(ValidateOptions),
                typeof(ConvertOptions),
            };
            parser.ParseArguments(args, commands)
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
                .WithParsed<ValidateOptions>(o => exitCode = Program.execAsync<ValidateCommand, ValidateOptions>(o).GetAwaiter().GetResult())
                .WithParsed<ConvertOptions>(o => exitCode = Program.execAsync<ConvertCommand, ConvertOptions>(o).GetAwaiter().GetResult())
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