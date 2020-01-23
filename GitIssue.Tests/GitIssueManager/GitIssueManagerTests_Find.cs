using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace GitIssue.Tests.GitIssueManager
{
    [TestFixture]
    public partial class GitIssueManagerTests
    {
        [TestFixture]
        public class Find : GitIssueManagerTests
        {
            [Test]
            public async Task FindsIssueByKey()
            {
                this.Initialize(this.TestDirectory);
                var create = await this.Sut
                    .CreateAsync(nameof(FindsIssueByKey), string.Empty)
                    .WithSafeResult();
                var find = this.Sut.Find(i => i.Key == create.Result.Key).ToArray();
                Assert.That(find.Count(), Is.EqualTo(1));
                Assert.That(find[0].Key, Is.EqualTo(create.Result.Key));
            }


            [TestCase("New Issue")]
            public async Task FindsIssueByTitle(string title)
            {
                this.Initialize(this.TestDirectory);
                var create = await this.Sut
                    .CreateAsync(title, string.Empty)
                    .WithSafeResult();
                var find = this.Sut.Find(i => i.Title == title).ToArray();
                Assert.That(find.Count(), Is.EqualTo(1));
                Assert.That(find[0].Title, Is.EqualTo(title));
            }
        }
    }
}
