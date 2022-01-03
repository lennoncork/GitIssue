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
                    .WithSafeResultAsync()
                    .AssertIfNotSuccess();

                var fixVersion = Version.Parse("1.2.3-abs+def");
                create.SetFixVersion(new[] { fixVersion });

                await create
                    .SaveAsync()
                    .WithSafeResultAsync()
                    .AssertIfNotSuccess();

                var find = Issues.Find(i => i.Key == create.Key).ToArray();

                var issue = find[0];
                Assert.That(issue.GetFixVersion()[0], Is.EqualTo(fixVersion));
            }
        }
    }
}