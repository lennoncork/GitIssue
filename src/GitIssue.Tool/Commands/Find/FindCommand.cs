using System;
using System.Threading.Tasks;
using GitIssue.Formatters;
using GitIssue.Issues;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Serilog;

namespace GitIssue.Tool.Commands.Find
{
    /// <summary>
    ///     Fine options
    /// </summary>
    public class FindCommand : Command<FindOptions>
    {
        private readonly ILogger logger;

        private readonly IIssueManager manager;

        public FindCommand(IIssueManager manager, ILogger logger)
        {
            this.manager = manager;
            this.logger = logger;
        }

        /// <inheritdoc />
        public override async Task Exec(FindOptions options)
        {
            Func<IIssue, bool> issueFilter;
            try
            {
                var script = ScriptOptions.Default.AddReferences(typeof(Issue).Assembly);
                issueFilter = await CSharpScript.EvaluateAsync<Func<IIssue, bool>>(options.Linq, script);
            }
            catch (Exception e)
            {
                this.logger.Error($"Unable to parse expression {e.Message}", e);
                return;
            }

            var formatter = new TerminalFormatter(options.Format);
            var find = manager.FindAsync(i => issueFilter.Invoke(i));
            await foreach (var issue in find) Console.WriteLine(issue.Format(formatter));
        }
    }
}