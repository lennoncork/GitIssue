using System.IO;
using System.Threading.Tasks;
using GitIssue.Issues;
using NUnit.Framework;

namespace GitIssue.Tests.IssueManagerTests
{
    [TestFixture]
    public partial class IssueManagerTests
    {
        [TestFixture]
        public class Delete : IssueManagerTests
        {
            [Test]
            public async Task DeletesExistingIssue()
            {
                this.Initialize(this.TestDirectory);
                SafeResult<IIssue> create = await this.Sut
                    .CreateAsync(nameof(Delete.DeletesExistingIssue), string.Empty)
                    .WithSafeResultAsync();
                Assert.IsTrue(create.IsSuccess);
                Assert.IsTrue(Directory.Exists(Path.Combine(this.IssueDirectory, this.Sut.KeyProvider.GetIssuePath(create.Result.Key))));
                SafeResult<bool> delete = await this.Sut
                    .DeleteAsync(create.Result.Key)
                    .WithSafeResultAsync();
                Assert.IsTrue(delete.IsSuccess);
                Assert.IsFalse(Directory.Exists(Path.Combine(this.IssueDirectory, this.Sut.KeyProvider.GetIssuePath(create.Result.Key))));
            }

            [Test]
            public async Task FailsIfIssueDoesNotExist()
            {
                this.Initialize(this.TestDirectory);
                SafeResult<bool> delete = await this.Sut
                    .DeleteAsync(this.Sut.KeyProvider.Next())
                    .WithSafeResultAsync();
                Assert.IsFalse(delete.IsSuccess);
            }

            [Test]
            public void DeleteByKey_DeletesExistingIssue()
            {
                this.Initialize(this.TestDirectory);
                IIssue issue = this.Sut.Create(nameof(Delete.DeleteByKey_DeletesExistingIssue), string.Empty);
                string path = Path.Combine(this.IssueDirectory, this.Sut.KeyProvider.GetIssuePath(issue.Key));
                Assert.IsTrue(Directory.Exists(path));
                bool result = this.Sut.Delete(issue.Key);
                Assert.That(result, Is.True);
                Assert.IsFalse(Directory.Exists(path));
            }

            [Test]
            public void DeleteById_DeletesExistingIssue()
            {
                this.Initialize(this.TestDirectory);
                IIssue issue = this.Sut.Create(nameof(Delete.DeleteById_DeletesExistingIssue), string.Empty);
                string id = this.Sut.KeyProvider.GetIssuePath(issue.Key);
                string path = Path.Combine(this.IssueDirectory, id);
                Assert.IsTrue(Directory.Exists(path));
                bool result = this.Sut.Delete(id);
                Assert.That(result, Is.True);
                Assert.IsFalse(Directory.Exists(path));
            }

            [Test]
            public void DeleteById_ReturnsFalseWhenKeyNotFound()
            {
                this.Initialize(this.TestDirectory);
                bool result = this.Sut.Delete("invalid-id");
                Assert.That(result, Is.False);
            }
        }
    }
}