using System;
using System.Threading.Tasks;
using Serilog;

namespace GitIssue.Tool.Commands.Init
{
    /// <summary>
    ///     Init command
    /// </summary>
    public class InitCommand : Command<InitOptions>
    {
        public delegate void Initializer();

        private readonly ILogger logger;

        private readonly Func<IIssueManager> factory;

        private readonly Action onInit;

        public InitCommand(ILogger logger, Func<IIssueManager> factory, Initializer initializer)
        {
            this.onInit = () => initializer.Invoke();
            this.factory = factory;
            this.logger = logger;
        }

        /// <inheritdoc />
        public override Task Exec(InitOptions options)
        {
            this.onInit();
            var manager = factory.Invoke();
            Console.WriteLine($"Initialized empty 'Issue' repository in {manager.Root.IssuesPath}");
            return Task.CompletedTask;
        }
    }
}