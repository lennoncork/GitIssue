using System;
using System.IO;
using System.Threading.Tasks;
using GitIssue.Tool.Commands.Edit;
using LibGit2Sharp;
using NUnit.Framework;
using Serilog.Core;

namespace GitIssue.Tool.Tests.Integration
{
    [TestFixture]
    public class EditCommandTests : IntegrationTestBase
    {
        private sealed class RecordingProcessRunner : IProcessRunner
        {
            public string[] Arguments { get; private set; } = Array.Empty<string>();

            public string FileName { get; private set; } = string.Empty;

            public Task<int> RunAsync(string fileName, params string[] arguments)
            {
                this.FileName = fileName;
                this.Arguments = arguments;
                return Task.FromResult(0);
            }
        }

        [Test]
        public void Edit_UpdatesIssueField()
        {
            this.InitializeRepository();
            string key = this.CreateIssue("Original title");

            int exitCode = this.Run("Edit", key, "Title", "Updated title");

            var issue = this.ReadIssue(key);
            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.Zero, this.Error);
                this.AssertIssueExists(key);
                this.AssertJsonValue(issue, "Title", "Updated title");
            });
        }

        [Test]
        public async Task Edit_UsesIssuesEditorWhenEditorOptionIsDefault()
        {
            this.InitializeRepository();
            string key = this.CreateIssue("Original title");
            this.SetGitConfig("issues.editor", "vim");

            RecordingProcessRunner runner = await this.EditField(new EditOptions
            {
                Key = key,
                Field = "Title",
            });

            Assert.Multiple(() =>
            {
                Assert.That(runner.FileName, Is.EqualTo("vim"));
                Assert.That(runner.Arguments, Has.Length.EqualTo(1));
                Assert.That(File.Exists(runner.Arguments[0]), Is.True);
                Assert.That(runner.Arguments[0], Does.Not.Contain("vim"));
                this.AssertJsonValue(this.ReadIssue(key), "Title", "Original title");
            });
        }

        [Test]
        public async Task Edit_PrefersEditorOptionOverIssuesEditor()
        {
            this.InitializeRepository();
            string key = this.CreateIssue("Original title");
            this.SetGitConfig("issues.editor", "vim");

            RecordingProcessRunner runner = await this.EditField(new EditOptions
            {
                Key = key,
                Field = "Title",
                Editor = "nano",
            });

            Assert.Multiple(() =>
            {
                Assert.That(runner.FileName, Is.EqualTo("nano"));
                Assert.That(runner.Arguments, Has.Length.EqualTo(1));
                Assert.That(File.Exists(runner.Arguments[0]), Is.True);
                this.AssertJsonValue(this.ReadIssue(key), "Title", "Original title");
            });
        }

        [Test]
        public async Task Edit_KeepsConfigurationEditorWhenNoEditorIsConfigured()
        {
            this.InitializeRepository();
            string key = this.CreateIssue("Original title");

            RecordingProcessRunner runner = await this.EditField(
                new EditOptions
                {
                    Key = key,
                    Field = "Title",
                },
                new Configuration
                {
                    Editor = "custom-editor",
                    Arguments = "--stay",
                });

            Assert.Multiple(() =>
            {
                Assert.That(runner.FileName, Is.EqualTo("custom-editor"));
                Assert.That(runner.Arguments, Has.Length.EqualTo(2));
                Assert.That(runner.Arguments[0], Is.EqualTo("--stay"));
                Assert.That(File.Exists(runner.Arguments[1]), Is.True);
                Assert.That(runner.Arguments[1], Does.Not.StartWith("--stay"));
                this.AssertJsonValue(this.ReadIssue(key), "Title", "Original title");
            });
        }

        private async Task<RecordingProcessRunner> EditField(EditOptions options, Configuration? configuration = null)
        {
            RecordingProcessRunner runner = new RecordingProcessRunner();
            Editor editor = new Editor(configuration ?? new Configuration(), runner);
            await using IIssueManager manager = IssueManager.Open(this.TestDirectory);
            EditCommand command = new EditCommand(manager, editor, Logger.None);
            await command.Exec(options);
            return runner;
        }

        private void SetGitConfig(string key, string value)
        {
            using Repository repository = new Repository(this.TestDirectory);
            repository.Config.Set(key, value);
        }
    }
}
