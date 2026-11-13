using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GitIssue.Exceptions;
using GitIssue.Fields;
using GitIssue.Issues;
using GitIssue.Issues.Json;
using GitIssue.Values;
using NUnit.Framework;
using String = GitIssue.Values.String;

namespace GitIssue.Tests.IssueManagerTests
{
    [TestFixture]
    public partial class IssueManagerTests
    {
        [TestFixture]
        public class IssueTypes : IssueManagerTests
        {
            [TestCase(IssueStorage.Json)]
            [TestCase(IssueStorage.Markdown)]
            public async Task PersistsAndLoadsDifferentSchemas(IssueStorage storage)
            {
                this.Initialize(this.TestDirectory, initIssue: false, initSut: false);
                IssueConfiguration configuration = new IssueConfiguration
                {
                    IssueType = "BUG",
                    Storage = storage,
                    KeyTemplate = "{type}-{uid}",
                };
                Dictionary<FieldKey, FieldInfo> core = configuration.GetSchema("ISSUE");
                Dictionary<FieldKey, FieldInfo> bugFields = new Dictionary<FieldKey, FieldInfo>(core)
                {
                    ["Severity"] = new FieldInfo<String, JsonValueField>(true),
                };
                Dictionary<FieldKey, FieldInfo> featureFields = new Dictionary<FieldKey, FieldInfo>(core)
                {
                    ["Motivation"] = new FieldInfo<String, JsonValueField>(true),
                };
                configuration.Issues.Add(new IssueTypeConfiguration { Name = "BUG", Fields = bugFields });
                configuration.Issues.Add(new IssueTypeConfiguration { Name = "FEATURE", Fields = featureFields });
                using IIssueManager manager = IssueManager.Init(configuration, this.TestDirectory);
                IField severity = bugFields["Severity"].CreateField(null!, "Severity");
                Assert.That(severity.Update("high"), Is.True);
                IField motivation = featureFields["Motivation"].CreateField(null!, "Motivation");
                Assert.That(motivation.Update("easier setup"), Is.True);
                IIssue bug = await manager.CreateAsync("Bug", "Bug description", "BUG", new[] { severity });
                IIssue feature = await manager.CreateAsync("Feature", "Feature description", "FEATURE", new[] { motivation });
                Assert.That(async () => await manager.CreateAsync("Missing severity", "", "BUG", Array.Empty<IField>()),
                    Throws.TypeOf<IssueValidationException>());

                using IIssueManager reopened = IssueManager.Open(this.TestDirectory);
                IIssue[] issues = reopened.Find(_ => true).ToArray();
                IIssue loadedBug = issues.Single(issue => issue.Key == bug.Key);
                IIssue loadedFeature = issues.Single(issue => issue.Key == feature.Key);
                Assert.Multiple(() =>
                {
                    Assert.That(loadedBug.IssueType, Is.EqualTo("BUG"));
                    Assert.That(loadedBug["Severity"].ToString(), Is.EqualTo("high"));
                    Assert.That(loadedBug.ContainsKey("Motivation"), Is.False);
                    Assert.That(loadedFeature.IssueType, Is.EqualTo("FEATURE"));
                    Assert.That(loadedFeature["Motivation"].ToString(), Is.EqualTo("easier setup"));
                    Assert.That(loadedFeature.ContainsKey("Severity"), Is.False);
                    Assert.That(bug.Key.ToString(), Does.StartWith("BUG-"));
                    Assert.That(feature.Key.ToString(), Does.StartWith("FEATURE-"));
                });

                loadedBug.SetField("IssueType").WithValue(String.Parse("FEATURE"));
                Assert.That(async () => await loadedBug.SaveAsync(), Throws.TypeOf<InvalidOperationException>());
            }

            [TestCase(IssueStorage.Json)]
            [TestCase(IssueStorage.Markdown)]
            public async Task MissingTypeUsesConfiguredDefault(IssueStorage storage)
            {
                this.Initialize(this.TestDirectory, initIssue: false, initSut: false);
                IssueConfiguration configuration = new IssueConfiguration
                {
                    Storage = storage,
                    KeyTemplate = "{type}-{uid}",
                };
                using IIssueManager manager = IssueManager.Init(configuration, this.TestDirectory);
                IIssue issue = await manager.CreateAsync("Legacy", "Description");
                string path = storage == IssueStorage.Markdown
                    ? ((GitIssue.Issues.Markdown.MarkdownIssue)issue).Markdown
                    : ((JsonIssue)issue).Json;
                string text = await File.ReadAllTextAsync(path);
                if (storage == IssueStorage.Markdown)
                {
                    text = text.Replace("\n## IssueType\n\nISSUE\n", "\n", StringComparison.Ordinal);
                }
                else
                {
                    var json = System.Text.Json.Nodes.JsonNode.Parse(text)!.AsObject();
                    json.Remove("IssueType");
                    text = json.ToJsonString();
                }

                await File.WriteAllTextAsync(path, text);
                configuration.Issues[0].Name = "BUG";
                configuration.IssueType = "BUG";
                IIssue loaded = manager.Find(_ => true).Single();
                Assert.That(loaded.Key.ToString(), Does.StartWith("ISSUE-"));
                Assert.That(loaded.IssueType, Is.EqualTo("BUG"));
                Assert.That(await loaded.SaveAsync(), Is.True);
                Assert.That(await File.ReadAllTextAsync(path), Does.Contain("IssueType"));
            }

            [TestCase(IssueStorage.Json)]
            [TestCase(IssueStorage.Markdown)]
            public async Task UnknownFieldsCannotBeDiscardedOnSave(IssueStorage storage)
            {
                this.Initialize(this.TestDirectory, initIssue: false, initSut: false);
                using IIssueManager manager = IssueManager.Init(new IssueConfiguration { Storage = storage }, this.TestDirectory);
                IIssue issue = await manager.CreateAsync("Keep data", "Description");
                string path = storage == IssueStorage.Markdown
                    ? ((GitIssue.Issues.Markdown.MarkdownIssue)issue).Markdown
                    : ((JsonIssue)issue).Json;
                string text = await File.ReadAllTextAsync(path);
                if (storage == IssueStorage.Markdown)
                {
                    text += "\n## Extra\n\nDo not discard\n";
                }
                else
                {
                    var json = System.Text.Json.Nodes.JsonNode.Parse(text)!.AsObject();
                    json["Extra"] = "Do not discard";
                    text = json.ToJsonString();
                }

                await File.WriteAllTextAsync(path, text);
                IIssue loaded = manager.Find(_ => true).Single();
                loaded.Title = "Changed";
                Assert.That(async () => await loaded.SaveAsync(), Throws.TypeOf<IssueValidationException>());
                Assert.That(await File.ReadAllTextAsync(path), Is.EqualTo(text));
            }

            [Test]
            public void MarkdownTypeReaderIgnoresHeadingsInCodeBlocks()
            {
                string markdown = "# key\n\n## Description\n\n```\n## IssueType\n\nWRONG\n```\n\n## IssueType\n\nBUG\n";
                Assert.That(GitIssue.Issues.Markdown.MarkdownIssueFormat.ReadIssueType(markdown), Is.EqualTo("BUG"));
                Assert.That(
                    () => GitIssue.Issues.Markdown.MarkdownIssueFormat.ReadIssueType(markdown + "\n## IssueType\n\nOTHER\n"),
                    Throws.TypeOf<InvalidDataException>());
            }
        }
    }
}
