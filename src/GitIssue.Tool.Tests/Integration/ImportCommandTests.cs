using System.IO;
using NUnit.Framework;

namespace GitIssue.Tool.Tests.Integration
{
    [TestFixture]
    public class ImportCommandTests : IntegrationTestBase
    {
        [Test]
        public void Import_CreatesIssueFromCsv()
        {
            this.InitializeRepository();
            string importFile = Path.Combine(this.TestDirectory, "issues.csv");
            File.WriteAllText(importFile, $"Key; Title{System.Environment.NewLine}IMPORTED-1; Imported issue");
            string importContents = this.ReadFile(importFile);

            int exitCode = this.Run("Import", importFile);

            string key = this.FindIssueKey("Imported issue");
            var issue = this.ReadIssue(key);
            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.Zero, this.Error);
                this.AssertFileExists(importFile);
                this.AssertFileContents(importFile, importContents);
                this.AssertIssueExists(key);
                this.AssertJsonValue(issue, "Title", "Imported issue");
                Assert.That(this.Output, Does.Contain("Imported 1 issues"));
            });
        }

        [Test]
        public void Import_PersistsAdditionalColumnsOnCreate()
        {
            this.InitializeRepository();
            string importFile = Path.Combine(this.TestDirectory, "issues.csv");
            File.WriteAllText(
                importFile,
                $"Key; Title; Description{System.Environment.NewLine}IMPORTED-1; Imported issue; Imported description");

            int exitCode = this.Run("Import", importFile);

            string key = this.FindIssueKey("Imported issue");
            var issue = this.ReadIssue(key);
            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.Zero, this.Error);
                Assert.That(this.GetIssueKeys(), Is.EqualTo(new[] { key }));
                this.AssertIssueExists(key);
                this.AssertJsonValue(issue, "Key", key);
                this.AssertJsonValue(issue, "Title", "Imported issue");
                this.AssertJsonValue(issue, "Description", "Imported description");
                Assert.That(this.Output, Does.Contain("Imported 1 issues"));
            });
        }

        [Test]
        public void Import_UpdateChangesExistingIssueWithoutCreatingDuplicate()
        {
            this.InitializeRepository();
            string key = this.CreateIssue("Original title", "Original description");
            string importFile = Path.Combine(this.TestDirectory, "issues.csv");
            File.WriteAllText(
                importFile,
                $"Key; Title; Description{System.Environment.NewLine}{key}; Updated title; Updated description");

            int exitCode = this.Run("Import", importFile, "--Update");

            var issue = this.ReadIssue(key);
            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.Zero, this.Error);
                Assert.That(this.GetIssueKeys(), Is.EqualTo(new[] { key }));
                this.AssertJsonValue(issue, "Key", key);
                this.AssertJsonValue(issue, "Title", "Updated title");
                this.AssertJsonValue(issue, "Description", "Updated description");
                Assert.That(this.Output, Does.Contain($"Updating {key}"));
                Assert.That(this.Output, Does.Contain("Imported 1 issues"));
            });
        }

        [Test]
        public void Import_WithoutUpdateLeavesMatchingIssueUnchanged()
        {
            this.InitializeRepository();
            string key = this.CreateIssue("Original title", "Original description");
            string importFile = Path.Combine(this.TestDirectory, "issues.csv");
            File.WriteAllText(
                importFile,
                $"Key; Title; Description{System.Environment.NewLine}{key}; Updated title; Updated description");

            int exitCode = this.Run("Import", importFile);

            var issue = this.ReadIssue(key);
            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.Zero, this.Error);
                Assert.That(this.GetIssueKeys(), Is.EqualTo(new[] { key }));
                this.AssertJsonValue(issue, "Key", key);
                this.AssertJsonValue(issue, "Title", "Original title");
                this.AssertJsonValue(issue, "Description", "Original description");
                Assert.That(this.Output, Does.Contain("Imported 0 issues"));
            });
        }

        [Test]
        public void Import_ReturnsNonZeroWhenFileDoesNotExist()
        {
            this.InitializeRepository();
            string importFile = Path.Combine(this.TestDirectory, "missing.csv");

            int exitCode = this.Run("Import", importFile);

            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.Not.Zero);
                Assert.That(this.Output + this.Error, Does.Contain($"Import file {importFile} does not exist"));
                Assert.That(this.GetIssueKeys(), Is.Empty);
            });
        }

        [Test]
        public void Import_MissingTitleColumnFails()
        {
            this.InitializeRepository();
            string importFile = Path.Combine(this.TestDirectory, "issues.csv");
            File.WriteAllText(
                importFile,
                $"Key; Description{System.Environment.NewLine}IMPORTED-1; Imported description");

            int exitCode = this.Run("Import", importFile);

            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.Not.Zero, this.Output);
                Assert.That(this.GetIssueKeys(), Is.Empty);
                Assert.That(this.Output, Does.Contain("CSV import requires the Title column."));
            });
        }

        [Test]
        public void Import_MissingKeyColumnFails()
        {
            this.InitializeRepository();
            string importFile = Path.Combine(this.TestDirectory, "issues.csv");
            File.WriteAllText(
                importFile,
                $"Title; Description{System.Environment.NewLine}Imported issue; Imported description");

            int exitCode = this.Run("Import", importFile);

            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.Not.Zero, this.Output);
                Assert.That(this.GetIssueKeys(), Is.Empty);
                Assert.That(this.Output, Does.Contain("CSV import requires the Key column."));
            });
        }

        [Test]
        public void Import_CreatesIssueFromJson()
        {
            this.InitializeRepository();
            string importFile = Path.Combine(this.TestDirectory, "issue.json");
            File.WriteAllText(importFile, "{ \"Key\": \"EXT-1\", \"Title\": \"Imported title\", \"Description\": \"Imported body\" }");

            int exitCode = this.Run("Import", importFile);

            string key = this.FindIssueKey("Imported title");
            var issue = this.ReadIssue(key);
            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.Zero, this.Error);
                this.AssertIssueExists(key);
                this.AssertJsonValue(issue, "Title", "Imported title");
                this.AssertJsonValue(issue, "Description", "Imported body");
                Assert.That(this.GetIssueKeys(), Has.Length.EqualTo(1));
                Assert.That(this.Output, Does.Contain("Imported 1 issues"));
            });
        }

        [Test]
        public void Import_CreatesIssueFromLowerCaseJson()
        {
            this.InitializeRepository();
            string importFile = Path.Combine(this.TestDirectory, "issue.json");
            File.WriteAllText(importFile, "{ \"key\": \"EXT-2\", \"title\": \"Lower case title\", \"description\": \"Lower case body\" }");

            int exitCode = this.Run("Import", importFile);

            string key = this.FindIssueKey("Lower case title");
            var issue = this.ReadIssue(key);
            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.Zero, this.Error);
                this.AssertJsonValue(issue, "Title", "Lower case title");
                this.AssertJsonValue(issue, "Description", "Lower case body");
                Assert.That(this.GetIssueKeys(), Has.Length.EqualTo(1));
                Assert.That(this.Output, Does.Contain("Imported 1 issues"));
            });
        }

        [Test]
        public void Import_ReturnsNonZeroWhenJsonFileDoesNotExist()
        {
            this.InitializeRepository();
            string importFile = Path.Combine(this.TestDirectory, "missing.json");

            int exitCode = this.Run("Import", importFile);

            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.Not.Zero);
                Assert.That(this.Output + this.Error, Does.Contain($"Import file {importFile} does not exist"));
                Assert.That(this.GetIssueKeys(), Is.Empty);
            });
        }

        [Test]
        public void Import_UnknownExtensionExitsNonZero()
        {
            this.InitializeRepository();
            string importFile = Path.Combine(this.TestDirectory, "issues.txt");
            File.WriteAllText(importFile, "not an import");

            int exitCode = this.Run("Import", importFile);

            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.Not.Zero);
                Assert.That(this.Output + this.Error, Does.Contain($"Unsupported import file '{importFile}'. Use a .json or .csv file."));
                Assert.That(this.GetIssueKeys(), Is.Empty);
                this.AssertFileContents(importFile, "not an import");
            });
        }

        [Test]
        public void Sync_ImportsIssueFromJson()
        {
            this.InitializeRepository();
            string importFile = Path.Combine(this.TestDirectory, "issue.json");
            File.WriteAllText(importFile, "{ \"Key\": \"EXT-1\", \"Title\": \"Synced title\", \"Description\": \"Synced body\" }");

            int exitCode = this.Run("Sync", importFile);

            string key = this.FindIssueKey("Synced title");
            var issue = this.ReadIssue(key);
            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.Zero, this.Error);
                this.AssertJsonValue(issue, "Title", "Synced title");
                this.AssertJsonValue(issue, "Description", "Synced body");
                Assert.That(this.GetIssueKeys(), Has.Length.EqualTo(1));
                Assert.That(this.Output, Does.Contain("Imported 1 issues"));
                Assert.That(this.Output, Does.Not.Contain("Exported"));
            });
        }

        [Test]
        public void Sync_ReturnsNonZeroWhenImportFails()
        {
            this.InitializeRepository();
            string importFile = Path.Combine(this.TestDirectory, "missing.json");

            int exitCode = this.Run("Sync", importFile);

            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.Not.Zero);
                Assert.That(this.Output + this.Error, Does.Contain($"Import file {importFile} does not exist"));
                Assert.That(this.GetIssueKeys(), Is.Empty);
            });
        }
    }
}
