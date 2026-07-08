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
                string issue_file = Path.Combine(TestContext.CurrentContext.TestDirectory,
                    "SyncTests", "ImportFiles", "jira-issue.json");

                string config_file = Path.Combine(TestContext.CurrentContext.TestDirectory,
                    "SyncTests", "ImportFiles", "jira-config.json");

                this.Initialize(this.TestDirectory);
                FileImporter importer = new FileImporter(this.Sut) { Configuration = await SyncConfiguration.ReadAsync(config_file), ImportPath = issue_file };
                await importer.Import();
            }
        }
    }
}