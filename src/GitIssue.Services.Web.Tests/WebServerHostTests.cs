using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;
using GitIssue.Services;
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
        public async Task IssuePost_WithoutAntiforgeryToken_IsRejected()
        {
            using RepositoryFixture fixture = new RepositoryFixture();
            WebServeRequest request = WebServerHostTests.request(fixture.Directory, WebServerHostTests.freePort(), null);
            await using WebApplication application = WebServerHost.Create(request);
            await application.StartAsync();
            try
            {
                using HttpClient client = new HttpClient();
                using StringContent content = new StringContent("_handler=Title&Field=Title&Value=Changed", System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");
                HttpResponseMessage response = await client.PostAsync($"{request.Url}/issue?key=missing", content);

                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
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

        [Test]
        public void Create_UsesAnAbsoluteRepositoryContentRoot()
        {
            using RepositoryFixture fixture = new RepositoryFixture();
            WebServeRequest request = WebServerHostTests.request(
                Path.GetRelativePath(Environment.CurrentDirectory, fixture.Directory),
                WebServerHostTests.freePort(),
                null);

            using WebApplication application = WebServerHost.Create(request);

            Assert.That(application.Environment.ContentRootPath, Is.EqualTo(Path.GetFullPath(request.RepositoryPath)));
        }

        [Test]
        public async Task ConfiguredKestrelEndpoints_DoNotOverrideTheRequestedUrl()
        {
            using RepositoryFixture fixture = new RepositoryFixture();
            int configuredPort = WebServerHostTests.freePort();
            int requestedPort = WebServerHostTests.freePort();
            while (requestedPort == configuredPort)
            {
                requestedPort = WebServerHostTests.freePort();
            }

            File.WriteAllText(
                Path.Combine(fixture.Directory, "appsettings.json"),
                JsonSerializer.Serialize(new
                {
                    Kestrel = new
                    {
                        Endpoints = new
                        {
                            Configured = new { Url = $"http://127.0.0.1:{configuredPort}" },
                        },
                    },
                }));
            WebServeRequest request = WebServerHostTests.request(fixture.Directory, requestedPort, null);
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

        internal static WebServeRequest request(string directory, int port, string? token)
        {
            return new WebServeRequest
            {
                RepositoryPath = directory,
                IssueFolderName = Paths.IssueRootFolderName,
                Host = "127.0.0.1",
                Port = port,
                Token = token,
            };
        }

        internal static int freePort()
        {
            using TcpListener listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

    }
}
