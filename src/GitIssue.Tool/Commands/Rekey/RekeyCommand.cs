using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GitIssue.Issues;

namespace GitIssue.Tool.Commands.Rekey
{
    /// <summary>
    ///     Rekey command
    /// </summary>
    public class RekeyCommand : Command<RekeyOptions>
    {
        private readonly IIssueManager manager;

        public RekeyCommand(IIssueManager manager)
        {
            this.manager = manager;
        }

        /// <inheritdoc />
        public override async Task Exec(RekeyOptions options)
        {
            IReadOnlyList<IssueRekeyResult> results = await this.manager.RekeyAsync(options.DryRun);
            if (results.Count == 0)
            {
                Console.WriteLine("Issue keys already match the configured template.");
                return;
            }

            foreach (IssueRekeyResult result in results)
            {
                Console.WriteLine($"{result.From} -> {result.To}");
            }

            string verb = options.DryRun ? "Would rename" : "Renamed";
            Console.WriteLine($"{verb} {results.Count} {(results.Count == 1 ? "issue" : "issues")}.");
        }
    }
}
