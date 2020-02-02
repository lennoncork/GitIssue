using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using Serilog;
using Serilog.Core;
using Serilog.Core.Sinks;
using System.Linq.Expressions;
using DynamicExpression = System.Linq.Dynamic.DynamicExpression;

namespace GitIssue.Util
{
    class Program
    {
        /// <summary>
        /// Command Line Options
        /// </summary>
        public class Options
        {
            /// <summary>
            /// The version type to deserialize
            /// </summary>
            [Option("path", Required = false, HelpText = "The working path")]
            public string Path { get; set; } = Environment.CurrentDirectory;
        }

        [Verb(nameof(Command.Init))]
        public class InitOptions : Options
        {

        }

        [Verb(nameof(Command.Create))]
        public class CreateOptions : Options
        {
            [Value(1, HelpText = "The issue title", Required = true)]
            public string Title { get; set; }
        }

        [Verb(nameof(Command.Delete))]
        public class DeleteOptions : Options
        {
            [Value(1, HelpText = "The issue key", Required = true)]
            public string Key { get; set; }
        }

        [Verb(nameof(Command.Find))]
        public class FindOptions : Options
        {
            [Option("LinqName", HelpText = "The name of the issue in the linq expression", Required = false)]
            public string Name { get; set; } = "i";

            [Value(1, HelpText = "The LINQ expression to use when matching", Required = true)]
            public string Linq { get; set; }
        }

        public static ILogger logger;

        static void Main(string[] args)
        {
            logger = new LoggerConfiguration()
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

            parser.ParseArguments<InitOptions, CreateOptions, DeleteOptions, FindOptions>(args)
                .WithParsed<InitOptions>(o => ExecAsync(Init, o).Wait())
                .WithParsed<CreateOptions>(o => ExecAsync(Create, o).Wait())
                .WithParsed<DeleteOptions>(o => ExecAsync(Delete, o).Wait())
                .WithParsed<FindOptions>(o => ExecAsync(Find, o).Wait());
        }

        static async Task ExecAsync<T>(Func<T, Task> func, T value)
        {
            await Task.Run(async () =>
            {
                try
                {
                    await func(value);
                }
                catch (Exception e)
                {
                    logger?.Error($"Exception caught when executing command: {e.Message}", e);
                }
            });
        }

        public static Task Init(InitOptions options)
        {
            IssueManager.Init(options.Path);
            return Task.CompletedTask;
        }

        public static async Task Create(CreateOptions options)
        {
            await using IssueManager issues = new IssueManager(options.Path, logger);
            await issues.CreateAsync(options.Title);
        }

        public static async Task Delete(DeleteOptions options)
        {
            await using IssueManager issues = new IssueManager(options.Path, logger);
            await issues.DeleteAsync(options.Key);
        }

        public static async Task Find(FindOptions options)
        {
            await using IssueManager issues = new IssueManager(options.Path, logger);
            Delegate evaluation;
            try
            {
                var parameter = Expression.Parameter(typeof(IIssue), options.Name);
                var expression = DynamicExpression.ParseLambda(new[] {parameter}, typeof(bool), options.Linq);
                evaluation = expression.Compile();
            }
            catch (Exception e)
            {
                logger.Error($"Unable to parse expression {e.Message}", e);
                return;
            }

            var find = issues.FindAsync(i => (bool)evaluation.DynamicInvoke(i));
            await foreach (IIssue issue in find)
            {
                Console.WriteLine(issue.Key);
            }
        }
    }
}
