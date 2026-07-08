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
        public class Severity : BugIntegrationTests
        {
            [Test]
            public async Task CanBeSetFromString()
            {
                this.Initialize(this.TestDirectory);
                SafeResult<IIssue> create = await this.Issues
                    .CreateAsync(nameof(Severity.CanBeSetFromString), string.Empty)
                    .WithSafeResultAsync();
                Assert.IsTrue(create.IsSuccess);
                Enumerated severity = new Enumerated("S1", new[] { "S1", "S2", "S3", "S4", "S5" });
                create.Result.SetSeverity(severity);
                await create.Result.SaveAsync();
                IIssue[] find = this.Issues.Find(i => i.Key == create.Result.Key).ToArray();
                IIssue issue = find[0];
                Assert.That(issue.GetSeverity(), Is.EqualTo(severity));
            }
        }
    }
}