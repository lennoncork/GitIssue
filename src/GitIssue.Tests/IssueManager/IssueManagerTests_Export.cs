using System.IO;
using System.Threading.Tasks;
using GitIssue.Json;
using NUnit.Framework;

namespace GitIssue.Tests.IssueManager
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
                Initialize(TestDirectory);
                var issue1 = await Sut
                    .CreateAsync(nameof(ExportsFileToJson))
                    .WithSafeResultAsync();
                var issue2 = await Sut
                    .CreateAsync(nameof(ExportsFileToJson))
                    .WithSafeResultAsync();
                var path = Path.Combine(TestDirectory, "export.json");
                await Sut.ExportAsJsonAsync(path);
                Assert.That(File.Exists(path), Is.True);
            }
        }
    }
}