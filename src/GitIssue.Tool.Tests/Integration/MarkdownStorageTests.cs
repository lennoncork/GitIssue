using System.IO;
using System.Linq;
using NUnit.Framework;

namespace GitIssue.Tool.Tests.Integration
{
    [TestFixture]
    public class MarkdownStorageTests : IntegrationTestBase
    {
        [Test]
        public void Init_DefaultsToMarkdownStorage()
        {
            Assert.That(this.Run("Init"), Is.Zero, this.Error);
            Assert.That(this.ReadJsonObject(this.ConfigurationFile)["Storage"]?.GetValue<string>(), Is.EqualTo("Markdown"));

            Assert.That(this.Run("Create", "Default storage", "Stored as markdown."), Is.Zero, this.Error);
            string key = this.findMarkdownKey("Default storage");

            this.AssertFileExists(this.getMarkdownFile(key));
            this.AssertFileDoesNotExist(this.GetIssueFile(key));
        }

        [Test]
        public void InitCreateValidateAndConvert_RoundTripsMarkdownStorage()
        {
            Assert.That(this.Run("Init", "--storage", "markdown"), Is.Zero, this.Error);
            Assert.That(this.ReadJsonObject(this.ConfigurationFile)["Storage"]?.GetValue<string>(), Is.EqualTo("Markdown"));

            Assert.That(this.Run("Create", "Markdown issue", "Stored as markdown."), Is.Zero, this.Error);
            string key = this.findMarkdownKey("Markdown issue");
            string issueFile = this.getMarkdownFile(key);

            Assert.Multiple(() =>
            {
                this.AssertFileExists(issueFile);
                this.AssertFileDoesNotExist(this.GetIssueFile(key));
                Assert.That(File.ReadAllText(issueFile), Does.Contain("## Title"));
                Assert.That(File.ReadAllText(issueFile), Does.Contain("Stored as markdown."));
            });

            Assert.That(this.Run("Validate"), Is.Zero, this.Output + this.Error);
            Assert.That(this.Output, Does.Contain("1 issue checked, 0 invalid."));
            Assert.That(this.Run("Show", "all"), Is.Zero, this.Error);
            Assert.That(this.Output, Does.Contain("Markdown issue"));

            string original = File.ReadAllText(issueFile);
            File.WriteAllText(issueFile, original.Replace("GitIssue Tool Tests <gitissue-tool-tests@example.com>", "Ada", System.StringComparison.Ordinal));
            Assert.That(this.Run("Validate", key), Is.EqualTo(1), this.Output);
            Assert.That(this.Output, Does.Contain("Author"));
            Assert.That(this.Output, Does.Contain("not a valid Signature"));
            Assert.That(File.ReadAllText(issueFile), Does.Contain("Ada"));

            File.WriteAllText(issueFile, original);
            Assert.That(this.Run("Convert", "--to", "json"), Is.Zero, this.Error);
            Assert.Multiple(() =>
            {
                this.AssertFileExists(this.GetIssueFile(key));
                this.AssertFileDoesNotExist(issueFile);
                Assert.That(this.ReadJsonObject(this.ConfigurationFile)["Storage"]?.GetValue<string>() ?? "Json", Is.EqualTo("Json"));
                Assert.That(this.ReadIssue(key)["Title"]?.GetValue<string>(), Is.EqualTo("Markdown issue"));
            });

            Assert.That(this.Run("Validate"), Is.Zero, this.Output + this.Error);
            Assert.That(this.Run("Convert", "--to", "markdown"), Is.Zero, this.Output + this.Error);
            this.AssertFileExists(issueFile);
            this.AssertFileDoesNotExist(this.GetIssueFile(key));
        }

