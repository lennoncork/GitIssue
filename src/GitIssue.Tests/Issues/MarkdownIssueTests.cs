using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GitIssue.Exceptions;
using GitIssue.Fields;
using GitIssue.Fields.Array;
using GitIssue.Issues;
using GitIssue.Issues.Json;
using GitIssue.Issues.Markdown;
using GitIssue.Validation;
using GitIssue.Values;
using NUnit.Framework;
using String = GitIssue.Values.String;

namespace GitIssue.Tests.Issues
{
    [TestFixture]
    public class MarkdownIssueTests
    {
        [Test]
        public void Read_PreservesDescriptionInsideAFence()
        {
            IssueConfiguration configuration = new IssueConfiguration();
            IssueKey key = IssueKey.Create("ISSUE-1");
            string description = "Before\n\n```\n## Title\n```\n\nAfter";
            string markdown = MarkdownIssueTests.document(key, description, "Ada Lovelace <ada@example.com>");

            MarkdownRead read = MarkdownIssueFormat.Read(markdown, key, configuration.GetSchema(configuration.IssueType));

            Assert.Multiple(() =>
            {
                Assert.That(read.Errors, Is.Empty);
                Assert.That(read.Values[FieldKey.Create("Description")], Is.EqualTo((String)description));
                Assert.That(read.Values[FieldKey.Create("Title")], Is.EqualTo((String)"Save issues as markdown"));
            });
        }

        [Test]
        public void Read_ReportsUnknownHeadingDuplicateAndInvalidValues()
        {
            IssueConfiguration configuration = new IssueConfiguration();
            IssueKey key = IssueKey.Create("ISSUE-1");
            string markdown = """
                # ISSUE-1

                ## Title

                Save issues as markdown

                ## Title

                Again

                ## Notes

                Extra

                ## Author

                Ada

                ## Labels

                not a list

                ## Description

                Body
                """;

            MarkdownRead read = MarkdownIssueFormat.Read(markdown, key, configuration.GetSchema(configuration.IssueType));
            string[] messages = read.Errors.Select(error => error.ToString()).ToArray();

            Assert.Multiple(() =>
            {
                Assert.That(messages, Has.Some.Contains("duplicate field"));
                Assert.That(messages, Has.Some.Contains("unknown field"));
                Assert.That(messages, Has.Some.Contains("not a valid Signature"));
                Assert.That(messages, Has.Some.Contains("content is not a list"));
                Assert.That(messages, Has.Some.Contains("required field is missing"));
            });
        }

        [Test]
        public void Read_RejectsALabelThatContainsMoreThanOneToken()
        {
            IssueConfiguration configuration = new IssueConfiguration();
            IssueKey key = IssueKey.Create("ISSUE-1");
            string markdown = MarkdownIssueTests.document(key, "Body", "Ada Lovelace <ada@example.com>")
                .Replace("- documentation", "- hello world", System.StringComparison.Ordinal);
            string windowsMarkdown = markdown.Replace("\r\n", "\n", System.StringComparison.Ordinal).Replace("\n", "\r\n", System.StringComparison.Ordinal);

            MarkdownRead read = MarkdownIssueFormat.Read(markdown, key, configuration.GetSchema(configuration.IssueType));
            MarkdownRead windowsRead = MarkdownIssueFormat.Read(windowsMarkdown, key, configuration.GetSchema(configuration.IssueType));

            Assert.Multiple(() =>
            {
                Assert.That(read.Errors.Select(error => error.ToString()), Has.Some.Contains("not a valid Label"));
                Assert.That(windowsRead.Errors.Select(error => error.ToString()), Has.Some.Contains("not a valid Label"));
            });
        }

