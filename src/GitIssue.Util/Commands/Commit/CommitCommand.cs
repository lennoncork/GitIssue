﻿using System.Threading.Tasks;
using Serilog;

namespace GitIssue.Util
{
    /// <summary>
    /// Commit command
    /// </summary>
    public class CommitCommand : Command<CommitOptions>
    {
        private static ILogger Logger => Program.Logger;

        private static IIssueManager Initialize(Options options) => Program.Initialize(options);

        /// <inheritdoc />
        public override async Task Exec(CommitOptions options)
        {
            await using var issues = Initialize(options);
            await issues.CommitAsync();
        }
        
    }
}