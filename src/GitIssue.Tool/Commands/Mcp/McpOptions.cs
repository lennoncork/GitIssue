using System.Collections.Generic;
using CommandLine;

namespace GitIssue.Tool.Commands.Mcp
{
#pragma warning disable 1591
    [Verb("mcp", HelpText = "Expose repository issues to an MCP client.\n  gitissue mcp serve\n  gitissue mcp install\n  gitissue mcp uninstall")]
    public class McpOptions : Options
    {
        [Value(0, MetaName = "action", Required = false, HelpText = "serve, install, uninstall, or help. Defaults to serve.")]
        public string Action { get; set; } = "serve";

        [Option("command", Required = false, HelpText = "Server command. Defaults to gitissue.")]
        public string? Command { get; set; }

        [Option("dry-run", Required = false, HelpText = "Preview the configuration change.")]
        public bool DryRun { get; set; }

        [Option("insiders", Required = false, HelpText = "Use the Visual Studio Code Insiders user profile.")]
        public bool Insiders { get; set; }

        [Option("scope", Required = false, HelpText = "workspace or user. Defaults to workspace.")]
        public string? Scope { get; set; }

        [Option("server-arg", Required = false, HelpText = "Replacement argument passed to the server command. Repeat the option for each argument.")]
        public IEnumerable<string> ServerArguments { get; set; } = new List<string>();
    }
#pragma warning restore 1591
}
