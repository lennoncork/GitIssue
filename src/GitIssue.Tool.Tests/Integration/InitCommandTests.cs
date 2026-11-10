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
                Assert.That(configuration["IssueType"]?.GetValue<string>(), Is.EqualTo("ISSUE"));
                Assert.That(configuration["KeyTemplate"]?.GetValue<string>(), Is.EqualTo("{created:yyyy}/{created:MM}/{created:dd}/{uid}"));
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
                Assert.That(configuration["KeyTemplate"]?.GetValue<string>(), Is.EqualTo("{uid} - {name}"));
            });
        }

        [Test]
        public void Init_SelectsKeyTemplate()
        {
            int exitCode = this.Run("Init", "--issue-type", "FEAT", "--key-template", "{type}-{uid}-{slug}");

            var configuration = this.ReadJsonObject(this.ConfigurationFile);
            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.Zero, this.Error);
                Assert.That(configuration["KeyProvider"]?.GetValue<string>(), Is.EqualTo("TemplateIssueKeyProvider"));
                Assert.That(configuration["IssueType"]?.GetValue<string>(), Is.EqualTo("FEAT"));
                Assert.That(configuration["KeyTemplate"]?.GetValue<string>(), Is.EqualTo("{type}-{uid}-{slug}"));
            });
        }

        [Test]
        public void Init_RejectsUnknownKeyProvider()
        {
            int exitCode = this.Run("Init", "--key-provider", "uuid");

            Assert.That(exitCode, Is.EqualTo(1), this.Output + this.Error);
        }

        [Test]
        public void Init_TemplateProviderRequiresTemplate()
        {
            int exitCode = this.Run("Init", "--key-provider", "template");

            Assert.That(exitCode, Is.EqualTo(1));
            Assert.That(this.Output, Does.Contain("requires --key-template"));
            this.AssertDirectoryDoesNotExist(this.IssuesDirectory);
        }

        [TestCase("date", "DateIssueKeyProvider")]
        [TestCase("DateIssueKeyProvider", "DateIssueKeyProvider")]
        [TestCase("NameIssueKeyProvider", "NameIssueKeyProvider")]
        [TestCase("File", "FileIssueKeyProvider")]
        [TestCase("FileIssueKeyProvider", "FileIssueKeyProvider")]
        public void Init_SelectsProviderByAlias(string provider, string expected)
        {
            int exitCode = this.Run("Init", "--key-provider", provider);

            Assert.That(exitCode, Is.Zero, this.Output + this.Error);
            Assert.That(this.ReadJsonObject(this.ConfigurationFile)["KeyProvider"]?.GetValue<string>(), Is.EqualTo(expected));
        }

        [Test]
        public void Init_RejectsResolvedTypeThatIsNotAKeyProvider()
        {
            int exitCode = this.Run("Init", "--key-provider", "System.String");

            Assert.That(exitCode, Is.EqualTo(1));
            Assert.That(this.Output, Does.Contain("is not Date, Name, or Template"));
            this.AssertDirectoryDoesNotExist(this.IssuesDirectory);
        }

        [TestCase("bug fix")]
        [TestCase("bug_fix")]
        [TestCase("   ")]
        public void Init_RejectsInvalidIssueType(string type)
        {
            int exitCode = this.Run("Init", "--issue-type", type);

            Assert.That(exitCode, Is.EqualTo(1), this.Output + this.Error);
            this.AssertDirectoryDoesNotExist(this.IssuesDirectory);
        }

        [Test]
        public void Help_ReturnsZero()
        {
            int exitCode = this.Run("--help");

            Assert.That(exitCode, Is.Zero);
        }
    }
}
