﻿using System;
using System.IO;
using System.Threading.Tasks;
using GitIssue.Formatters;
using GitIssue.Issues;
using GitIssue.Util.Commands.Track;
using Serilog;

namespace GitIssue.Util.Commands.Create
{
    /// <summary>
    ///     Create Command
    /// </summary>
    public class CreateCommand : Command<CreateOptions>
    {
        private Lazy<IIssueFormatter> formatter = new Lazy<IIssueFormatter>(() => new DetailedFormatter());
        
        private readonly ILogger logger;

        private readonly IIssueManager manager;

        public CreateCommand(IIssueManager manager, ILogger logger)
        {
            this.manager = manager;
            this.logger = logger;
        }
        /// <inheritdoc />
        public override async Task Exec(CreateOptions options)
        {
            var issue = await manager.CreateAsync(options.Title, options.Description);
            if (options.Track || options.Tracked == TrackedIssue.None)
            {
                options.Tracked = new TrackedIssue(issue.Key);
                await options.Tracked.SaveAsync(Path.Combine(options.Path, options.Name, options.Tracking), this.logger);
                Console.WriteLine($"Created and tracking new issue '{issue.Key}'");
            }
            else
            {
                Console.WriteLine($"Created new issue '{issue.Key}'");
            }
        }
    }
}