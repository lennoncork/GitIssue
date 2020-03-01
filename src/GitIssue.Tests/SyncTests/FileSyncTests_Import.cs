using System.IO;
using System.Threading.Tasks;
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
                var issue_file = Path.Combine(TestContext.CurrentContext.TestDirectory, 
                    "SyncTests", "ImportFiles", "jira-issue.json");

                var config_file = Path.Combine(TestContext.CurrentContext.TestDirectory,
                    "SyncTests", "ImportFiles", "jira-config.json");

                Initialize(TestDirectory);
                var importer = new FileImporter(this.Sut)
                {
                    Configuration = await SyncConfiguration.ReadAsync(config_file),
                    ImportPath = issue_file,
                };
                await importer.Import();
            }
        }
    }
}