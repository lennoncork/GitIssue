using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GitIssue.Issues;
using NUnit.Framework;

namespace GitIssue.Tests.IssueManagerTests
{
    [TestFixture]
    public partial class IssueManagerTests
    {
        [TestFixture]
        public class Rekey : IssueManagerTests
        {
            [Test]
            public async Task RegeneratesKeysFromUpdatedTemplate()
            {
                this.Initialize(this.TestDirectory);
                IIssue issue = await this.Sut.CreateAsync("Add login page", "Feature work");
                string original = issue.Key.ToString();
                string originalPath = Path.Combine(this.IssueDirectory, this.Sut.KeyProvider.GetIssuePath(issue.Key));
                Assert.That(original, Does.Match(new Regex(@"^\d{4}-\d{2}-\d{2}-[A-F0-9]{8}$")));

                this.Sut.Configuration.IssueType = "FEAT";
                this.Sut.Configuration.KeyTemplate = "{type}-{uid}-{slug}";

                IReadOnlyList<IssueRekeyResult> dryRun = this.Sut.Rekey(true);
                Assert.That(dryRun, Has.Count.EqualTo(1));
                Assert.That(dryRun[0].From.ToString(), Is.EqualTo(original));
                Assert.That(Directory.Exists(originalPath), Is.True);

                IReadOnlyList<IssueRekeyResult> results = this.Sut.Rekey();
                Assert.That(results, Has.Count.EqualTo(1));
                Assert.That(results[0].To.ToString(), Does.Match(new Regex(@"^FEAT-[A-F0-9]{8}-add-login-page$")));
                Assert.That(results[0].To.ToString(), Does.Contain(original[^8..]));

                IIssue[] found = this.Sut.Find(_ => true).ToArray();
                Assert.That(found, Has.Length.EqualTo(1));
                Assert.That(found[0].Key, Is.EqualTo(results[0].To));
                Assert.That(found[0].Title, Is.EqualTo("Add login page"));
                Assert.That(Directory.Exists(Path.Combine(this.IssueDirectory, this.Sut.KeyProvider.GetIssuePath(found[0].Key))), Is.True);
            }

            [Test]
            public async Task PreservesExistingIssueTypesWhenRekeying()
            {
                this.Initialize(this.TestDirectory);
                this.Sut.Configuration.KeyTemplate = "{type}-{uid}-{slug}";
                this.Sut.Configuration.IssueType = "FEAT";
                IIssue feature = await this.Sut.CreateAsync("Feature", string.Empty);
                this.Sut.Configuration.IssueType = "BUG";
                IIssue bug = await this.Sut.CreateAsync("Bug", string.Empty);

                feature.Title = "Updated feature";
                await feature.SaveAsync();
                bug.Title = "Updated bug";
                await bug.SaveAsync();
                this.Sut.Configuration.IssueType = "TASK";

                IReadOnlyList<IssueRekeyResult> results = this.Sut.Rekey();

                Assert.That(results, Has.Count.EqualTo(2));
                Assert.That(results.Any(result => result.To.ToString().StartsWith("FEAT-")), Is.True);
                Assert.That(results.Any(result => result.To.ToString().StartsWith("BUG-")), Is.True);
            }

            [Test]
            public async Task RegeneratesNamedKeysFromDateKeys()
            {
                this.Initialize(this.TestDirectory);
                IIssue issue = await this.Sut.CreateAsync("Add login page", "Feature work");
                string original = issue.Key.ToString();
                string uid = original[^8..];

                this.Sut.Configuration.KeyProvider = GitIssue.Values.TypeValue.Create<NameIssueKeyProvider>();
                this.Sut.Configuration.KeyTemplate = IssueKeyTemplate.Name;

                IReadOnlyList<IssueRekeyResult> results = this.Sut.Rekey();
                Assert.That(results, Has.Count.EqualTo(1));
                Assert.That(results[0].To.ToString(), Is.EqualTo($"{uid} - Add login page"));
                Assert.That(this.Sut.Find(_ => true).Single().Title, Is.EqualTo("Add login page"));
            }

            [Test]
            public async Task DryRunDoesNotRenameFiles()
            {
                this.Initialize(this.TestDirectory);
                IIssue issue = await this.Sut.CreateAsync("Keep path", string.Empty);
                string path = Path.Combine(this.IssueDirectory, this.Sut.KeyProvider.GetIssuePath(issue.Key));

                this.Sut.Configuration.KeyTemplate = "{uid}-{slug}";
                IReadOnlyList<IssueRekeyResult> dryRun = await this.Sut.RekeyAsync(true);

                Assert.That(dryRun, Has.Count.EqualTo(1));
                Assert.That(dryRun[0].From, Is.EqualTo(issue.Key));
                Assert.That(Directory.Exists(path), Is.True);
                Assert.That(
                    ((TemplateIssueKeyProvider)this.Sut.KeyProvider).FindExisting().Single().Key,
                    Is.EqualTo(issue.Key));
            }
        }
    }
}
