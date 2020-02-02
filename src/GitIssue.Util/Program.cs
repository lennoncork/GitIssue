using System;
using System.Threading.Tasks;
using CommandLine;
using GitIssue.Formatters;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Serilog;

namespace GitIssue.Util
{
    internal class Program
    {
        public static ILogger logger;

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

            parser.ParseArguments<InitOptions, CreateOptions, DeleteOptions, FindOptions>(args)
                .WithParsed<InitOptions>(o => ExecAsync(Init, o).Wait())
                .WithParsed<CreateOptions>(o => ExecAsync(Create, o).Wait())
                .WithParsed<DeleteOptions>(o => ExecAsync(Delete, o).Wait())
                .WithParsed<FindOptions>(o => ExecAsync(Find, o).Wait());
        }

        private static async Task ExecAsync<T>(Func<T, Task> func, T value)
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
            await using var issues = new IssueManager(options.Path, logger);
            await issues.CreateAsync(options.Title, options.Description);
        }

        public static async Task Delete(DeleteOptions options)
        {
            await using var issues = new IssueManager(options.Path, logger);
            await issues.DeleteAsync(options.Key);
        }

        public static async Task Find(FindOptions options)
        {
            await using var issues = new IssueManager(options.Path, logger);
            Func<IIssue, bool> issueFilter;
            try
            {
                //var parameter = Expression.Parameter(typeof(IIssue), options.Name);
                //var expression = DynamicExpression.ParseLambda(new[] {parameter}, typeof(bool), options.Linq);
                //evaluation = expression.Compile();
                var script = ScriptOptions.Default.AddReferences(typeof(Issue).Assembly);
                issueFilter = await CSharpScript.EvaluateAsync<Func<IIssue, bool>>(options.Linq, script);
            }
            catch (Exception e)
            {
                logger.Error($"Unable to parse expression {e.Message}", e);
                return;
            }

            var formatter = new SimpleFormatter();
            var find = issues.FindAsync(i => issueFilter.Invoke(i));
            await foreach (var issue in find) Console.WriteLine(issue.Format(formatter));
        }

        /// <summary>
        ///     Command Line Options
        /// </summary>
        public class Options
        {
            /// <summary>
            ///     The version type to deserialize
            /// </summary>
            [Option("path", Required = false, HelpText = "The working path")]
            public string Path { get; set; } = Environment.CurrentDirectory;
        }

        [Verb(nameof(Command.Init), HelpText = "Initializes the issue repository")]
        public class InitOptions : Options
        {
        }

        [Verb(nameof(Command.Create), HelpText = "Creates a new issue")]
        public class CreateOptions : Options
        {
            [Value(1, HelpText = "The issue title", Required = true)]
            public string Title { get; set; }

            [Value(1, HelpText = "The issue description", Required = false)]
            public string Description { get; set; } = "";
        }

        [Verb(nameof(Command.Delete), HelpText = "Deletes an existing issue")]
        public class DeleteOptions : Options
        {
            [Value(1, HelpText = "The issue key", Required = true)]
            public string Key { get; set; }
        }

        [Verb(nameof(Command.Find), HelpText = "Finds an existing issue")]
        public class FindOptions : Options
        {
            [Option("LinqName", HelpText = "The name of the issue in the linq expression", Required = false)]
            public string Name { get; set; } = "i";

            [Value(1, HelpText = "The LINQ expression to use when matching", Required = false)]
            public string Linq { get; set; } = "i => true";
        }
    }
}