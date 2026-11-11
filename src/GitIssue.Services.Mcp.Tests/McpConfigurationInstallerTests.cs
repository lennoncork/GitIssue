using System;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using GitIssue.Services;
using NUnit.Framework;

namespace GitIssue.Services.Mcp.Tests
{
    [TestFixture]
    public class McpConfigurationInstallerTests
    {
        [Test]
        public void Install_PreservesOtherServersAndUninstallRemovesOnlyGitIssue()
        {
            using TempDirectory directory = new TempDirectory();
            string path = Path.Combine(directory.Path, "mcp.json");
            File.WriteAllText(path, """
                {
                  "inputs": [ { "id": "token", "type": "promptString" } ],
                  "servers": {
                    "other": { "type": "stdio", "command": "other" }
                  }
                }
                """);

            McpConfigurationInstaller installer = new McpConfigurationInstaller();
            McpInstallRequest request = new McpInstallRequest
            {
                ConfigurationFile = path,
            };

            McpConfigurationEdit installed = installer.Install(request);
            JsonObject installedJson = JsonNode.Parse(installed.Json)!.AsObject();
            JsonObject servers = installedJson["servers"]!.AsObject();

            Assert.Multiple(() =>
            {
                Assert.That(installed.FileUpdated, Is.True);
                Assert.That(servers.ContainsKey("other"), Is.True);
                Assert.That(servers.ContainsKey("gitissue"), Is.True);
                Assert.That(installedJson.ContainsKey("inputs"), Is.True);
                Assert.That(servers["gitissue"]!["command"]!.GetValue<string>(), Is.EqualTo("gitissue"));
                Assert.That(servers["gitissue"]!["args"]![0]!.GetValue<string>(), Is.EqualTo("mcp"));
                Assert.That(servers["gitissue"]!["cwd"]!.GetValue<string>(), Is.EqualTo("${workspaceFolder}"));
            });

            McpConfigurationEdit removed = installer.Uninstall(request)!;
            JsonObject removedServers = JsonNode.Parse(removed.Json)!["servers"]!.AsObject();
            Assert.Multiple(() =>
            {
                Assert.That(removed.Changed, Is.True);
                Assert.That(removedServers.ContainsKey("gitissue"), Is.False);
                Assert.That(removedServers.ContainsKey("other"), Is.True);
            });
        }

        [Test]
        public void Install_DryRun_DoesNotWrite()
        {
            using TempDirectory directory = new TempDirectory();
            string path = Path.Combine(directory.Path, "mcp.json");
            McpConfigurationInstaller installer = new McpConfigurationInstaller();

            McpConfigurationEdit edit = installer.Install(new McpInstallRequest
            {
                ConfigurationFile = path,
                DryRun = true,
                Command = "dotnet",
                IssueFolderName = "work-items",
            });

            JsonObject server = JsonNode.Parse(edit.Json)!["servers"]!["gitissue"]!.AsObject();
            Assert.Multiple(() =>
            {
                Assert.That(edit.FileUpdated, Is.False);
                Assert.That(File.Exists(path), Is.False);
                Assert.That(server["command"]!.GetValue<string>(), Is.EqualTo("dotnet"));
                Assert.That(server["args"]![0]!.GetValue<string>(), Is.EqualTo("gitissue"));
                Assert.That(server["args"]!.AsArray().Select(item => item!.GetValue<string>()), Does.Contain("--name"));
                Assert.That(server["args"]!.AsArray().Select(item => item!.GetValue<string>()), Does.Contain("work-items"));
            });
        }

        [Test]
        public void Install_InvalidJson_Throws()
        {
            using TempDirectory directory = new TempDirectory();
            string path = Path.Combine(directory.Path, "mcp.json");
            File.WriteAllText(path, "{");

            McpConfigurationInstaller installer = new McpConfigurationInstaller();
            Assert.That(
                () => installer.Install(new McpInstallRequest { ConfigurationFile = path }),
                Throws.TypeOf<IssueOperationException>());
        }

        [Test]
        public void Install_EmptyFileAndInvalidServers_AreReported()
        {
            using TempDirectory directory = new TempDirectory();
            string path = Path.Combine(directory.Path, "mcp.json");
            File.WriteAllText(path, "   ");
            McpConfigurationInstaller installer = new McpConfigurationInstaller();
            McpConfigurationEdit installed = installer.Install(new McpInstallRequest
            {
                ConfigurationFile = path,
            });
            string written = File.ReadAllText(path);
            File.WriteAllText(path, """{ "servers": [] }""");
            Assert.Multiple(() =>
            {
                Assert.That(installed.Existed, Is.True);
                Assert.That(written, Does.Contain("gitissue"));
                Assert.That(
                    () => installer.Install(new McpInstallRequest { ConfigurationFile = path }),
                    Throws.TypeOf<IssueOperationException>());
                Assert.That(installer.Uninstall(new McpInstallRequest
                {
                    ConfigurationFile = Path.Combine(directory.Path, "absent.json"),
                }), Is.Null);
            });
        }

        [Test]
        public void Resolve_UsesWorkspaceUserAndExplicitPaths()
        {
            using TempDirectory directory = new TempDirectory();
            string explicitPath = Path.Combine(directory.Path, "custom.json");
            string? previous = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
            try
            {
                if (OperatingSystem.IsLinux())
                {
                    Environment.SetEnvironmentVariable("XDG_CONFIG_HOME", directory.Path);
                }

                string userPath = McpConfigurationPaths.Resolve(new McpInstallRequest
                {
                    Scope = McpInstallScope.User,
                });
                string workspacePath = McpConfigurationPaths.Resolve(new McpInstallRequest
                {
                    Scope = McpInstallScope.Workspace,
                    WorkspaceDirectory = directory.Path,
                });

                Assert.Multiple(() =>
                {
                    Assert.That(
                        McpConfigurationPaths.Resolve(new McpInstallRequest { ConfigurationFile = explicitPath }),
                        Is.EqualTo(explicitPath));
                    Assert.That(workspacePath, Is.EqualTo(Path.Combine(directory.Path, ".vscode", "mcp.json")));
                    Assert.That(userPath, Is.EqualTo(McpConfigurationPaths.UserConfigurationFile(false)));
                });
            }
            finally
            {
                Environment.SetEnvironmentVariable("XDG_CONFIG_HOME", previous);
            }
        }

        [Test]
        public void UserConfigurationFile_UsesTheCurrentPlatformProfile()
        {
            string stable = McpConfigurationPaths.UserConfigurationFile(false);
            string insiders = McpConfigurationPaths.UserConfigurationFile(true);

            Assert.Multiple(() =>
            {
                Assert.That(stable, Does.EndWith(Path.Combine("Code", "User", "mcp.json")));
                Assert.That(insiders, Does.Contain("Code - Insiders"));
                Assert.That(insiders, Does.EndWith(Path.Combine("User", "mcp.json")));
            });
        }
    }
}
