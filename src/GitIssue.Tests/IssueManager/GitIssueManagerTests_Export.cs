using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GitIssue.Json;
using NUnit.Framework;

namespace GitIssue.Tests.GitIssueManager
{
    [TestFixture]
    public partial class GitIssueManagerTests
    {
        [TestFixture]
        public class Export : GitIssueManagerTests
        {
            [Test]
            public async Task ExportsFileToJson()
            {
                this.Initialize(this.TestDirectory);
                var issue1 = await this.Sut
                    .CreateAsync(nameof(ExportsFileToJson))
                    .WithSafeResultAsync();
                var issue2 = await this.Sut
                    .CreateAsync(nameof(ExportsFileToJson))
                    .WithSafeResultAsync();
                string path = Path.Combine(this.TestDirectory, "export.json");
                await this.Sut.ExportAsJsonAsync(path);
                Assert.That(File.Exists(path), Is.True);
            }
        }
    }
}
