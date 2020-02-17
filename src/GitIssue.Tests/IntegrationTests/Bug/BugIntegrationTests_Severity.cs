using System.Linq;
using System.Threading.Tasks;
using GitIssue.Configurations.Bug;
using GitIssue.Values;
using NUnit.Framework;

namespace GitIssue.Tests.IntegrationTests.Bug
{
    [TestFixture]
    public partial class BugIntegrationTests
    {
        [TestFixture]
        public class Severity : BugIntegrationTests
        {
            [Test]
            public async Task CanBeSetFromString()
            {
                Initialize(TestDirectory);
                var create = await Issues
                    .CreateAsync(nameof(CanBeSetFromString), string.Empty)
                    .WithSafeResultAsync();
                Assert.IsTrue(create.IsSuccess);
                var severity = new Enumerated("S1", new[] {"S1", "S2", "S3", "S4", "S5"});
                create.Result.SetSeverity(severity);
                await create.Result.SaveAsync();
                var find = Issues.Find(i => i.Key == create.Result.Key).ToArray();
                var issue = find[0];
                Assert.That(issue.GetSeverity(), Is.EqualTo(severity));
            }
        }
    }
}