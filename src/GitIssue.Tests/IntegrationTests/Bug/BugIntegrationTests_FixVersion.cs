using System.Linq;
using System.Threading.Tasks;
using GitIssue.Issues;
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
                this.Initialize(this.TestDirectory);

                IIssue create = await this.Issues
                    .CreateAsync(nameof(FixVersion.CanBeSetFromString), string.Empty)
                    .WithSafeResultAsync()
                    .AssertIfNotSuccess();

                Version fixVersion = Version.Parse("1.2.3-abs+def");
                create.SetFixVersion(new[] { fixVersion });

                await create
                    .SaveAsync()
                    .WithSafeResultAsync()
                    .AssertIfNotSuccess();

                IIssue[] find = this.Issues.Find(i => i.Key == create.Key).ToArray();

                IIssue issue = find[0];
                Assert.That(issue.GetFixVersion()[0], Is.EqualTo(fixVersion));
            }
        }
    }
}