using System.IO;
using NUnit.Framework;

namespace GitIssue.Tool.Tests.Integration
{
    [TestFixture]
    public class ExportCommandTests : IntegrationTestBase
    {
        [Test]
        public void Export_WritesIssuesToJson()
        {
            this.InitializeRepository();
            string key = this.CreateIssue("Issue to export");
            string exportFile = Path.Combine(this.TestDirectory, "issues.json");
            this.AssertFileDoesNotExist(exportFile);

            int exitCode = this.Run("Export", exportFile);

            var export = this.ReadJsonObject(exportFile);
            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.Zero, this.Error);
                Assert.That(File.Exists(exportFile), Is.True);
                this.AssertIssueExists(key);
                this.AssertJsonPropertyExists(export, key);
                this.AssertJsonValue(export[key]!.AsObject(), "Title", "Issue to export");
                Assert.That(this.Output, Does.Contain("Exported 1 issues"));
            });
        }

        [Test]
        public void Export_WritesIssuesToCsv()
        {
            this.InitializeRepository();
            this.CreateIssue("Issue to export as CSV", "CSV description");
            string exportFile = Path.Combine(this.TestDirectory, "issues.csv");
            this.AssertFileDoesNotExist(exportFile);

            int exitCode = this.Run("Export", exportFile);

            string[] lines = File.ReadAllLines(exportFile);
            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.Zero, this.Error);
                Assert.That(lines, Has.Length.EqualTo(2));
                Assert.That(lines[0], Does.Contain("Key").And.Contain("Title"));
                Assert.That(lines[1], Does.Contain("Issue to export as CSV").And.Contain("CSV description"));
                Assert.That(this.Output, Does.Contain("Exported 1 issues"));
            });
        }

        [Test]
        public void Export_DoesNotOverwriteExistingFileWithoutOverwriteOption()
        {
            this.InitializeRepository();
            this.CreateIssue("Issue to preserve export");
            string exportFile = Path.Combine(this.TestDirectory, "issues.json");
            File.WriteAllText(exportFile, "preserve");

            int exitCode = this.Run("Export", exportFile);

            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.Not.Zero);
                this.AssertFileContents(exportFile, "preserve");
                Assert.That(this.Output, Does.Not.Contain("Exported"));
            });
        }

        [Test]
        public void Export_OverwritesExistingFileWhenRequested()
        {
            this.InitializeRepository();
            string key = this.CreateIssue("Issue to overwrite export");
            string exportFile = Path.Combine(this.TestDirectory, "issues.json");
            File.WriteAllText(exportFile, "preserve");

            int exitCode = this.Run("Export", exportFile, "--Overwrite");

            var export = this.ReadJsonObject(exportFile);
            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.Zero, this.Error);
                this.AssertJsonPropertyExists(export, key);
                Assert.That(this.Output, Does.Contain("Exported 1 issues"));
            });
        }
    }
}