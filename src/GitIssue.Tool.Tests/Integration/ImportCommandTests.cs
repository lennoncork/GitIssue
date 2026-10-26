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
        public void Import_UnknownExtensionFails()
        {
            this.InitializeRepository();
            string importFile = Path.Combine(this.TestDirectory, "issues.txt");
            File.WriteAllText(importFile, "Key; Title");

            int exitCode = this.Run("Import", importFile);

            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.Not.Zero, this.Output);
                Assert.That(this.GetIssueKeys(), Is.Empty);
                Assert.That(this.Output, Does.Contain("Import of \".txt\" files is not supported."));
            });
        }
    }
}
