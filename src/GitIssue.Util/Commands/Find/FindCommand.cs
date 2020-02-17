using System;
using System.Threading.Tasks;
using GitIssue.Formatters;
using GitIssue.Issues;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Serilog;

namespace GitIssue.Util.Commands.Find
{
    /// <summary>
    ///     Fine options
    /// </summary>
    public class FindCommand : Command<FindOptions>
    {
        private static ILogger? Logger => Program.Logger;

        private static IIssueManager Initialize(Options options)
        {
            return Program.Initialize(options);
        }

        /// <inheritdoc />
        public override async Task Exec(FindOptions options)
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
                Logger?.Error($"Unable to parse expression {e.Message}", e);
                return;
            }

            var formatter = new SimpleFormatter(options.Format);
            var find = issues.FindAsync(i => issueFilter.Invoke(i));
            await foreach (var issue in find) Console.WriteLine(issue.Format(formatter));
        }
    }
}