        [Test]
        public void TryAccept_UsesEnumeratedMetadata()
        {
            FieldInfo info = new FieldInfo<Enumerated, JsonValueField>(true)
            {
                ValueMetadata = "[open, closed]",
            };
            FieldKey key = FieldKey.Create("State");

            bool open = IssueValidator.TryAccept(info, key, "open", out object? openValue, out string? openError);
            bool closed = IssueValidator.TryAccept(info, key, "later", out object? _, out string? closedError);

            Assert.Multiple(() =>
            {
                Assert.That(open, Is.True, openError);
                Assert.That(openValue, Is.EqualTo(Enumerated.TryParse(new ValueMetadata("open", info.ValueMetadata), out Enumerated parsed) ? parsed : default(Enumerated)));
                Assert.That(closed, Is.False);
                Assert.That(closedError, Does.Contain("not a valid"));
            });
        }

        [Test]
        public async Task SaveAndRead_RoundTripsConfiguredFields()
        {
            string directory = Helpers.CreateTempDirectory();
            RepositoryRoot root = RepositoryRoot.Open(directory, null);
            IssueKey key = IssueKey.Create("ISSUE-1");
            IssueRoot issueRoot = new IssueRoot(root, key, "issue");
            IssueConfiguration configuration = new IssueConfiguration();
            MarkdownIssue issue = new MarkdownIssue(issueRoot, configuration.GetSchema(configuration.IssueType));
            issue.Title = "Save issues as markdown";
            issue.Description = "Before\n\n```\n## Title\n```\n\nAfter";
            issue.Author = Signature.Parse("Ada Lovelace <ada@example.com>");
            issue.Created = System.DateTime.Parse("2026-11-08T03:25:00Z");
            issue.Updated = System.DateTime.Parse("2026-11-08T04:00:00Z");
            issue.SetField("Labels").WithArray(new[] { Label.Parse("documentation"), Label.Parse("ai") });
            issue.SetField("Comments").WithArray(new[] { String.Parse("First comment\nwith two lines"), String.Parse("Second") });

            Assert.That(await issue.SaveAsync(), Is.True);
            string written = await File.ReadAllTextAsync(issue.Markdown);
            IIssue? loaded = await MarkdownIssue.ReadAsync(issueRoot, configuration.GetSchema(configuration.IssueType));

            Assert.Multiple(() =>
            {
                Assert.That(File.Exists(JsonIssue.GetJsonFile(issueRoot)), Is.False);
                Assert.That(written, Does.Contain("```\n## Title\n```"));
                Assert.That(loaded, Is.Not.Null);
                Assert.That(loaded!.Title, Is.EqualTo((String)"Save issues as markdown"));
                Assert.That(loaded.Description, Is.EqualTo(issue.Description));
                Assert.That(loaded.Author, Is.EqualTo(issue.Author));
                Assert.That(loaded.Comments, Is.EquivalentTo(issue.Comments));
                Assert.That(((IArrayField)loaded[FieldKey.Create("Labels")]).Values,
                    Is.EqualTo(new object[] { Label.Parse("documentation"), Label.Parse("ai") }));
            });
        }

        [Test]
        public void Save_LeavesTheFileUnwrittenWhenValidationFails()
        {
            string directory = Helpers.CreateTempDirectory();
            RepositoryRoot root = RepositoryRoot.Open(directory, null);
            IssueRoot issueRoot = new IssueRoot(root, IssueKey.Create("ISSUE-1"), "issue");
            IssueConfiguration configuration = new IssueConfiguration();
            MarkdownIssue issue = new MarkdownIssue(issueRoot, configuration.GetSchema(configuration.IssueType));
            issue.Description = "Body";

            Assert.That(async () => await issue.SaveAsync(), Throws.TypeOf<IssueValidationException>());
            Assert.That(File.Exists(issue.Markdown), Is.False);
        }

        private static string document(IssueKey key, string description, string author)
        {
            return $"""
                # {key}

                ## Key

                {key}

                ## Title

                Save issues as markdown

                ## Description

                {description}

                ## Author

                {author}

                ## Created

                2026-11-08T03:25:00.0000000Z

                ## Updated

                2026-11-08T03:25:00.0000000Z

                ## Labels

                - documentation

                ## Comments

                ### 1

                First comment

                """;
        }
    }
}
