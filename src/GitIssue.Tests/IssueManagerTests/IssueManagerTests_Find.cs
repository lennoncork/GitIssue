using System.Linq;
using System.Threading.Tasks;
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
                Initialize(TestDirectory);
                var create = await Sut
                    .CreateAsync(title, string.Empty)
                    .WithSafeResultAsync();
                var find = Sut.Find(i => i.Title == title).ToArray();
                Assert.That(find.Count(), Is.EqualTo(1));
                Assert.That(find[0].Title, Is.EqualTo(title));
            }

            [Test]
            public async Task FindsIssueByKey()
            {
                Initialize(TestDirectory);
                var create = await Sut
                    .CreateAsync(nameof(FindsIssueByKey), string.Empty)
                    .WithSafeResultAsync();
                var find = Sut.Find(i => i.Key == create.Result.Key).ToArray();
                Assert.That(find.Count(), Is.EqualTo(1));
                Assert.That(find[0].Key, Is.EqualTo(create.Result.Key));
            }
        }
    }
}