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
    }
}