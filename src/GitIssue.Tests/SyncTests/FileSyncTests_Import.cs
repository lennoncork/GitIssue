using System;
using System.IO;
using System.Threading.Tasks;
using GitIssue.Issues;
using GitIssue.Syncs;
using NUnit.Framework;

namespace GitIssue.Tests.SyncTests
{
    [TestFixture]
    public partial class FileSyncTests
    {
        [TestFixture]
        public class Import : FileSyncTests
        {
            [Test]
            public async Task ImportsIssue()
            {
                string issue_file = Path.Combine(TestContext.CurrentContext.TestDirectory,
                    "SyncTests", "ImportFiles", "jira-issue.json");

                string config_file = Path.Combine(TestContext.CurrentContext.TestDirectory,
                    "SyncTests", "ImportFiles", "jira-config.json");

                this.Initialize(this.TestDirectory);
                FileImporter importer = new FileImporter(this.Sut) { Configuration = await SyncConfiguration.ReadAsync(config_file), ImportPath = issue_file };
                await importer.Import();
            }

            [Test]
            public void SaveMapping_RejectsPathOutsideImportDirectory()
            {
                this.Initialize(this.TestDirectory);
                string sentinel = Path.Combine(this.TestDirectory, "sentinel.txt");
                File.WriteAllText(sentinel, "preserve");
                FileImporter importer = new FileImporter(this.Sut);
                SyncedIssue synced = new SyncedIssue(importer, IssueKey.Create("local"), IssueKey.Create("../../sentinel.txt"));

                Assert.ThrowsAsync<ArgumentException>(async () => await synced.SaveAsync());
                Assert.That(File.ReadAllText(sentinel), Is.EqualTo("preserve"));
            }
        }
    }
}