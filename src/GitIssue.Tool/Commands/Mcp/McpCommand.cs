using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GitIssue.Services.Mcp;

namespace GitIssue.Tool.Commands.Mcp
{
    /// <summary>
    ///     The <c>gitissue mcp</c> command
    /// </summary>
    public class McpCommand : Command<McpOptions>
    {
        /// <inheritdoc />
        public override bool WriteSpacing => false;

        /// <inheritdoc />
        public override Task Exec(McpOptions options)
        {
            McpArguments parsed = McpCommand.ToArguments(options);
            this.ExitCode = McpApplication.Execute(parsed, Console.Out, Console.Error);
            return Task.CompletedTask;
        }

        private static McpArguments ToArguments(McpOptions options)
        {
            string actionValue = string.IsNullOrWhiteSpace(options.Action) ? "serve" : options.Action;
            if (!McpCommand.tryParseAction(actionValue, out McpAction action))
            {
                return McpCommand.invalid($"Unknown mcp action \"{actionValue}\".");
            }

            IReadOnlyList<string> serverArguments = (options.ServerArguments ?? new List<string>()).ToList();
            bool scopeSet = options.Scope is not null;
            bool commandSet = options.Command is not null;
            if ((action == McpAction.Serve) && (options.DryRun || options.Insiders || scopeSet || commandSet || (serverArguments.Count > 0)))
            {
                return McpCommand.invalid("serve accepts only --path and --name.");
            }

            if ((action == McpAction.Uninstall) && (commandSet || (serverArguments.Count > 0)))
            {
                return McpCommand.invalid("uninstall does not accept --command or --server-arg.");
            }

            McpInstallScope scope = McpInstallScope.Workspace;
            if (scopeSet && !McpCommand.tryParseScope(options.Scope!, out scope))
            {
                return McpCommand.invalid("--scope must be workspace or user.");
            }

            return new McpArguments
            {
                Action = action,
                RepositoryPath = options.Path,
                IssueFolderName = options.Name,
                Scope = scope,
                DryRun = options.DryRun,
                Insiders = options.Insiders,
                Command = options.Command ?? "gitissue",
                ServerArguments = serverArguments,
            };
        }

        private static McpArguments invalid(string error)
        {
            return new McpArguments
            {
                Error = error,
            };
        }

        private static bool tryParseAction(string value, out McpAction action)
        {
            if (value.Equals("serve", StringComparison.OrdinalIgnoreCase))
            {
                action = McpAction.Serve;
                return true;
            }

            if (value.Equals("install", StringComparison.OrdinalIgnoreCase))
            {
                action = McpAction.Install;
                return true;
            }

            if (value.Equals("uninstall", StringComparison.OrdinalIgnoreCase))
            {
                action = McpAction.Uninstall;
                return true;
            }

            if (value.Equals("help", StringComparison.OrdinalIgnoreCase))
            {
                action = McpAction.Help;
                return true;
            }

            action = McpAction.Serve;
            return false;
        }

        private static bool tryParseScope(string value, out McpInstallScope scope)
        {
            if (value.Equals("workspace", StringComparison.OrdinalIgnoreCase))
            {
                scope = McpInstallScope.Workspace;
                return true;
            }

            if (value.Equals("user", StringComparison.OrdinalIgnoreCase))
            {
                scope = McpInstallScope.User;
                return true;
            }

            scope = McpInstallScope.Workspace;
            return false;
        }
    }
}
