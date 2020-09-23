using System;
using System.IO;
using System.Threading.Tasks;
using GitIssue.Formatters;
using GitIssue.Issues;
using GitIssue.Tool.Commands.Track;
using Serilog;

namespace GitIssue.Tool.Commands.Create
{
    /// <summary>
    ///     Create Command
    /// </summary>
    public class ChangesCommand : Command<ChangesOptions>
    {
        private readonly Lazy<IIssueFormatter> formatter = new Lazy<IIssueFormatter>(() => IssueFormatter.Detailed);
        
        private readonly ILogger logger;

        private readonly IIssueManager manager;

        public ChangesCommand(IIssueManager manager, ILogger logger)
        {
            this.manager = manager;
            this.logger = logger;
        }

        /// <inheritdoc />
        public override Task Exec(ChangesOptions options)
        {
            Console.Write(manager.Changes.GenerateComments());
            return Task.CompletedTask;
        }
    }
}