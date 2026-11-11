using NUnit.Framework;

namespace GitIssue.Services.Tests
{
    [TestFixture]
    public class WebServeCommandLineTests
    {
        [Test]
        public void ParseCli_DefaultsToLoopbackServe()
        {
            WebServeRequest request = WebServeCommandLine.ParseCli(new[] { "web" });

            Assert.Multiple(() =>
            {
                Assert.That(request.IsValid, Is.True);
                Assert.That(request.Host, Is.EqualTo("127.0.0.1"));
                Assert.That(request.Port, Is.EqualTo(5080));
                Assert.That(request.Url, Is.EqualTo("http://127.0.0.1:5080"));
                Assert.That(request.IssueFolderName, Is.EqualTo(".issues"));
            });
        }

        [Test]
        public void ParseCli_HelpDoesNotRequireTheServeAction()
        {
            WebServeRequest request = WebServeCommandLine.ParseCli(new[] { "web", "--help" });

            Assert.Multiple(() =>
            {
                Assert.That(request.Help, Is.True);
                Assert.That(request.IsValid, Is.True);
            });
        }

        [Test]
        public void ParseCli_RejectsAnUnknownAction()
        {
            WebServeRequest request = WebServeCommandLine.ParseCli(new[] { "web", "install" });

            Assert.That(request.Error, Does.Contain("Unknown web action"));
        }

        [Test]
        public void ParseCli_RequiresATokenOffLoopback()
        {
            WebServeRequest missing = WebServeCommandLine.ParseCli(new[] { "web", "serve", "--host", "0.0.0.0" });
            WebServeRequest present = WebServeCommandLine.ParseCli(new[] { "web", "serve", "--host", "0.0.0.0", "--token", "secret" });

            Assert.Multiple(() =>
            {
                Assert.That(missing.Error, Does.Contain("--token"));
                Assert.That(present.IsValid, Is.True);
                Assert.That(present.Token, Is.EqualTo("secret"));
            });
        }

        [Test]
        public void ParseHost_ReadsForwardedOptions()
        {
            WebServeRequest request = WebServeCommandLine.ParseHost(new[]
            {
                "--path", "/work",
                "--name", "work-items",
                "--port", "5091",
            });

            Assert.Multiple(() =>
            {
                Assert.That(request.IsValid, Is.True);
                Assert.That(request.RepositoryPath, Is.EqualTo("/work"));
                Assert.That(request.IssueFolderName, Is.EqualTo("work-items"));
                Assert.That(request.Port, Is.EqualTo(5091));
            });
        }
    }
}
