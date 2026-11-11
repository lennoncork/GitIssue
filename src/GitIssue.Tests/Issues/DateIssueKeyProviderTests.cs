using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using GitIssue.Issues;
using NUnit.Framework;

namespace GitIssue.Tests.Issues
{
    [TestFixture]
    public class DateIssueKeyProviderTests
    {
        private static readonly Regex dateKey = new Regex(@"^\d{4}-\d{2}-\d{2}-[A-F0-9]{8}$", RegexOptions.Compiled);

        private RepositoryRoot root = default;

        private DateIssueKeyProvider provider = null!;

        [SetUp]
        public void Setup()
        {
            string directory = Helpers.CreateTempDirectory();
            this.root = RepositoryRoot.Create(directory, Paths.IssueRootFolderName);
            this.provider = new DateIssueKeyProvider(this.root);
        }

        [Test]
        public void Next_CreatesDateBasedKey()
        {
            IssueKey key = this.provider.Next();
            DateTime today = DateTime.Now;

            Assert.That(key.ToString(), Does.Match(DateIssueKeyProviderTests.dateKey));
            Assert.That(key.ToString(), Does.StartWith($"{today.Year:D4}-{today.Month:D2}-{today.Day:D2}-"));
        }

        [Test]
        public void Next_IgnoresSuppliedName()
        {
            IssueKey key = this.provider.Next("Add login page");

            Assert.That(key.ToString(), Does.Match(DateIssueKeyProviderTests.dateKey));
            Assert.That(key.ToString(), Does.Not.Contain("Add login page"));
        }

        [Test]
        public void Next_CreatesUniqueKeys()
        {
            IssueKey first = this.provider.Next();
            IssueKey second = this.provider.Next();

            Assert.That(first, Is.Not.EqualTo(second));
        }

        [Test]
        public void GetIssuePath_MapsHyphensToDirectories()
        {
            IssueKey key = IssueKey.Create("2026-11-09-ABCD1234");

            Assert.That(this.provider.GetIssuePath(key), Is.EqualTo(Path.Combine("2026", "11", "09", "ABCD1234")));
        }

        [Test]
        public void TryGetKey_ParsesDateKey()
        {
            string path = Path.Combine(this.root.IssuesPath, "2026", "11", "09", "ABCD1234");
            Directory.CreateDirectory(path);

            Assert.That(this.provider.TryGetKey("2026-11-09-ABCD1234", out IssueKey key), Is.True);
            Assert.That(key.ToString(), Is.EqualTo("2026-11-09-ABCD1234"));
        }

        [Test]
        public void TryGetKey_RejectsInvalidKey()
        {
            Assert.That(this.provider.TryGetKey("not-a-date-key", out IssueKey key), Is.False);
            Assert.That(key, Is.EqualTo(IssueKey.None));
        }

        [Test]
        public void Keys_FindsExistingDateIssues()
        {
            string path = Path.Combine(this.root.IssuesPath, "2026", "11", "09", "ABCD1234");
            Directory.CreateDirectory(path);

            IssueKey[] keys = this.provider.Keys.ToArray();

            Assert.That(keys.Select(key => key.ToString()), Is.EqualTo(new[] { "2026-11-09-ABCD1234" }));
        }

        [Test]
        public void FindExisting_FindsIssueFilesWhenTheTemplateNoLongerMatches()
        {
            string path = Path.Combine(this.root.IssuesPath, "2026", "11", "09", "ABCD1234");
            Directory.CreateDirectory(path);
            File.WriteAllText(Path.Combine(path, "2026-11-09-ABCD1234.json"), "{}");

            IssueConfiguration configuration = new IssueConfiguration
            {
                KeyTemplate = IssueKeyTemplate.Name,
            };
            TemplateIssueKeyProvider renamed = new TemplateIssueKeyProvider(this.root, configuration);

            Assert.That(renamed.Keys, Is.Empty);
            Assert.That(
                renamed.FindExisting().Select(item => item.Key.ToString()),
                Is.EqualTo(new[] { "2026-11-09-ABCD1234" }));
        }
    }
}
