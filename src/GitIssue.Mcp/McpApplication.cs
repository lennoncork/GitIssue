using System;
using System.Collections.Generic;
using System.IO;
using Exceptionable;

namespace GitIssue.Mcp
{
    /// <summary>
    ///     An mcp subcommand
    /// </summary>
    public enum McpAction
    {
        /// <summary>
        ///     Run the stdio server
        /// </summary>
        Serve,

        /// <summary>
        ///     Write the Visual Studio Code MCP configuration
        /// </summary>
        Install,

        /// <summary>
        ///     Remove the Visual Studio Code MCP configuration entry
        /// </summary>
        Uninstall,

        /// <summary>
        ///     Print usage
        /// </summary>
        Help,
    }

    /// <summary>
    ///     Parses <c>gitissue mcp</c> arguments
    /// </summary>
    public static class McpCommandLine
    {
        /// <summary>
        ///     Usage text for the mcp command
        /// </summary>
        public const string Usage =
            """
            Usage:
              gitissue mcp serve [--path <directory>] [--name <folder>]
              gitissue mcp install [--scope workspace|user] [--dry-run] [--insiders] [--command <command>] [--server-arg <arg>] [--path <directory>] [--name <folder>]
              gitissue mcp uninstall [--scope workspace|user] [--dry-run] [--insiders] [--path <directory>]

            serve runs the MCP server on standard input and output. Logs are written to standard error.
            install writes a Visual Studio Code Copilot server entry. The default scope is the workspace file .vscode/mcp.json.
            uninstall removes that entry and leaves other servers in place.
            Rewriting mcp.json drops comments in that file.
            """;

