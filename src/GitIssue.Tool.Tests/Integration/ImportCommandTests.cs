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
    }
}