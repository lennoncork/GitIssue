using System;
using System.Collections.Generic;

namespace GitIssue.Services
{
    /// <summary>
    ///     Parses <c>gitissue web serve</c> arguments
    /// </summary>
    public static class WebServeCommandLine
    {
        /// <summary>
        ///     Usage text for the web command
        /// </summary>
        public const string Usage =
            """
            Usage:
              gitissue web serve [--host <address>] [--port <port>] [--open] [--token <token>] [--path <directory>] [--name <folder>]

            serve runs the local issue website. The default address is http://127.0.0.1:5080.
            A host other than loopback requires --token.
            """;

        /// <summary>
        ///     Parses arguments that start with <c>web</c>
        /// </summary>
        /// <param name="args">the process arguments</param>
        /// <returns>the parsed request</returns>
        public static WebServeRequest ParseCli(IReadOnlyList<string> args)
        {
            if ((args.Count == 0) || !args[0].Equals("web", StringComparison.OrdinalIgnoreCase))
            {
                return WebServeCommandLine.invalid("The web command is missing.");
            }

            if (WebServeCommandLine.containsHelp(args))
            {
                return new WebServeRequest
                {
                    Help = true,
                };
            }

            int cursor = 1;
            if ((cursor < args.Count) && !args[cursor].StartsWith('-'))
            {
                if (args[cursor].Equals("help", StringComparison.OrdinalIgnoreCase))
                {
                    return new WebServeRequest
                    {
                        Help = true,
                    };
                }

                if (!args[cursor].Equals("serve", StringComparison.OrdinalIgnoreCase))
                {
                    return WebServeCommandLine.invalid($"Unknown web action \"{args[cursor]}\".");
                }

                cursor++;
            }

            return WebServeCommandLine.parseOptions(args, cursor);
        }

        /// <summary>
        ///     Parses the arguments forwarded to the web host
        /// </summary>
        /// <param name="args">the host arguments</param>
        /// <returns>the parsed request</returns>
        public static WebServeRequest ParseHost(IReadOnlyList<string> args)
        {
            if (WebServeCommandLine.containsHelp(args))
            {
                return new WebServeRequest
                {
                    Help = true,
                };
            }

            return WebServeCommandLine.parseOptions(args, 0);
        }

        private static WebServeRequest parseOptions(IReadOnlyList<string> args, int cursor)
        {
            string repositoryPath = Environment.CurrentDirectory;
            string issueFolderName = Paths.IssueRootFolderName;
            string host = "127.0.0.1";
            int port = 5080;
            bool openBrowser = false;
            string? token = null;

            while (cursor < args.Count)
            {
                string option = args[cursor];
                cursor++;
                switch (option.ToLowerInvariant())
                {
                    case "--path":
                        if (!WebServeCommandLine.tryTake(args, ref cursor, out repositoryPath))
                        {
                            return WebServeCommandLine.invalid("--path requires a directory.");
                        }

                        break;
                    case "--name":
                        if (!WebServeCommandLine.tryTake(args, ref cursor, out issueFolderName))
                        {
                            return WebServeCommandLine.invalid("--name requires a folder name.");
                        }

                        break;
                    case "--host":
                        if (!WebServeCommandLine.tryTake(args, ref cursor, out host))
                        {
                            return WebServeCommandLine.invalid("--host requires an address.");
                        }

                        break;
                    case "--port":
                        if (!WebServeCommandLine.tryTake(args, ref cursor, out string portValue) ||
                            !int.TryParse(portValue, out port) ||
                            (port < 1) ||
                            (port > 65535))
                        {
                            return WebServeCommandLine.invalid("--port must be from 1 to 65535.");
                        }

                        break;
                    case "--token":
                        if (!WebServeCommandLine.tryTake(args, ref cursor, out string tokenValue))
                        {
                            return WebServeCommandLine.invalid("--token requires a value.");
                        }

                        token = tokenValue;
                        break;
                    case "--open":
                        openBrowser = true;
                        break;
                    default:
                        return WebServeCommandLine.invalid($"Unknown option \"{option}\".");
                }
            }

            if (!WebServeCommandLine.isLoopback(host) && string.IsNullOrEmpty(token))
            {
                return WebServeCommandLine.invalid("--token is required when --host is not loopback.");
            }

            return new WebServeRequest
            {
                RepositoryPath = repositoryPath,
                IssueFolderName = issueFolderName,
                Host = host,
                Port = port,
                OpenBrowser = openBrowser,
                Token = token,
            };
        }

        private static bool isLoopback(string host)
        {
            return host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
                host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase) ||
                host.Equals("::1", StringComparison.OrdinalIgnoreCase) ||
                host.Equals("[::1]", StringComparison.OrdinalIgnoreCase);
        }

        private static bool containsHelp(IReadOnlyList<string> args)
        {
            for (int index = 0; index < args.Count; index++)
            {
                if (args[index].Equals("--help", StringComparison.OrdinalIgnoreCase) ||
                    args[index].Equals("-h", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

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

        private static WebServeRequest invalid(string error)
        {
            return new WebServeRequest
            {
                Error = error,
            };
        }
    }
}
