using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GitIssue.Fields;
using GitIssue.Issues;
using NUnit.Framework;

namespace GitIssue.Tests.IssueManagerTests
{
    [TestFixture]
    public partial class IssueManagerTests
    {
        [TestFixture]
        public class FreeLayout : IssueManagerTests
        {
            [Test]
            public async Task FreeIssueCanLiveInAnyFolderUnderTheTypeDirectory()
            {
                this.Initialize(this.TestDirectory);
                IssueConfiguration configuration = IssueConfiguration.Read(this.ConfigFile);
                configuration.Issues.Add(new IssueTypeConfiguration
                {
                    Name = "specification",
                    Layout = IssueLayout.Free,
                    Fields = IssueConfiguration.CreateDefaultFields(),
                });
                configuration.Save(this.ConfigFile);
                IIssueManager manager = IssueManager.Open(this.TestDirectory);

                IIssue created = await manager.CreateAsync("Login flow", "Describe login.", "specification", Array.Empty<IField>(), "auth/login.md");

                string file = Path.Combine(this.IssueDirectory, "specifications", "auth", "login.md");
                Assert.That(created.Key.ToString(), Does.Match(new Regex(@"^specification-[A-F0-9]{8}$")));
                Assert.That(File.Exists(file), Is.True);
                Assert.That(File.ReadAllText(file), Does.Contain(created.Key.ToString()));

                string moved = Path.Combine(this.IssueDirectory, "specifications", "drafts", "login.md");
                Directory.CreateDirectory(Path.GetDirectoryName(moved)!);
                File.Move(file, moved);

                IIssueManager reopened = IssueManager.Open(this.TestDirectory);
                Assert.That(reopened.KeyProvider.Keys.Select(key => key.ToString()), Does.Contain(created.Key.ToString()));
                IIssue? loaded = await this.findAsync(reopened, created.Key);
                Assert.That(loaded, Is.Not.Null);
                Assert.That(loaded!.Title.ToString(), Is.EqualTo("Login flow"));
                Assert.That(reopened.KeyProvider.GetIssuePath(created.Key).Replace('\\', '/'), Does.Contain("specifications/drafts"));

                Assert.That(await reopened.RekeyAsync(), Is.Empty);
                Assert.That(File.Exists(moved), Is.True);
            }

            [Test]
            public async Task MovedTemplateIssueIsFoundByTheKeyInsideTheFile()
            {
                this.Initialize(this.TestDirectory);
                IIssue created = await this.Sut.CreateAsync("Keep me", "Still the same issue.");
                string original = this.documentPath(this.Sut, created.Key);
                string movedDirectory = Path.Combine(this.IssueDirectory, "Issues", "random_folder");
                Directory.CreateDirectory(movedDirectory);
                string moved = Path.Combine(movedDirectory, "issue.md");
                File.Move(original, moved);
                Directory.Delete(Path.GetDirectoryName(original)!, true);

                IIssueManager reopened = IssueManager.Open(this.TestDirectory);
                IIssue? loaded = await this.findAsync(reopened, created.Key);
                Assert.That(loaded, Is.Not.Null);
                Assert.That(loaded!.Title.ToString(), Is.EqualTo("Keep me"));
                Assert.That(reopened.KeyProvider.GetDocumentFileName(created.Key), Is.EqualTo("issue.md"));
            }

            [Test]
            public async Task DuplicateKeysAreRejected()
            {
                this.Initialize(this.TestDirectory);
                IIssue created = await this.Sut.CreateAsync("Once", "Only one copy.");
                string original = this.documentPath(this.Sut, created.Key);
                string copyDirectory = Path.Combine(this.IssueDirectory, "Issues", "copy");
                Directory.CreateDirectory(copyDirectory);
                File.Copy(original, Path.Combine(copyDirectory, "issue.md"));

                IIssueManager reopened = IssueManager.Open(this.TestDirectory);
                Assert.That(
                    () => reopened.KeyProvider.Keys.ToArray(),
                    Throws.InvalidOperationException.With.Message.Contains(created.Key.ToString()));
            }

            [Test]
            public void MarkdownWithoutAKeyHeadingIsNotAnIssue()
            {
                this.Initialize(this.TestDirectory);
                string notes = Path.Combine(this.IssueDirectory, "Issues", "notes.md");
                Directory.CreateDirectory(Path.GetDirectoryName(notes)!);
                File.WriteAllText(notes, "# Notes\n\nThis is not an issue.\n");

                Assert.That(this.Sut.KeyProvider.Keys, Is.Empty);
            }

            private async Task<IIssue?> findAsync(IIssueManager manager, IssueKey key)
            {
                await foreach (IIssue issue in manager.FindAsync(issue => issue.Key == key))
                {
                    return issue;
                }

                return null;
            }

            private string documentPath(IIssueManager manager, IssueKey key)
            {
                string? fileName = manager.KeyProvider.GetDocumentFileName(key) ?? $"{key}.md";
                return Path.Combine(this.IssueDirectory, manager.KeyProvider.GetIssuePath(key), fileName);
            }
        }
    }
}
