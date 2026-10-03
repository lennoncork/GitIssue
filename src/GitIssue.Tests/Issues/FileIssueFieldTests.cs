using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GitIssue.Fields;
using GitIssue.Issues;
using GitIssue.Issues.File;
using NUnit.Framework;
using StringValue = GitIssue.Values.String;

namespace GitIssue.Tests.Issues
{
    [TestFixture]
    public class FileIssueFieldTests
    {
        private IssueRoot CreateIssueRoot()
        {
            string repository = Helpers.CreateTempDirectory();
            RepositoryRoot repositoryRoot = RepositoryRoot.Open(repository, null);
            IssueRoot issueRoot = new IssueRoot(repositoryRoot, IssueKey.Create("issue"), "issue");
            Directory.CreateDirectory(issueRoot.IssuePath);
            return issueRoot;
        }

        [Test]
        public async Task ArrayField_RoundTripsInOrderAndRemovesStaleValues()
        {
            IssueRoot root = this.CreateIssueRoot();
            FieldKey key = FieldKey.Create("values");
            FileArrayField<StringValue> field = new FileArrayField<StringValue>(root, key,
                new[] { StringValue.Parse("one"), StringValue.Parse("two"), StringValue.Parse("three") });

            await field.SaveAsync();
            field.Values = new[] { StringValue.Parse("replacement") };
            await field.SaveAsync();
            IField result = await FileArrayField<StringValue>.ReadAsync(root, key, new FieldInfo());

            Assert.That(((FileArrayField<StringValue>)result).Values,
                Is.EqualTo(new[] { StringValue.Parse("replacement") }));
            Assert.That(Directory.EnumerateFiles(field.DirectoryPath).Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task ValueField_RoundTrips()
        {
            IssueRoot root = this.CreateIssueRoot();
            FieldKey key = FieldKey.Create("value");
            FileValueField<StringValue> field = new FileValueField<StringValue>(root, key, StringValue.Parse("saved"));

            await field.SaveAsync();
            IField result = await FileValueField<StringValue>.ReadAsync(root, key, new FieldInfo());

            Assert.That(((FileValueField<StringValue>)result).Value, Is.EqualTo(StringValue.Parse("saved")));
        }
    }
}