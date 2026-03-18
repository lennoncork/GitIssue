using System.Linq;
using System.Threading.Tasks;
using GitIssue.Issues;
using NUnit.Framework;

namespace GitIssue.Tests.IssueManagerTests
{
    [TestFixture]
    public partial class IssueManagerTests
    {
        [TestFixture]
        public class Find : IssueManagerTests
        {
            [TestCase("New Issue")]
            public async Task FindsIssueByTitle(string title)
            {
                this.Initialize(this.TestDirectory);
                SafeResult<IIssue> create = await this.Sut
                    .CreateAsync(title, string.Empty)
                    .WithSafeResultAsync();
                IIssue[] find = this.Sut.Find(i => i.Title == title).ToArray();
                Assert.That(find.Count(), Is.EqualTo(1));
                Assert.That(find[0].Title, Is.EqualTo(title));
            }

            [Test]
            public async Task FindsIssueByKey()
            {
                this.Initialize(this.TestDirectory);
                SafeResult<IIssue> create = await this.Sut
                    .CreateAsync(nameof(Find.FindsIssueByKey), string.Empty)
                    .WithSafeResultAsync();
                IIssue[] find = this.Sut.Find(i => i.Key == create.Result.Key).ToArray();
                Assert.That(find.Count(), Is.EqualTo(1));
                Assert.That(find[0].Key, Is.EqualTo(create.Result.Key));
            }
        }
    }
}