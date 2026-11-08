using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GitIssue.Exceptions;
using GitIssue.Fields;
using GitIssue.Issues;
using GitIssue.Issues.Json;
using GitIssue.Issues.Markdown;
using GitIssue.Values;
using NUnit.Framework;
using StringValue = GitIssue.Values.String;

namespace GitIssue.Tool.Tests
{
    [TestFixture]
    public class EditorTests
    {
        private sealed class TestProcessRunner : IProcessRunner
        {
            private readonly Func<string, Task>? updateFile;

            public TestProcessRunner(int exitCode, Func<string, Task>? updateFile = null)
            {
                this.ExitCode = exitCode;
                this.updateFile = updateFile;
            }

            public string Arguments { get; private set; } = string.Empty;

            public string FileName { get; private set; } = string.Empty;

            public int ExitCode { get; }

            public async Task<int> RunAsync(string fileName, string arguments)
            {
                this.FileName = fileName;
                this.Arguments = arguments;
                if (this.updateFile != null)
                {
                    await this.updateFile(arguments.Trim());
                }

                return this.ExitCode;
            }
        }

        [Test]
        public async Task Edit_ReturnsUpdatedContentWhenProcessSucceeds()
        {
            TestProcessRunner runner = new TestProcessRunner(0, async file =>
            {
                await File.WriteAllTextAsync(file, "updated");
                File.SetLastWriteTime(file, System.DateTime.Now.AddSeconds(1));
            });
            Editor editor = new Editor(new Configuration { Editor = "test-editor", Arguments = string.Empty }, runner);

            string result = await editor.Edit("Update title", "original");

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo("updated"));
                Assert.That(runner.FileName, Is.EqualTo("test-editor"));
                Assert.That(File.Exists(runner.Arguments.Trim()), Is.True);
            });
        }

        [Test]
        public async Task Edit_ReturnsOriginalContentWhenProcessFails()
        {
            Editor editor = new Editor(new Configuration { Editor = "test-editor", Arguments = string.Empty }, new TestProcessRunner(1));

            string result = await editor.Edit("Update title", "original");

            Assert.That(result, Is.EqualTo("original"));
        }

        [Test]
        public async Task Open_UpdatesFieldWhenProcessSucceeds()
        {
            TestProcessRunner runner = new TestProcessRunner(0, async file =>
            {
                await File.WriteAllTextAsync(file, "updated");
                File.SetLastWriteTime(file, System.DateTime.Now.AddSeconds(1));
            });
            Editor editor = new Editor(new Configuration { Editor = "test-editor", Arguments = string.Empty }, runner);
            JsonValueField<StringValue> field = new JsonValueField<StringValue>(FieldKey.Create("Title"), StringValue.Parse("original"));

            await editor.Open(field);

            Assert.That(field.Value, Is.EqualTo(StringValue.Parse("updated")));
        }

        [Test]
        public void Open_LeavesMarkdownIssueUnchangedWhenEditedDocumentIsInvalid()
        {
            string directory = Path.Combine(Path.GetTempPath(), "gitissue-editor-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            RepositoryRoot root = RepositoryRoot.Open(directory, null);
            IssueKey key = IssueKey.Create("ISSUE-1");
            MarkdownIssue issue = new MarkdownIssue(new IssueRoot(root, key, "issue"), new IssueConfiguration().Fields);
            issue.Title = "Markdown issue";
            issue.Description = "Body";
            issue.Author = Signature.Parse("Ada Lovelace <ada@example.com>");
            issue.Created = System.DateTime.Parse("2026-11-08T03:25:00Z");
            issue.Updated = issue.Created;
            TestProcessRunner runner = new TestProcessRunner(0, async file =>
            {
                string text = await File.ReadAllTextAsync(file);
                await File.WriteAllTextAsync(file, text.Replace("Ada Lovelace <ada@example.com>", "Ada", System.StringComparison.Ordinal));
                File.SetLastWriteTime(file, System.DateTime.Now.AddSeconds(1));
            });
            Editor editor = new Editor(new Configuration { Editor = "test-editor", Arguments = string.Empty }, runner);

            Assert.That(async () => await editor.Open(issue), Throws.TypeOf<IssueValidationException>());
            Assert.That(issue.Author, Is.EqualTo(Signature.Parse("Ada Lovelace <ada@example.com>")));
        }
    }
}
