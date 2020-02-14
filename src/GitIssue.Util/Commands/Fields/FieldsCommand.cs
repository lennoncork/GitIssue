using System;
using System.Threading.Tasks;
using Serilog;

namespace GitIssue.Util
{
    /// <summary>
    /// Fields command
    /// </summary>
    public class FieldsCommand: Command<FieldsOptions>
    {
        private static ILogger Logger => Program.Logger;

        private static IIssueManager Initialize(Options options) => Program.Initialize(options);

        /// <inheritdoc />
        public override async Task Exec(FieldsOptions options)
        {
            await using var issues = Initialize(options);
            foreach (var kvp in issues.Configuration.Fields)
            {
                var output = $"{kvp.Key}: A '{kvp.Value.FieldType}' field '{kvp.Value.ValueType}' values";
                Console.WriteLine(output);
            }
        }
    }
}