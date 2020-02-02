using System.Linq;
using System.Threading.Tasks;
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
                var fixVersion = new[] {"Blah"};
                create.Result.SetFixVersion(fixVersion);
                await create.Result.SaveAsync();
                var find = Issues.Find(i => i.Key == create.Result.Key).ToArray();
                var issue = find[0];
                Assert.That(issue.GetFixVersion(), Is.EqualTo(fixVersion));
            }
        }
    }
}