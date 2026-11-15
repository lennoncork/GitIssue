using CommandLine;

namespace GitIssue.Tool.Commands.Web
{
#pragma warning disable 1591
    [Verb("web", HelpText = "Run the local issue website.\n  gitissue web serve [--host <address>] [--port <port>] [--open] [--token <token>]")]
    public class WebOptions : Options
    {
        [Value(0, MetaName = "action", Required = false, HelpText = "serve or help. Defaults to serve.")]
        public string Action { get; set; } = "serve";

        [Option("host", Required = false, HelpText = "Bind address. Defaults to 127.0.0.1.")]
        public string Host { get; set; } = "127.0.0.1";

        [Option("open", Required = false, HelpText = "Open a browser after the server starts.")]
        public bool Open { get; set; }

        [Option("port", Required = false, HelpText = "Bind port from 1 to 65535. Defaults to 5080.")]
        public int Port { get; set; } = 5080;

        [Option("token", Required = false, HelpText = "Bearer token. Required when the host is not loopback.")]
        public string? Token { get; set; }
    }
#pragma warning restore 1591
}
