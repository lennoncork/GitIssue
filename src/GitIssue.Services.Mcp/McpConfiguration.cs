using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using Exceptionable;
using GitIssue.Services;

namespace GitIssue.Services.Mcp
{
    /// <summary>
    ///     Where an MCP configuration file is written
    /// </summary>
    public enum McpInstallScope
    {
        /// <summary>
        ///     The workspace file <c>.vscode/mcp.json</c>
        /// </summary>
        Workspace,

        /// <summary>
        ///     The Visual Studio Code user profile <c>mcp.json</c>
        /// </summary>
        User,
    }

    /// <summary>
    ///     Locates Visual Studio Code MCP configuration files
    /// </summary>
    public static class McpConfigurationPaths
    {
        /// <summary>
        ///     Gets the user-profile MCP configuration path for this operating system
        /// </summary>
        /// <param name="insiders">true to use Visual Studio Code Insiders</param>
        /// <returns>the absolute configuration path</returns>
        public static string UserConfigurationFile(bool insiders)
        {
            string folder = insiders ? "Code - Insiders" : "Code";
            if (OperatingSystem.IsWindows())
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    folder,
                    "User",
                    "mcp.json");
            }

            if (OperatingSystem.IsMacOS())
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "Library",
                    "Application Support",
                    folder,
                    "User",
                    "mcp.json");
            }

            string configHome = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME") ?? string.Empty;
            if (string.IsNullOrEmpty(configHome))
            {
                configHome = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config");
            }

            return Path.Combine(configHome, folder, "User", "mcp.json");
        }

        /// <summary>
        ///     Resolves the configuration file for a request
        /// </summary>
        /// <param name="request">the install request</param>
        /// <returns>the configuration path</returns>
        public static string Resolve(McpInstallRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.ConfigurationFile))
            {
                return request.ConfigurationFile;
            }

            if (request.Scope == McpInstallScope.User)
            {
                return McpConfigurationPaths.UserConfigurationFile(request.Insiders);
            }

            return Path.Combine(request.WorkspaceDirectory, ".vscode", "mcp.json");
        }
    }

    /// <summary>
    ///     Values used to install or remove the gitissue MCP server
    /// </summary>
    public sealed class McpInstallRequest
    {
        /// <summary>
        ///     Gets the configuration scope
        /// </summary>
        public McpInstallScope Scope { get; init; } = McpInstallScope.Workspace;

        /// <summary>
        ///     Gets the workspace directory used for <see cref="McpInstallScope.Workspace" />
        /// </summary>
        public string WorkspaceDirectory { get; init; } = Environment.CurrentDirectory;

        /// <summary>
        ///     Gets a value indicating whether to use the Insiders user profile
        /// </summary>
        public bool Insiders { get; init; }

        /// <summary>
        ///     Gets a value indicating whether to return the JSON without writing it
        /// </summary>
        public bool DryRun { get; init; }

        /// <summary>
        ///     Gets the command Visual Studio Code starts
        /// </summary>
        public string Command { get; init; } = "gitissue";

        /// <summary>
        ///     Gets the issue directory name passed to the server when it is not the default
        /// </summary>
        public string IssueFolderName { get; init; } = Paths.IssueRootFolderName;

        /// <summary>
        ///     Gets arguments that replace the default server arguments when the list is not empty
        /// </summary>
        public IReadOnlyList<string> ServerArguments { get; init; } = new List<string>();

        /// <summary>
        ///     Gets an explicit configuration path. When set, scope is not used to locate the file
        /// </summary>
        public string? ConfigurationFile { get; init; }
    }

    /// <summary>
    ///     The JSON written or previewed by an install or uninstall
    /// </summary>
    public sealed class McpConfigurationEdit
    {
        /// <summary>
        ///     Gets the configuration path
        /// </summary>
        public string Path { get; init; } = string.Empty;

        /// <summary>
        ///     Gets the configuration JSON
        /// </summary>
        public string Json { get; init; } = string.Empty;

        /// <summary>
        ///     Gets a value indicating whether the file existed before the edit
        /// </summary>
        public bool Existed { get; init; }

        /// <summary>
        ///     Gets a value indicating whether the gitissue entry was added or removed
        /// </summary>
        public bool Changed { get; init; }

        /// <summary>
        ///     Gets a value indicating whether the file was written
        /// </summary>
        public bool FileUpdated { get; init; }
    }

    /// <summary>
    ///     Merges the gitissue server into a Visual Studio Code <c>mcp.json</c> file
    /// </summary>
    public sealed class McpConfigurationInstaller
    {
        /// <summary>
        ///     The server name written into <c>mcp.json</c>
        /// </summary>
        public const string ServerName = "gitissue";

        /// <summary>
        ///     Adds or replaces the gitissue server entry
        /// </summary>
        /// <param name="request">the install request</param>
        /// <returns>the resulting configuration</returns>
        [Throws<IssueOperationException>]
        public McpConfigurationEdit Install(McpInstallRequest request)
        {
            string path = McpConfigurationPaths.Resolve(request);
            bool existed = File.Exists(path);
            JsonObject root = McpConfigurationInstaller.readRoot(path);
            JsonObject servers = McpConfigurationInstaller.requireServers(root, path);
            servers[McpConfigurationInstaller.ServerName] = McpConfigurationInstaller.createServer(request);
            return McpConfigurationInstaller.write(path, root, existed, true, request.DryRun);
        }

        /// <summary>
        ///     Removes the gitissue server entry and leaves every other server in place
        /// </summary>
        /// <param name="request">the uninstall request</param>
        /// <returns>the resulting configuration, or null when the file does not exist</returns>
        [Throws<IssueOperationException>]
        public McpConfigurationEdit? Uninstall(McpInstallRequest request)
        {
            string path = McpConfigurationPaths.Resolve(request);
            if (!File.Exists(path))
            {
                return null;
            }

            JsonObject root = McpConfigurationInstaller.readRoot(path);
            bool changed = false;
            if (root["servers"] is JsonObject servers)
            {
                changed = servers.Remove(McpConfigurationInstaller.ServerName);
            }
            else if (root["servers"] is not null)
            {
                throw new IssueOperationException($"Property \"servers\" in {path} must be a JSON object.");
            }

            return McpConfigurationInstaller.write(path, root, true, changed, request.DryRun || !changed);
        }

        private static McpConfigurationEdit write(string path, JsonObject root, bool existed, bool changed, bool dryRun)
        {
            string json = root.ToJsonString(new JsonSerializerOptions
            {
                WriteIndented = true,
            }) + Environment.NewLine;

            bool fileUpdated = changed && !dryRun;
            if (fileUpdated)
            {
                string? directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(path, json);
            }

            return new McpConfigurationEdit
            {
                Path = path,
                Json = json,
                Existed = existed,
                Changed = changed,
                FileUpdated = fileUpdated,
            };
        }

        private static JsonObject createServer(McpInstallRequest request)
        {
            JsonArray args = new JsonArray();
            if (request.ServerArguments.Count > 0)
            {
                foreach (string argument in request.ServerArguments)
                {
                    args.Add(argument);
                }
            }
            else if (string.Equals(request.Command, "dotnet", StringComparison.OrdinalIgnoreCase))
            {
                args.Add("gitissue");
                args.Add("mcp");
                args.Add("serve");
            }
            else
            {
                args.Add("mcp");
                args.Add("serve");
            }

            if ((request.ServerArguments.Count == 0) &&
                !string.Equals(request.IssueFolderName, Paths.IssueRootFolderName, StringComparison.Ordinal))
            {
                args.Add("--name");
                args.Add(request.IssueFolderName);
            }

            return new JsonObject
            {
                ["type"] = "stdio",
                ["command"] = request.Command,
                ["args"] = args,
                ["cwd"] = "${workspaceFolder}",
            };
        }

        private static JsonObject requireServers(JsonObject root, string path)
        {
            if (root["servers"] is JsonObject servers)
            {
                return servers;
            }

            if (root["servers"] is not null)
            {
                throw new IssueOperationException($"Property \"servers\" in {path} must be a JSON object.");
            }

            JsonObject created = new JsonObject();
            root["servers"] = created;
            return created;
        }

        private static JsonObject readRoot(string path)
        {
            if (!File.Exists(path))
            {
                return new JsonObject();
            }

            string text = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(text))
            {
                return new JsonObject();
            }

            JsonNode? node;
            try
            {
                node = JsonNode.Parse(text, documentOptions: new JsonDocumentOptions
                {
                    CommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true,
                });
            }
            catch (JsonException exception)
            {
                throw new IssueOperationException($"Unable to read MCP configuration {path}: {exception.Message}");
            }

            if (node is not JsonObject root)
            {
                throw new IssueOperationException($"MCP configuration {path} must be a JSON object.");
            }

            return root;
        }
    }
}
