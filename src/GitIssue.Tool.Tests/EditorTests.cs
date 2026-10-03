using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GitIssue.Fields;
using GitIssue.Issues.Json;
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
                File.SetLastWriteTime(file, DateTime.Now.AddSeconds(1));
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
                File.SetLastWriteTime(file, DateTime.Now.AddSeconds(1));
            });
            Editor editor = new Editor(new Configuration { Editor = "test-editor", Arguments = string.Empty }, runner);
            JsonValueField<StringValue> field = new JsonValueField<StringValue>(FieldKey.Create("Title"), StringValue.Parse("original"));

            await editor.Open(field);

            Assert.That(field.Value, Is.EqualTo(StringValue.Parse("updated")));
        }
    }
}