        [Test]
        public void Validate_ReportsInvalidJsonAuthor()
        {
            this.InitializeRepository();
            string key = this.CreateIssue("Json issue", "Body");
            string issueFile = this.GetIssueFile(key);
            string json = File.ReadAllText(issueFile);
            File.WriteAllText(issueFile, json.Replace("GitIssue Tool Tests <gitissue-tool-tests@example.com>", "Ada", System.StringComparison.Ordinal));

            int exitCode = this.Run("Validate", key);

            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.EqualTo(1), this.Output + this.Error);
                Assert.That(this.Output, Does.Contain("not a valid Signature"));
                Assert.That(File.ReadAllText(issueFile), Does.Contain("Ada"));
            });
        }

        [Test]
        public void Convert_RejectsConflictingDocumentsWithoutChangingEitherFile()
        {
            this.InitializeRepository();
            string key = this.CreateIssue("Conflicting issue", "Body");
            string jsonPath = this.GetIssueFile(key);
            string markdownPath = this.getMarkdownFile(key);
            string originalJson = File.ReadAllText(jsonPath);
            const string originalMarkdown = "# Existing document";
            File.WriteAllText(markdownPath, originalMarkdown);

            int exitCode = this.Run("Convert", "--to", "markdown");

            Assert.That(exitCode, Is.EqualTo(1), this.Output + this.Error);
            Assert.That(this.Output, Does.Contain("has both JSON and Markdown files"));
            Assert.That(File.ReadAllText(jsonPath), Is.EqualTo(originalJson));
            Assert.That(File.ReadAllText(markdownPath), Is.EqualTo(originalMarkdown));
            Assert.That(this.ReadJsonObject(this.ConfigurationFile)["Storage"]?.GetValue<string>() ?? "Json", Is.EqualTo("Json"));
        }

        [Test]
        public void Convert_RejectsUnknownJsonFieldWithoutLosingData()
        {
            this.InitializeRepository();
            string validKey = this.CreateIssue("Valid JSON issue", "Body");
            string key = this.CreateIssue("Invalid JSON issue", "Body");
            string validJsonPath = this.GetIssueFile(validKey);
            string validOriginal = File.ReadAllText(validJsonPath);
            string jsonPath = this.GetIssueFile(key);
            var json = this.ReadIssue(key);
            json["Unexpected"] = "keep this";
            string original = json.ToJsonString();
            File.WriteAllText(jsonPath, original);

            int exitCode = this.Run("Convert", "--to", "markdown");

            Assert.That(exitCode, Is.EqualTo(1), this.Output + this.Error);
            Assert.That(this.Output, Does.Contain("unknown field"));
            Assert.That(File.ReadAllText(jsonPath), Is.EqualTo(original));
            this.AssertFileDoesNotExist(this.getMarkdownFile(key));
            Assert.That(File.ReadAllText(validJsonPath), Is.EqualTo(validOriginal));
            this.AssertFileDoesNotExist(this.getMarkdownFile(validKey));
            Assert.That(this.ReadJsonObject(this.ConfigurationFile)["Storage"]?.GetValue<string>() ?? "Json", Is.EqualTo("Json"));
        }

        [Test]
        public void Convert_RejectsUnknownMarkdownFieldWithoutLosingData()
        {
            Assert.That(this.Run("Init"), Is.Zero, this.Output + this.Error);
            Assert.That(this.Run("Create", "Invalid markdown issue", "Body"), Is.Zero, this.Output + this.Error);
            string key = this.findMarkdownKey("Invalid markdown issue");
            string markdownPath = this.getMarkdownFile(key);
            string original = File.ReadAllText(markdownPath) + "\n## Unexpected\n\nkeep this\n";
            File.WriteAllText(markdownPath, original);

            int exitCode = this.Run("Convert", "--to", "json");

            Assert.That(exitCode, Is.EqualTo(1), this.Output + this.Error);
            Assert.That(this.Output, Does.Contain("unknown field"));
            Assert.That(File.ReadAllText(markdownPath), Is.EqualTo(original));
            this.AssertFileDoesNotExist(this.GetIssueFile(key));
            Assert.That(this.ReadJsonObject(this.ConfigurationFile)["Storage"]?.GetValue<string>(), Is.EqualTo("Markdown"));
        }

        [Test]
        public void Init_RejectsUnknownStorage()
        {
            int exitCode = this.Run("Init", "--storage", "yaml");

            Assert.That(exitCode, Is.EqualTo(1), this.Output + this.Error);
        }

        private string findMarkdownKey(string title)
        {
            string[] keys = Directory.EnumerateFiles(this.IssuesDirectory, "*.md", SearchOption.AllDirectories)
                .Where(file => File.ReadAllText(file).Contains($"## Title\n\n{title}", System.StringComparison.Ordinal))
                .Select(file => Path.GetFileNameWithoutExtension(file))
                .ToArray();
            Assert.That(keys, Has.Length.EqualTo(1));
            return keys[0];
        }

        private string getMarkdownFile(string key)
        {
            return Path.Combine(this.GetIssueDirectory(key), $"{key}.md");
        }
    }
}
