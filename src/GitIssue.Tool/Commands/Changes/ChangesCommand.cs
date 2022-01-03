using System;
using System.Threading.Tasks;
using GitIssue.Formatters;
using Serilog;

namespace GitIssue.Tool.Commands.Changes
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