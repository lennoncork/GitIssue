using System;
using System.IO;
using System.Threading.Tasks;
using CommandLine;
using Serilog;

namespace GitIssue.Util
{
    internal class Program
    {
        public static ILogger Logger;

        private static void Main(string[] args)
        {
            var parser = new Parser(with =>
            {
                with.EnableDashDash = true;
                with.AutoHelp = true;
                with.CaseSensitive = false;
                with.HelpWriter = Console.Error;
            });

            parser.ParseArguments<InitOptions, CreateOptions, DeleteOptions, FindOptions,
                    ShowOptions, EditOptions, TrackOptions, FieldsOptions, CommitOptions>(args)
                .WithParsed<InitOptions>(o => ExecAsync<InitCommand, InitOptions>(o).Wait())
                .WithParsed<CreateOptions>(o => ExecAsync<CreateCommand, CreateOptions>(o).Wait())
                .WithParsed<DeleteOptions>(o => ExecAsync<DeleteCommand, DeleteOptions>(o).Wait())
                .WithParsed<FindOptions>(o => ExecAsync<FindCommand, FindOptions>(o).Wait())
                .WithParsed<ShowOptions>(o => ExecAsync<ShowCommand,ShowOptions>(o).Wait())
                .WithParsed<EditOptions>(o => ExecAsync<EditCommand, EditOptions>(o).Wait())
                .WithParsed<FieldsOptions>(o => ExecAsync<FieldsCommand, FieldsOptions>(o).Wait())
                .WithParsed<TrackOptions>(o => ExecAsync<TrackCommand, TrackOptions>(o).Wait())
                .WithParsed<CommitOptions>(o => ExecAsync<CommitCommand, CommitOptions>(o).Wait());
        }

        public static IIssueManager Initialize(Options options)
        {
            Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .MinimumLevel.Debug()
                .CreateLogger();

            if (options is KeyOptions keyOptions)
                keyOptions.Tracked = TrackedIssue
                    .Read(Path.Combine(options.Path, options.Name, options.Tracking), Logger);

            //Configuration configuration = Configuration.Read();

            return new IssueManager(options.Path, options.Name, Logger);
        }

        private static async Task ExecAsync<TC, T>(T value) 
            where TC : Command<T> 
            where T : Options
        {
            TC command = Activator.CreateInstance<TC>();
            await ExecAsync(command.Exec, value);
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
                }
                catch (Exception e)
                {
                    Logger?.Error($"Exception caught when executing command: {e.Message}", e);
                }
            });
        }
    }
}