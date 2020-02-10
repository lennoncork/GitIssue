using System;
using System.Threading.Tasks;
using CommandLine;
using GitIssue.Editors;
using GitIssue.Formatters;
using GitIssue.Keys;
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
            var parser = new Parser(with =>
            {
                with.EnableDashDash = true;
                with.AutoHelp = true;
                with.CaseSensitive = false;
                with.HelpWriter = Console.Error;
            });

            parser.ParseArguments<InitOptions, CreateOptions, DeleteOptions, FindOptions,
                    ShowOptions, EditOptions>(args)
                .WithParsed<InitOptions>(o => ExecAsync(Init, o).Wait())
                .WithParsed<CreateOptions>(o => ExecAsync(Create, o).Wait())
                .WithParsed<DeleteOptions>(o => ExecAsync(Delete, o).Wait())
                .WithParsed<FindOptions>(o => ExecAsync(Find, o).Wait())
                .WithParsed<ShowOptions>(o => ExecAsync(Show, o).Wait())
                .WithParsed<EditOptions>(o => ExecAsync(Edit, o).Wait());
        }

        public static IIssueManager Initialize(Options options)
        {
            logger = new LoggerConfiguration()
                .WriteTo.Console()
                .MinimumLevel.Debug()
                .CreateLogger();

            return new IssueManager(options.Path, options.Type, logger);
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

        public static async Task Init(InitOptions options)
        {
            await using var issues = Initialize(options);
        }

        public static async Task Create(CreateOptions options)
        {
            await using var issues = Initialize(options);
            await issues.CreateAsync(options.Title, options.Description);
        }

        public static async Task Delete(DeleteOptions options)
        {
            await using var issues = new IssueManager(options.Path, logger);
            await issues.DeleteAsync(options.Key);
        }

        public static async Task Find(FindOptions options)
        {
            await using var issues = Initialize(options);
            Func<IIssue, bool> issueFilter;
            try
            {
                var script = ScriptOptions.Default.AddReferences(typeof(Issue).Assembly);
                issueFilter = await CSharpScript.EvaluateAsync<Func<IIssue, bool>>(options.Linq, script);
            }
            catch (Exception e)
            {
                logger.Error($"Unable to parse expression {e.Message}", e);
                return;
            }

            var formatter = new SimpleFormatter(options.Format);
            var find = issues.FindAsync(i => issueFilter.Invoke(i));
            await foreach (var issue in find) Console.WriteLine(issue.Format(formatter));
        }

        /// <summary>
        ///     Shows the issue details
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static async Task Show(ShowOptions options)
        {
            var formatter = new DetailedFormatter();
            await using var issues = Initialize(options);
            var find = issues.FindAsync(i => i.Key.ToString() == options.Key);
            await foreach (var issue in find)
            {
                Console.WriteLine(issue.Format(formatter));
                break;
            }
        }

        /// <summary>
        ///     Edits an existing issue
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static async Task Edit(EditOptions options)
        {
            var formatter = new DetailedFormatter();
            await using var issues = Initialize(options);
            var find = issues.FindAsync(i => i.Key.ToString() == options.Key);

            IIssue issue = null;
            var updated = false;
            await foreach (var found in find)
            {
                issue = found;
                break;
            }

            if (issue == null)
            {
                logger.Error($"Issue \"{options.Key}\" not found");
                return;
            }

            if (string.IsNullOrEmpty(options.Field))
            {
                var editor = new Editor();
                await editor.Open(issue);
                updated = true;
            }

            if (updated)
            {
                issue.Updated = DateTime.Now;
                if (await issue.SaveAsync()) Console.WriteLine(issue.Format(formatter));
                return;
            }

            var key = FieldKey.Create(options.Field);
            if (!issue.ContainsKey(key))
            {
                logger.Error($"Field \"{key}\" does not exist on issue \"{issue.Key}\"");
                return;
            }

            if (string.IsNullOrEmpty(options.Update))
            {
                var editor = new Editor();
                await editor.Open(issue[key]);
                updated = true;
            }
            else if (await issue[key].UpdateAsync(options.Update))
            {
                updated = true;
            }

            if (updated)
            {
                issue.Updated = DateTime.Now;
                if (await issue.SaveAsync()) Console.WriteLine(issue.Format(formatter));
            }
        }
    }
}