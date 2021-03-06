﻿using System;
using System.Threading.Tasks;
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
                var output = $"{kvp.Key}: A '{kvp.Value.FieldType}' field '{kvp.Value.ValueType}' values";
                Console.WriteLine(output);
            }
            return Task.CompletedTask;
        }
    }
}