using System.Collections.Generic;
using System.Threading.Tasks;
using GitIssue;
using GitIssue.Fields;
using GitIssue.Issues;
using GitIssue.Tool;
using GitIssue.Tool.Commands.Create;
using NUnit.Framework;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace GitIssue.Tool.Tests.Integration
{
    [TestFixture]
    public class CreateCommandTests : IntegrationTestBase
    {
        [Test]
        public void Create_PersistsNewIssue()
        {
            this.InitializeRepository();

            int exitCode = this.Run("Create", "Integration title", "Integration description");

            string[] keys = this.GetIssueKeys();
            Assert.That(keys, Has.Length.EqualTo(1));
            string key = keys[0];
            var issue = this.ReadIssue(key);
            var tracking = this.ReadJsonObject(this.TrackingFile);
            Assert.Multiple(() =>
            {
                Assert.That(exitCode, Is.Zero, this.Error);
                this.AssertIssueExists(key);
                this.AssertJsonValue(issue, "Key", key);
                this.AssertJsonValue(issue, "Title", "Integration title");
                this.AssertJsonValue(issue, "Description", "Integration description");
                this.AssertFileExists(this.TrackingFile);
                this.AssertJsonValue(tracking, "Key", key);
                Assert.That(this.Output, Does.Contain("Created and tracking new issue"));
            });
        }

        [Test]
        public async Task Create_EmptyEditorTitle_DoesNotCreateIssue()
        {
            this.InitializeRepository();

            string log = await this.ExecInteractiveAsync(new ScriptedEditor(new Dictionary<string, string>
            {
                ["Title"] = string.Empty,
            }));

            Assert.Multiple(() =>
            {
                Assert.That(this.GetIssueKeys(), Is.Empty);
                this.AssertFileDoesNotExist(this.TrackingFile);
                Assert.That(log, Does.Contain("A valid title must be provided"));
            });
        }

        [Test]
        public async Task Create_SavesEditorSuppliedFields()
        {
            this.InitializeRepository();

            await this.ExecInteractiveAsync(new ScriptedEditor(new Dictionary<string, string>
            {
                ["Title"] = "Editor title",
                ["Description"] = "Editor description",
                ["Labels"] = "+regression",
                ["Comments"] = "+Saved from the editor",
            }));

            string[] keys = this.GetIssueKeys();
            Assert.That(keys, Has.Length.EqualTo(1));
            string key = keys[0];
            var issue = this.ReadIssue(key);
            var tracking = this.ReadJsonObject(this.TrackingFile);
            Assert.Multiple(() =>
            {
                this.AssertIssueExists(key);
                this.AssertJsonValue(issue, "Title", "Editor title");
                this.AssertJsonValue(issue, "Description", "Editor description");
                this.AssertJsonArrayContains(issue, "Labels", "regression");
                this.AssertJsonArrayContains(issue, "Comments", "Saved from the editor");
                this.AssertFileExists(this.TrackingFile);
                this.AssertJsonValue(tracking, "Key", key);
            });
        }

        private async Task<string> ExecInteractiveAsync(IEditor editor)
        {
            CollectingSink sink = new CollectingSink();
            using IIssueManager manager = IssueManager.Open(this.TestDirectory);
            using Logger logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Sink(sink)
                .CreateLogger();
            CreateCommand command = new CreateCommand(manager, manager.Configuration, editor, logger);
            await command.Exec(new CreateOptions
            {
                Path = this.TestDirectory,
            });
            return string.Join('\n', sink.Messages);
        }

        private sealed class CollectingSink : ILogEventSink
        {
            public List<string> Messages { get; } = new List<string>();

            public void Emit(LogEvent logEvent)
            {
                this.Messages.Add(logEvent.RenderMessage());
            }
        }

        private sealed class ScriptedEditor : IEditor
        {
            private readonly Dictionary<string, string> updates;

            public ScriptedEditor(Dictionary<string, string> updates)
            {
                this.updates = updates;
            }

            public Task<string> Edit(string header, string content)
            {
                return Task.FromResult(content);
            }

            public Task Open(IIssue issue)
            {
                return Task.CompletedTask;
            }

            public Task Open(IEnumerable<IField> fields)
            {
                foreach (IField field in fields)
                {
                    if (this.updates.TryGetValue(field.Key, out string? update) && update != null)
                    {
                        field.Update(update);
                    }
                }

                return Task.CompletedTask;
            }

            public Task Open(IField field)
            {
                return Task.CompletedTask;
            }
        }
    }
}
