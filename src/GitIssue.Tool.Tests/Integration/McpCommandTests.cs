using System.IO;
using NUnit.Framework;

namespace GitIssue.Tool.Tests.Integration
{
    [TestFixture]
    public class McpCommandTests : IntegrationTestBase
    {
        [Test]
        public void Help_PrintsMcpUsage()
        {
            int exitCode = this.Run("mcp", "--help");

            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.Zero, this.Error);
                Assert.That(this.Output, Does.Contain("gitissue mcp install"));
            });
        }

        [Test]
        public void InstallAndUninstall_UpdatesWorkspaceConfiguration()
        {
            Assert.That(this.Run("mcp", "install"), Is.Zero, this.Error);

            string configuration = Path.Combine(this.TestDirectory, ".vscode", "mcp.json");
            this.AssertFileExists(configuration);
            Assert.That(File.ReadAllText(configuration), Does.Contain("\"gitissue\""));

            Assert.That(this.Run("mcp", "uninstall"), Is.Zero, this.Error);
            Assert.That(File.ReadAllText(configuration), Does.Not.Contain("gitissue"));
        }

        [Test]
        public void WebHelp_PrintsServeUsage()
        {
            int exitCode = this.Run("web", "--help");

            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.Zero, this.Error);
                Assert.That(this.Output, Does.Contain("gitissue web serve"));
            });
        }

        [Test]
        public void WebUnknownAction_Fails()
        {
            int exitCode = this.Run("web", "install");

            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.EqualTo(1));
                Assert.That(this.Error, Does.Contain("Unknown web action"));
            });
        }

        [Test]
        public void WebServe_ReportsAMissingHostAssembly()
        {
            int exitCode = this.Run("web", "serve");

            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.EqualTo(1));
                Assert.That(this.Error, Does.Contain("web host was not found"));
            });
        }
    }
}
