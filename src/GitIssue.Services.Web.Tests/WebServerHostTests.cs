using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Threading.Tasks;
using GitIssue.Issues;
using GitIssue.Services;
using LibGit2Sharp;
using Microsoft.AspNetCore.Builder;
using NUnit.Framework;

namespace GitIssue.Services.Web.Tests
{
    [TestFixture]
    public class WebServerHostTests
    {
        [Test]
        public async Task Health_ReturnsOkForAnInitializedRepository()
        {
            using RepositoryFixture fixture = new RepositoryFixture();
            WebServeRequest request = WebServerHostTests.request(fixture.Directory, WebServerHostTests.freePort(), null);
            await using WebApplication application = WebServerHost.Create(request);
            await application.StartAsync();
            try
            {
                using HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync($"{request.Url}/health");

                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            }
            finally
            {
                await application.StopAsync();
            }
        }

        [Test]
        public async Task Requests_RequireTheConfiguredToken()
        {
            using RepositoryFixture fixture = new RepositoryFixture();
            WebServeRequest request = WebServerHostTests.request(fixture.Directory, WebServerHostTests.freePort(), "secret");
            await using WebApplication application = WebServerHost.Create(request);
            await application.StartAsync();
            try
            {
                using HttpClient client = new HttpClient();
                HttpResponseMessage denied = await client.GetAsync($"{request.Url}/health");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "secret");
                HttpResponseMessage allowed = await client.GetAsync($"{request.Url}/");

                Assert.Multiple(() =>
                {
                    Assert.That(denied.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
                    Assert.That(allowed.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                });
            }
            finally
            {
                await application.StopAsync();
            }
        }

        [Test]
        public void Prepare_PrintsUsageForHelp()
        {
            using StringWriter output = new StringWriter();
            using StringWriter error = new StringWriter();

            int code = Program.Prepare(new[] { "--help" }, output, error);

            Assert.Multiple(() =>
            {
                Assert.That(code, Is.Zero);
                Assert.That(output.ToString(), Does.Contain("gitissue web serve"));
                Assert.That(error.ToString(), Is.Empty);
            });
        }

        [Test]
        public void Prepare_PrintsTheErrorForAnInvalidPort()
        {
            using StringWriter output = new StringWriter();
            using StringWriter error = new StringWriter();

            int code = Program.Prepare(new[] { "--port", "0" }, output, error);

            Assert.Multiple(() =>
            {
                Assert.That(code, Is.EqualTo(1));
                Assert.That(error.ToString(), Does.Contain("--port must be from 1 to 65535."));
            });
        }

        [Test]
        public void Prepare_ContinuesForAValidRequest()
        {
            using StringWriter output = new StringWriter();
            using StringWriter error = new StringWriter();

            int code = Program.Prepare(new[] { "--port", "5080" }, output, error);

            Assert.That(code, Is.EqualTo(-1));
        }

        [Test]
        public void Create_RejectsADirectoryThatIsNotInitialized()
        {
            string directory = Path.Combine(Path.GetTempPath(), "gitissue-web-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            try
            {
                Assert.That(
                    () => WebServerHost.Create(WebServerHostTests.request(directory, WebServerHostTests.freePort(), null)),
                    Throws.TypeOf<IssueOperationException>());
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        private static WebServeRequest request(string directory, int port, string? token)
        {
            return new WebServeRequest
            {
                RepositoryPath = directory,
                IssueFolderName = ".issues",
                Host = "127.0.0.1",
                Port = port,
                Token = token,
            };
        }

        private static int freePort()
        {
            using TcpListener listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        private sealed class RepositoryFixture : IDisposable
        {
            public RepositoryFixture()
            {
                this.Directory = Path.Combine(Path.GetTempPath(), "gitissue-web-" + Guid.NewGuid().ToString("N"));
                System.IO.Directory.CreateDirectory(this.Directory);
                LibGit2Sharp.Repository.Init(this.Directory);
                using Repository repository = new Repository(this.Directory);
                repository.Config.Set("user.name", "Tester");
                repository.Config.Set("user.email", "tester@example.com");
                using IIssueManager manager = IssueManager.Init(this.Directory);
            }

            public string Directory { get; }

            public void Dispose()
            {
                try
                {
                    System.IO.Directory.Delete(this.Directory, true);
                }
                catch (UnauthorizedAccessException)
                {
                }
            }
        }
    }
}
