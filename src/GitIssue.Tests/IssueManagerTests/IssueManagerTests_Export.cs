using System.IO;
using System.Threading.Tasks;
using GitIssue.Issues;
using GitIssue.Issues.Json;
using NUnit.Framework;

namespace GitIssue.Tests.IssueManagerTests
{
    [TestFixture]
    public partial class IssueManagerTests
    {
        [TestFixture]
        public class Export : IssueManagerTests
        {
            [Test]
            public async Task ExportsFileToJson()
            {
                this.Initialize(this.TestDirectory);
                SafeResult<IIssue> issue1 = await this.Sut
                    .CreateAsync(nameof(Export.ExportsFileToJson))
                    .WithSafeResultAsync();
                SafeResult<IIssue> issue2 = await this.Sut
                    .CreateAsync(nameof(Export.ExportsFileToJson))
                    .WithSafeResultAsync();
                string path = Path.Combine(this.TestDirectory, "export.json");
                await this.Sut.ExportAsJsonAsync(path);
                Assert.That(File.Exists(path), Is.True);
            }
        }
    }
}