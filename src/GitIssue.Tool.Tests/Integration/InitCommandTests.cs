using System.IO;
using System.Text.Json.Nodes;
using NUnit.Framework;

namespace GitIssue.Tool.Tests.Integration
{
    [TestFixture]
    public class InitCommandTests : IntegrationTestBase
    {
        [Test]
        public void Init_CreatesIssueRepository()
        {
            this.AssertFileDoesNotExist(this.ConfigurationFile);
            this.AssertDirectoryDoesNotExist(Path.Combine(this.TestDirectory, ".issues"));

            int exitCode = this.Run("Init");

            var configuration = this.ReadJsonObject(this.ConfigurationFile);
            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.Zero, this.Error);
                this.AssertFileExists(Path.Combine(this.TestDirectory, ".gitissue.config"));
                this.AssertDirectoryDoesNotExist(Path.Combine(this.TestDirectory, ".issues"));
                this.AssertFileExists(this.ConfigurationFile);
                this.AssertFileDoesNotExist(this.TrackingFile);
                this.AssertFileDoesNotExist(this.ChangesFile);
                Assert.That(this.GetIssueKeys(), Is.Empty);
                Assert.That(configuration.ContainsKey("Fields"), Is.False);
                JsonObject issue = configuration["Issues"]![0]!.AsObject();
                Assert.That(issue["Name"]?.GetValue<string>(), Is.EqualTo("Issue"));
                JsonObject fields = issue["Fields"]!.AsObject();
                this.AssertJsonPropertyExists(fields, "Title");
                Assert.That(fields["Title"]!["FieldType"]!.GetValue<string>(), Is.EqualTo("Value"));
                Assert.That(fields["Labels"]!["FieldType"]!.GetValue<string>(), Is.EqualTo("Array"));
                Assert.That(configuration["Storage"]?.GetValue<string>(), Is.EqualTo("Markdown"));
                Assert.That(configuration["KeyProvider"]?.GetValue<string>(), Is.EqualTo("DateIssueKeyProvider"));
                Assert.That(configuration["IssueType"]?.GetValue<string>(), Is.EqualTo("Issue"));
                Assert.That(configuration.ContainsKey("ProjectName"), Is.False);
                Assert.That(configuration["KeyTemplate"]?.GetValue<string>(), Is.EqualTo("{type}s/{created:yyyy}/{created:MM}/{created:dd}/{uid}"));
                Assert.That(this.Output, Does.Contain("Initialized empty 'Issue' repository"));
            });
        }

        [Test]
        public void Init_StoresBugIssuesBesideTheRepositoryRoot()
        {
            Assert.That(this.Run("Init", "--issue-type", "bug", "--storage", "json"), Is.Zero, this.Error);
            Assert.That(this.Run("Create", "Broken link", "The link fails."), Is.Zero, this.Error);

            string[] keys = this.GetIssueKeys();
            Assert.Multiple(() =>
            {
                Assert.That(keys, Has.Length.EqualTo(1));
                Assert.That(keys[0], Does.StartWith("bugs-"));
                this.AssertDirectoryExists(Path.Combine(this.TestDirectory, "bugs"));
                this.AssertDirectoryDoesNotExist(Path.Combine(this.TestDirectory, ".issues"));
                this.AssertFileExists(Path.Combine(this.TestDirectory, ".gitissue.config"));
                this.AssertFileExists(Path.Combine(this.TestDirectory, ".gitissue.tracking"));
                this.AssertFileExists(Path.Combine(this.TestDirectory, ".gitissue.changes"));
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
        public void Init_StoresProjectName()
        {
            int exitCode = this.Run("Init", "--project-name", "  Apollo  ");

            var configuration = this.ReadJsonObject(this.ConfigurationFile);
            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.Zero, this.Error);
                Assert.That(configuration["ProjectName"]?.GetValue<string>(), Is.EqualTo("Apollo"));
            });
        }

        [Test]
        public void Init_RejectsBlankProjectName()
        {
            int exitCode = this.Run("Init", "--project-name", "   ");

            Assert.That(exitCode, Is.EqualTo(1), this.Output + this.Error);
            Assert.That(this.Output + this.Error, Does.Contain("Project name cannot be empty."));
            this.AssertFileDoesNotExist(this.ConfigurationFile);
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
            this.AssertFileDoesNotExist(this.ConfigurationFile);
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
            this.AssertFileDoesNotExist(this.ConfigurationFile);
        }

        [TestCase("bug fix")]
        [TestCase("bug_fix")]
        [TestCase("   ")]
        public void Init_RejectsInvalidIssueType(string type)
        {
            int exitCode = this.Run("Init", "--issue-type", type);

            Assert.That(exitCode, Is.EqualTo(1), this.Output + this.Error);
            this.AssertFileDoesNotExist(this.ConfigurationFile);
        }

        [Test]
        public void Help_ReturnsZero()
        {
            int exitCode = this.Run("--help");

            Assert.That(exitCode, Is.Zero);
        }
    }
}
