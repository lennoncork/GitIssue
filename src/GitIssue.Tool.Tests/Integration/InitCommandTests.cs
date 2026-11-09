using System.IO;
using NUnit.Framework;

namespace GitIssue.Tool.Tests.Integration
{
    [TestFixture]
    public class InitCommandTests : IntegrationTestBase
    {
        [Test]
        public void Init_CreatesIssueRepository()
        {
            this.AssertDirectoryDoesNotExist(this.IssuesDirectory);

            int exitCode = this.Run("Init");

            var configuration = this.ReadJsonObject(this.ConfigurationFile);
            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.Zero, this.Error);
                this.AssertDirectoryExists(this.IssuesDirectory);
                this.AssertFileExists(this.ConfigurationFile);
                this.AssertFileDoesNotExist(this.TrackingFile);
                this.AssertFileDoesNotExist(this.ChangesFile);
                Assert.That(this.GetIssueKeys(), Is.Empty);
                this.AssertJsonPropertyExists(configuration, "Fields");
                this.AssertJsonPropertyExists(configuration["Fields"]!.AsObject(), "Title");
                Assert.That(configuration["Storage"]?.GetValue<string>(), Is.EqualTo("Markdown"));
                Assert.That(configuration["KeyProvider"]?.GetValue<string>(), Is.EqualTo("DateIssueKeyProvider"));
                Assert.That(this.Output, Does.Contain("Initialized empty 'Issue' repository"));
            });
        }

        [Test]
        public void Init_SelectsNameKeyProvider()
        {
            int exitCode = this.Run("Init", "--key-provider", "name");

            var configuration = this.ReadJsonObject(this.ConfigurationFile);
            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.Zero, this.Error);
                Assert.That(configuration["KeyProvider"]?.GetValue<string>(), Is.EqualTo("NameIssueKeyProvider"));
            });
        }

        [Test]
        public void Init_RejectsUnknownKeyProvider()
        {
            int exitCode = this.Run("Init", "--key-provider", "uuid");

            Assert.That(exitCode, Is.EqualTo(1), this.Output + this.Error);
        }

        [Test]
        public void Help_ReturnsZero()
        {
            int exitCode = this.Run("--help");

            Assert.That(exitCode, Is.Zero);
        }
    }
}
