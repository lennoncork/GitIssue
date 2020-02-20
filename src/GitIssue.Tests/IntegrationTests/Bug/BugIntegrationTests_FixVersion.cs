using System.Linq;
using System.Threading.Tasks;
using GitIssue.Values;
using NUnit.Framework;

namespace GitIssue.Tests.IntegrationTests.Bug
{
    [TestFixture]
    public partial class BugIntegrationTests
    {
        [TestFixture]
        public class FixVersion : BugIntegrationTests
        {
            [Test]
            public async Task CanBeSetFromString()
            {
                Initialize(TestDirectory);
                var create = await Issues
                    .CreateAsync(nameof(CanBeSetFromString), string.Empty)
                    .WithSafeResultAsync();
                Assert.IsTrue(create.IsSuccess);
                var fixVersion = Version.Parse("1.2.3-abs+def");
                create.Result.SetFixVersion(new[] {fixVersion});
                await create.Result.SaveAsync();
                var find = Issues.Find(i => i.Key == create.Result.Key).ToArray();
                var issue = find[0];
                Assert.That(issue.GetFixVersion()[0], Is.EqualTo(fixVersion));
            }
        }
    }
}