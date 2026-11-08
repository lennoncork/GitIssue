using System;

namespace GitIssue.Services
{
    /// <summary>
    ///     Arguments for <c>gitissue web serve</c>
    /// </summary>
    public sealed class WebServeRequest
    {
        /// <summary>
        ///     Gets a value indicating whether usage was requested
        /// </summary>
        public bool Help { get; init; }

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
        ///     Gets the bind address
        /// </summary>
        public string Host { get; init; } = "127.0.0.1";

        /// <summary>
        ///     Gets the bind port
        /// </summary>
        public int Port { get; init; } = 5080;

        /// <summary>
        ///     Gets a value indicating whether a browser should open
        /// </summary>
        public bool OpenBrowser { get; init; }

        /// <summary>
        ///     Gets the bearer token required for a non-loopback host
        /// </summary>
        public string? Token { get; init; }

        /// <summary>
        ///     Gets a value indicating whether the arguments parsed
        /// </summary>
        public bool IsValid => this.Error is null;

        /// <summary>
        ///     Gets the server URL
        /// </summary>
        public string Url
        {
            get
            {
                string host = this.Host.Contains(':') ? $"[{this.Host.Trim('[', ']')}]" : this.Host;
                return $"http://{host}:{this.Port}";
            }
        }
    }
}
