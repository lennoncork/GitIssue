using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using GitIssue.Fields;
using GitIssue.Formatters;
using GitIssue.Issues;
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
                    ShowOptions, EditOptions, TrackOptions, FieldsOptions>(args)
                .WithParsed<InitOptions>(o => ExecAsync(Init, o).Wait())
                .WithParsed<CreateOptions>(o => ExecAsync(Create, o).Wait())
                .WithParsed<DeleteOptions>(o => ExecAsync(Delete, o).Wait())
                .WithParsed<FindOptions>(o => ExecAsync(Find, o).Wait())
                .WithParsed<ShowOptions>(o => ExecAsync(Show, o).Wait())
                .WithParsed<EditOptions>(o => ExecAsync(Edit, o).Wait())
                .WithParsed<FieldsOptions>(o => ExecAsync(Fields, o).Wait())
                .WithParsed<TrackOptions>(o => ExecAsync(Track, o).Wait());
        }

        public static IIssueManager Initialize(Options options)
        {
            logger = new LoggerConfiguration()
                .WriteTo.Console()
                .MinimumLevel.Debug()
                .CreateLogger();

            if (options is KeyOptions keyOptions)
                keyOptions.Tracked = TrackedIssue
                    .Read(Path.Combine(options.Path, options.Name, options.Tracking), logger);

            //Configuration configuration = Configuration.Read();

            return new IssueManager(options.Path, options.Name, logger);
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
            await using var issues = IssueManager.Init(options.Path, options.Name);
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

        public static async Task Track(TrackOptions options)
        {
            await using var issues = Initialize(options);
            options.Tracked = TrackedIssue.None;
            if (!string.IsNullOrEmpty(options.Key))
            {
                var find = await issues.FindAsync(i => i.Key.ToString() == options.Key)
                    .FirstOrDefaultAsync();

                if (find != null)
                {
                    options.Tracked.Key = find.Key;
                    options.Tracked.Started = DateTime.Now;
                }
            }

            options.Tracked.Save(Path.Combine(options.Path, options.Name, options.Tracking), logger);
            Console.WriteLine($"Tacking Issue '{options.Tracked.Key}'");
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
            var issue = await issues
                .FindAsync(i => i.Key.ToString() == options.Key)
                .FirstOrDefaultAsync();
            Console.WriteLine(issue?.Format(formatter));
        }

        public static async Task Fields(FieldsOptions options)
        {
            await using var issues = Initialize(options);
            foreach (var kvp in issues.Configuration.Fields)
            {
                var output = $"{kvp.Key}: A '{kvp.Value.FieldType}' field '{kvp.Value.ValueType}' values";
                Console.WriteLine(output);
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
            var issue = await issues
                .FindAsync(i => i.Key.ToString() == options.Key)
                .FirstOrDefaultAsync();

            if (issue == null)
            {
                logger.Error($"Issue \"{options.Key}\" not found");
                return;
            }

            var updated = false;
            if (string.IsNullOrEmpty(options.Field))
            {
                var editor = new Editor
                {
                    Command = options.Editor,
                    Arguments = options.Arguments
                };
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
                var editor = new Editor
                {
                    Command = options.Editor,
                    Arguments = options.Arguments
                };
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