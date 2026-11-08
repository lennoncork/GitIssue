using System;
using System.IO;
using NUnit.Framework;

namespace GitIssue.Mcp.Tests
{
    [TestFixture]
    public class McpCommandTests
    {
        [Test]
        public void Execute_Help_PrintsUsage()
        {
            using StringWriter output = new StringWriter();
            using StringWriter error = new StringWriter();

            int code = McpApplication.Execute(new[] { "mcp", "--help" }, output, error);

            Assert.Multiple(() =>
            {
                Assert.That(code, Is.Zero);
                Assert.That(output.ToString(), Does.Contain("gitissue mcp install"));
                Assert.That(error.ToString(), Is.Empty);
            });
        }

        [Test]
        public void Execute_InstallDryRun_DoesNotWriteTheWorkspaceFile()
        {
            using TempDirectory directory = new TempDirectory();
            using StringWriter output = new StringWriter();
            using StringWriter error = new StringWriter();

            int code = McpApplication.Execute(
                new[] { "mcp", "install", "--dry-run", "--path", directory.Path, "--server-arg", "help" },
                output,
                error);

            Assert.Multiple(() =>
            {
                Assert.That(code, Is.Zero, error.ToString());
                Assert.That(output.ToString(), Does.Contain("\"gitissue\""));
                Assert.That(output.ToString(), Does.Contain("help"));
                Assert.That(File.Exists(Path.Combine(directory.Path, ".vscode", "mcp.json")), Is.False);
            });
        }

        [Test]
        public void Parse_ServeRejectsInstallOptions()
        {
            McpArguments parsed = McpCommandLine.Parse(new[] { "mcp", "serve", "--dry-run" });

            Assert.That(parsed.IsValid, Is.False);
        }

        [Test]
        public void Execute_InstallAndUninstall_WritesTheWorkspaceFile()
        {
            using TempDirectory directory = new TempDirectory();
            using StringWriter output = new StringWriter();
            using StringWriter error = new StringWriter();
            string[] install = new[] { "mcp", "install", "--path", directory.Path, "--scope", "workspace" };

            int installed = McpApplication.Execute(install, output, error);
            string configuration = Path.Combine(directory.Path, ".vscode", "mcp.json");
            string installedText = File.ReadAllText(configuration);
            int unchanged = McpApplication.Execute(
                new[] { "mcp", "uninstall", "--dry-run", "--path", directory.Path },
                output,
                error);
            File.WriteAllText(configuration, """
                {
                  "servers": {
                    "other": { "type": "stdio", "command": "other" }
                  }
                }
                """);
            int absent = McpApplication.Execute(
                new[] { "mcp", "uninstall", "--path", directory.Path },
                output,
                error);
            int missing = McpApplication.Execute(
                new[] { "mcp", "uninstall", "--path", Path.Combine(directory.Path, "missing") },
                output,
                error);

            Assert.Multiple(() =>
            {
                Assert.That(installed, Is.Zero, error.ToString());
                Assert.That(output.ToString(), Does.Contain("Installed gitissue"));
                Assert.That(installedText, Does.Contain("gitissue"));
                Assert.That(unchanged, Is.Zero, error.ToString());
                Assert.That(absent, Is.Zero, error.ToString());
                Assert.That(output.ToString(), Does.Contain("was not installed"));
                Assert.That(missing, Is.Zero, error.ToString());
                Assert.That(output.ToString(), Does.Contain("No MCP configuration"));
            });
        }

        [Test]
        public void Execute_Serve_MissingDirectoryReturnsFailure()
        {
            using StringWriter output = new StringWriter();
            using StringWriter error = new StringWriter();

            int code = McpApplication.Execute(
                new[] { "mcp", "serve", "--path", Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")) },
                output,
                error);

            Assert.Multiple(() =>
            {
                Assert.That(code, Is.EqualTo(1));
                Assert.That(error.ToString(), Does.Contain("does not exist"));
            });
        }

        [Test]
        public void Parse_RejectsIncompleteAndMisplacedOptions()
        {
            Assert.Multiple(() =>
            {
                Assert.That(McpCommandLine.Parse(new[] { "mcp", "install", "--path" }).IsValid, Is.False);
                Assert.That(McpCommandLine.Parse(new[] { "mcp", "install", "--scope", "global" }).IsValid, Is.False);
                Assert.That(McpCommandLine.Parse(new[] { "mcp", "install", "--unknown" }).IsValid, Is.False);
                Assert.That(McpCommandLine.Parse(new[] { "mcp", "uninstall", "--command", "dotnet" }).IsValid, Is.False);
                Assert.That(McpCommandLine.Parse(new[] { "mcp" }).Action, Is.EqualTo(McpAction.Serve));
                Assert.That(
                    McpCommandLine.Parse(new[] { "mcp", "install", "--scope", "user", "--insiders", "--name", "work-items" }).Scope,
                    Is.EqualTo(McpInstallScope.User));
            });
        }

        [Test]
        public void Execute_UnknownAction_ReturnsFailure()
        {
            using StringWriter output = new StringWriter();
            using StringWriter error = new StringWriter();

            int code = McpApplication.Execute(new[] { "mcp", "publish" }, output, error);

            Assert.Multiple(() =>
            {
                Assert.That(code, Is.EqualTo(1));
                Assert.That(error.ToString(), Does.Contain("Unknown mcp action"));
            });
        }
    }
}
