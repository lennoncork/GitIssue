using System;
using System.Drawing;
using System.Threading.Tasks;
using Pastel;
using Serilog;

namespace GitIssue.Tool.Commands.Fields
{
    /// <summary>
    ///     Fields command
    /// </summary>
    public class FieldsCommand : Command<FieldsOptions>
    {
        private readonly ILogger logger;

        private readonly IIssueManager manager;

        public FieldsCommand(IIssueManager manager, ILogger logger)
        {
            this.manager = manager;
            this.logger = logger;
        }

        /// <inheritdoc />
        public override Task Exec(FieldsOptions options)
        {
            foreach (var kvp in manager.Configuration.Fields)
            {
                var output = $"{kvp.Key.ToString().Pastel(Color.FromArgb(165, 229, 250))}: A '{kvp.Value.FieldType}' field with '{kvp.Value.ValueType}' values";
                Console.WriteLine(output);
            }
            return Task.CompletedTask;
        }
    }
}