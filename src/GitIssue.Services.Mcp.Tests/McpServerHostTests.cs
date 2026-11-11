using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using GitIssue.Services;
using GitIssue.Services.Mcp.Tools;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Server;
using NUnit.Framework;

namespace GitIssue.Services.Mcp.Tests
{
    [TestFixture]
    [NonParallelizable]
    public class McpServerHostTests
    {
        [Test]
        public void CreateHost_RegistersIssueToolsResourcesAndPrompts()
        {
            using TempDirectory directory = new TempDirectory();
            using IHost host = McpServerHost.CreateHost(new McpServeOptions(directory.Path, ".issues"));

            string[] tools = host.Services.GetServices<McpServerTool>().Select(tool => tool.ProtocolTool.Name).ToArray();
            string[] resources = host.Services.GetServices<McpServerResource>().Select(resource => resource.ProtocolResource?.Name ?? resource.ProtocolResourceTemplate?.Name ?? string.Empty).ToArray();
            string[] prompts = host.Services.GetServices<McpServerPrompt>().Select(prompt => prompt.ProtocolPrompt.Name).ToArray();

            Assert.Multiple(() =>
            {
                Assert.That(tools, Is.EquivalentTo(new[]
                {
                    "list_issues",
                    "get_issue",
                    "find_issues",
                    "list_fields",
                    "get_tracked_issue",
                    "get_changes",
                    "create_issue",
                    "update_issue",
                    "add_comment",
                    "track_issue",
                }));
                Assert.That(resources, Does.Contain("schema"));
                Assert.That(resources, Does.Contain("issue"));
                Assert.That(prompts, Is.EquivalentTo(new[] { "triage-issues", "draft-issue" }));
            });
        }

        [Test]
        public void CreateHost_DoesNotLoadRepositoryApplicationSettings()
        {
            using TempDirectory directory = new TempDirectory();
            File.WriteAllText(Path.Combine(directory.Path, "appsettings.json"), "{");

            using IHost host = McpServerHost.CreateHost(new McpServeOptions(directory.Path, ".issues"));

            Assert.That(
                host.Services.GetRequiredService<IHostEnvironment>().ContentRootPath,
                Is.EqualTo(AppContext.BaseDirectory));
        }

        [Test]
        public async Task CreateHost_ResolvesTheIssueService()
        {
            using RepositoryFixture fixture = new RepositoryFixture();
            using IHost host = McpServerHost.CreateHost(new McpServeOptions(fixture.Directory, ".issues"));
            IssueTools tools = ActivatorUtilities.CreateInstance<IssueTools>(host.Services);

            IssueDocument created = await tools.CreateIssue("Host issue", "Created through dependency injection.", CancellationToken.None);
            string schema = ActivatorUtilities.CreateInstance<IssueResources>(host.Services).Schema();

            Assert.Multiple(() =>
            {
                Assert.That(created.Title, Is.EqualTo("Host issue"));
                Assert.That(schema, Does.Contain("Title"));
                Assert.That(IssuePrompts.TriageIssues(), Does.Contain("list_issues"));
                Assert.That(IssuePrompts.DraftIssue(), Does.Contain("list_fields"));
            });
        }

        [Test]
        public async Task CreateHost_KeepsStandardIoAndDoesNotOpenAPort()
        {
            using TempDirectory directory = new TempDirectory();
            int port = McpServerHostTests.freePort();
            Environment.SetEnvironmentVariable("ASPNETCORE_URLS", $"http://127.0.0.1:{port}");
            try
            {
                await using WebApplication application = McpServerHost.CreateHost(new McpServeOptions(directory.Path, ".issues"));
                await application.StartAsync();
                try
                {
                    Assert.That(McpServerHostTests.portIsOpen(port), Is.False);
                }
                finally
                {
                    await application.StopAsync();
                }
            }
            finally
            {
                Environment.SetEnvironmentVariable("ASPNETCORE_URLS", null);
            }
        }

        [Test]
        public void IssueTools_WrapsRepositoryFailures()
        {
            IssueTools tools = new IssueTools(new IssueSession(() => throw new InvalidOperationException("missing config")));

            IssueOperationException? exception = Assert.Throws<IssueOperationException>(() => tools.ListFields());

            Assert.That(exception!.Message, Does.Contain("gitissue init"));
        }

        private static int freePort()
        {
            using TcpListener listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        private static bool portIsOpen(int port)
        {
            try
            {
                using TcpClient client = new TcpClient();
                Task connect = client.ConnectAsync(IPAddress.Loopback, port);
                return connect.Wait(200) && client.Connected;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