        /// <summary>
        ///     Parses mcp arguments. The first argument must be <c>mcp</c>
        /// </summary>
        /// <param name="args">the process arguments</param>
        /// <returns>the parsed arguments</returns>
        public static McpArguments Parse(IReadOnlyList<string> args)
        {
            if ((args.Count == 0) || !args[0].Equals("mcp", StringComparison.OrdinalIgnoreCase))
            {
                return McpCommandLine.invalid("The mcp command is missing.");
            }

            for (int index = 1; index < args.Count; index++)
            {
                if (McpCommandLine.isHelpFlag(args[index]))
                {
                    return new McpArguments
                    {
                        Action = McpAction.Help,
                    };
                }
            }

            int cursor = 1;
            McpAction action = McpAction.Serve;
            if ((cursor < args.Count) && !args[cursor].StartsWith('-'))
            {
                if (!McpCommandLine.tryParseAction(args[cursor], out action))
                {
                    return McpCommandLine.invalid($"Unknown mcp action \"{args[cursor]}\".");
                }

                cursor++;
            }

            string repositoryPath = Environment.CurrentDirectory;
            string issueFolderName = Paths.IssueRootFolderName;
            McpInstallScope scope = McpInstallScope.Workspace;
            bool dryRun = false;
            bool insiders = false;
            string command = "gitissue";
            List<string> serverArguments = new List<string>();
            bool scopeSet = false;
            bool commandSet = false;

            while (cursor < args.Count)
            {
                string token = args[cursor];
                cursor++;
                switch (token.ToLowerInvariant())
                {
                    case "--path":
                        if (!McpCommandLine.tryTake(args, ref cursor, out string path))
                        {
                            return McpCommandLine.invalid("--path requires a directory.");
                        }

                        repositoryPath = path;
                        break;
                    case "--name":
                        if (!McpCommandLine.tryTake(args, ref cursor, out string name))
                        {
                            return McpCommandLine.invalid("--name requires a folder name.");
                        }

                        issueFolderName = name;
                        break;
                    case "--scope":
                        if (!McpCommandLine.tryTake(args, ref cursor, out string scopeValue) ||
                            !McpCommandLine.tryParseScope(scopeValue, out scope))
                        {
                            return McpCommandLine.invalid("--scope must be workspace or user.");
                        }

                        scopeSet = true;
                        break;
                    case "--command":
                        if (!McpCommandLine.tryTake(args, ref cursor, out command))
                        {
                            return McpCommandLine.invalid("--command requires a command name.");
                        }

                        commandSet = true;
                        break;
                    case "--server-arg":
                        if (!McpCommandLine.tryTake(args, ref cursor, out string serverArgument))
                        {
                            return McpCommandLine.invalid("--server-arg requires an argument.");
                        }

                        serverArguments.Add(serverArgument);
                        break;
                    case "--dry-run":
                        dryRun = true;
                        break;
                    case "--insiders":
                        insiders = true;
                        break;
                    default:
                        return McpCommandLine.invalid($"Unknown option \"{token}\".");
                }
            }

            if ((action == McpAction.Serve) && (dryRun || insiders || scopeSet || commandSet || (serverArguments.Count > 0)))
            {
                return McpCommandLine.invalid("serve accepts only --path and --name.");
            }

            if ((action == McpAction.Uninstall) && (commandSet || (serverArguments.Count > 0)))
            {
                return McpCommandLine.invalid("uninstall does not accept --command or --server-arg.");
            }

            return new McpArguments
            {
                Action = action,
                RepositoryPath = repositoryPath,
                IssueFolderName = issueFolderName,
                Scope = scope,
                DryRun = dryRun,
                Insiders = insiders,
                Command = command,
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

        private static bool tryTake(IReadOnlyList<string> args, ref int cursor, out string value)
        {
            if ((cursor >= args.Count) || args[cursor].StartsWith('-'))
            {
                value = string.Empty;
                return false;
            }

            value = args[cursor];
            cursor++;
            return true;
        }

        private static bool isHelpFlag(string value)
        {
            return value.Equals("--help", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("-h", StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    ///     Runs the mcp command
    /// </summary>
    public static class McpApplication
    {
        /// <summary>
        ///     Executes mcp arguments
        /// </summary>
        /// <param name="args">the process arguments, including the leading mcp token</param>
        /// <param name="output">the standard output writer</param>
        /// <param name="error">the standard error writer</param>
        /// <returns>the process exit code</returns>
        [Throws<ArgumentNullException>]
        public static int Execute(string[] args, TextWriter output, TextWriter error)
        {
            McpArguments parsed = McpCommandLine.Parse(args);
            if (!parsed.IsValid)
            {
                error.WriteLine(parsed.Error);
                error.WriteLine(McpCommandLine.Usage);
                return 1;
            }

            if (parsed.Action == McpAction.Help)
            {
                output.WriteLine(McpCommandLine.Usage);
                return 0;
            }

            try
            {
                if (parsed.Action == McpAction.Serve)
                {
                    McpServerHost.Run(new McpServeOptions(parsed.RepositoryPath, parsed.IssueFolderName));
                    return 0;
                }

                McpConfigurationInstaller installer = new McpConfigurationInstaller();
                McpInstallRequest request = parsed.ToInstallRequest();
                if (parsed.Action == McpAction.Install)
                {
                    McpConfigurationEdit edit = installer.Install(request);
                    if (parsed.DryRun)
                    {
                        output.Write(edit.Json);
                    }
                    else
                    {
                        output.WriteLine($"Installed gitissue MCP server in {edit.Path}");
                    }

                    return 0;
                }

                McpConfigurationEdit? removed = installer.Uninstall(request);
                if (removed is null)
                {
                    output.WriteLine($"No MCP configuration at {McpConfigurationPaths.Resolve(request)}");
                    return 0;
                }

                if (parsed.DryRun)
                {
                    output.Write(removed.Json);
                    return 0;
                }

                if (removed.Changed)
                {
                    output.WriteLine($"Removed gitissue MCP server from {removed.Path}");
                }
                else
                {
                    output.WriteLine($"gitissue MCP server was not installed in {removed.Path}");
                }

                return 0;
            }
            catch (Exception exception)
            {
                error.WriteLine(exception.Message);
                return 1;
            }
        }
    }

    /// <summary>
    ///     Parsed <c>gitissue mcp</c> arguments
    /// </summary>
    public sealed class McpArguments
    {
        /// <summary>
        ///     Gets the selected action
        /// </summary>
        public McpAction Action { get; init; } = McpAction.Serve;

        /// <summary>
        ///     Gets the parse error, when parsing failed
        /// </summary>
        public string? Error { get; init; }

        /// <summary>
        ///     Gets the working directory
        /// </summary>
        public string RepositoryPath { get; init; } = Environment.CurrentDirectory;

        /// <summary>
        ///     Gets the issue directory name
        /// </summary>
        public string IssueFolderName { get; init; } = Paths.IssueRootFolderName;

        /// <summary>
        ///     Gets the configuration scope
        /// </summary>
        public McpInstallScope Scope { get; init; } = McpInstallScope.Workspace;

        /// <summary>
        ///     Gets a value indicating whether the configuration is previewed
        /// </summary>
        public bool DryRun { get; init; }

        /// <summary>
        ///     Gets a value indicating whether the Insiders user profile is used
        /// </summary>
        public bool Insiders { get; init; }

        /// <summary>
        ///     Gets the server command
        /// </summary>
        public string Command { get; init; } = "gitissue";

        /// <summary>
        ///     Gets replacement server arguments
        /// </summary>
        public IReadOnlyList<string> ServerArguments { get; init; } = new List<string>();

        /// <summary>
        ///     Gets a value indicating whether the arguments parsed
        /// </summary>
        public bool IsValid => this.Error is null;

        /// <summary>
        ///     Creates the install request described by these arguments
        /// </summary>
        /// <returns>the install request</returns>
        public McpInstallRequest ToInstallRequest()
        {
            return new McpInstallRequest
            {
                Scope = this.Scope,
                WorkspaceDirectory = this.RepositoryPath,
                Insiders = this.Insiders,
                DryRun = this.DryRun,
                Command = this.Command,
                IssueFolderName = this.IssueFolderName,
                ServerArguments = this.ServerArguments,
            };
        }
    }
}
