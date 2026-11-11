using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using GitIssue.Issues;
using NUnit.Framework;

namespace GitIssue.Tests.Issues
{
    [TestFixture]
    public class NameIssueKeyProviderTests
    {
        private static readonly Regex namedKey = new Regex(@"^[A-F0-9]{8} - .+$", RegexOptions.Compiled);

        private RepositoryRoot root = default;

        private NameIssueKeyProvider provider = null!;

        [SetUp]
        public void Setup()
        {
            string directory = Helpers.CreateTempDirectory();
            this.root = RepositoryRoot.Create(directory, Paths.IssueRootFolderName);
            this.provider = new NameIssueKeyProvider(this.root);
        }

        [Test]
        public void Next_UsesIssueName()
        {
            IssueKey key = this.provider.Next("Add login page");

            Assert.That(key.ToString(), Does.Match(NameIssueKeyProviderTests.namedKey));
            Assert.That(key.ToString(), Does.EndWith(" - Add login page"));
        }

        [Test]
        public void Next_WithoutName_UsesUntitled()
        {
            IssueKey key = this.provider.Next();

            Assert.That(key.ToString(), Does.EndWith(" - Untitled"));
        }

        [Test]
        public void Next_SanitizesInvalidFileNameCharacters()
        {
            IssueKey key = this.provider.Next("Fix: path/to\\file?");

            Assert.That(key.ToString(), Does.EndWith(" - Fix path to file"));
            Assert.That(key.ToString(), Does.Not.Contain("/"));
            Assert.That(key.ToString(), Does.Not.Contain("\\"));
            Assert.That(key.ToString(), Does.Not.Contain(":"));
            Assert.That(key.ToString(), Does.Not.Contain("?"));
        }

        [Test]
        public void Next_CollapsesWhitespace()
        {
            IssueKey key = this.provider.Next("  Add   login   page  ");

            Assert.That(key.ToString(), Does.EndWith(" - Add login page"));
        }

        [Test]
        public void Next_UsesUntitledWhenNameIsEmpty()
        {
            IssueKey key = this.provider.Next("   ");

            Assert.That(key.ToString(), Does.EndWith(" - Untitled"));
        }

        [Test]
        public void Next_CreatesUniqueKeysForTheSameName()
        {
            IssueKey first = this.provider.Next("Add login page");
            IssueKey second = this.provider.Next("Add login page");

            Assert.That(first, Is.Not.EqualTo(second));
            Assert.That(first.ToString(), Does.EndWith(" - Add login page"));
            Assert.That(second.ToString(), Does.EndWith(" - Add login page"));
        }

        [Test]
        public void GetIssuePath_UsesTheKeyAsASingleDirectory()
        {
            IssueKey key = IssueKey.Create("ABCD1234 - Add login page");

            Assert.That(this.provider.GetIssuePath(key), Is.EqualTo("ABCD1234 - Add login page"));
        }

        [Test]
        public void TryGetKey_ParsesNamedKey()
        {
            Assert.That(this.provider.TryGetKey("ABCD1234 - Add login page", out IssueKey key), Is.True);
            Assert.That(key.ToString(), Is.EqualTo("ABCD1234 - Add login page"));
        }

        [Test]
        public void TryGetKey_FindsIssueByUniqueId()
        {
            string key = "ABCD1234 - Add login page";
            Directory.CreateDirectory(Path.Combine(this.root.IssuesPath, key));

            Assert.That(this.provider.TryGetKey("ABCD1234", out IssueKey found), Is.True);
            Assert.That(found.ToString(), Is.EqualTo(key));
        }

        [Test]
        public void TryGetKey_RejectsInvalidKey()
        {
            Assert.That(this.provider.TryGetKey("not a named key", out IssueKey key), Is.False);
            Assert.That(key, Is.EqualTo(IssueKey.None));
        }

        [Test]
        public void Keys_FindsExistingNamedIssues()
        {
            string key = "ABCD1234 - Add login page";
            Directory.CreateDirectory(Path.Combine(this.root.IssuesPath, key));
            Directory.CreateDirectory(Path.Combine(this.root.IssuesPath, "other"));

            IssueKey[] keys = this.provider.Keys.ToArray();

            Assert.That(keys.Select(item => item.ToString()), Is.EqualTo(new[] { key }));
        }
    }
}
