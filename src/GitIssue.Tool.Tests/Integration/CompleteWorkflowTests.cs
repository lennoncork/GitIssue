using System.IO;
using LibGit2Sharp;
using NUnit.Framework;

namespace GitIssue.Tool.Tests.Integration
{
    [TestFixture]
    public class CompleteWorkflowTests : IntegrationTestBase
    {
        [Test]
        public void IssueLifecycle_ExecutesEveryRegisteredCommand()
        {
            const string initialTitle = "Prepare release notes";
            const string updatedTitle = "Publish release notes";
            const string importedTitle = "Follow up with external reporter";

            // A user starts by creating the local issue store for the repository.
            this.RunSuccessfully("Init");
            this.AssertFileExists(this.ConfigurationFile);
            this.AssertFileDoesNotExist(this.TrackingFile);

            // Inspect available fields before recording the issue details.
            this.RunSuccessfully("Fields");
            Assert.That(this.Output, Does.Contain("Title: A 'JsonValue' field with 'String' values"));

            // Capture a newly discovered release task and begin tracking it.
            this.RunSuccessfully("Create", initialTitle, "Document customer-visible changes.");
            string primaryKey = this.FindIssueKey(initialTitle);
            var primaryIssue = this.ReadIssue(primaryKey);
            this.AssertIssueExists(primaryKey);
            this.AssertJsonValue(primaryIssue, "Description", "Document customer-visible changes.");
            this.AssertJsonValue(this.ReadJsonObject(this.TrackingFile), "Key", primaryKey);

            // Classify the task so it can be filtered alongside similar work.
            this.RunSuccessfully("Add", primaryKey, "Labels", "release");
            this.AssertJsonArrayContains(this.ReadIssue(primaryKey), "Labels", "release");

            // Refine the title once the task is ready for publication.
            this.RunSuccessfully("Edit", primaryKey, "Title", updatedTitle);
            this.AssertJsonValue(this.ReadIssue(primaryKey), "Title", updatedTitle);

            // Preserve the conversation that explains why this task matters.
            this.RunSuccessfully("Comment", primaryKey, "Include the compatibility notes.");
            this.AssertJsonArrayContains(this.ReadIssue(primaryKey), "Comments", "Include the compatibility notes.");

            // Search the backlog to confirm the updated task is discoverable.
            this.RunSuccessfully("Find", "title=Publish release notes");
            Assert.That(this.Output, Does.Contain(updatedTitle));
            Assert.That(this.Output, Does.Not.Contain(importedTitle));

            // Remove the temporary triage label after the release work is categorized elsewhere.
            this.RunSuccessfully("Remove", primaryKey, "Labels", "release");
            this.AssertJsonArrayDoesNotContain(this.ReadIssue(primaryKey), "Labels", "release");

            // Bring an externally reported follow-up into the same local backlog.
            string importFile = Path.Combine(this.TestDirectory, "external-issues.csv");
            File.WriteAllText(importFile, $"Key; Title{System.Environment.NewLine}EXTERNAL-1; {importedTitle}");
            this.RunSuccessfully("Import", importFile);
            string importedKey = this.FindIssueKey(importedTitle);
            this.AssertFileContents(importFile, $"Key; Title{System.Environment.NewLine}EXTERNAL-1; {importedTitle}");
            this.AssertIssueExists(importedKey);
            this.AssertJsonValue(this.ReadIssue(importedKey), "Title", importedTitle);

            // Shift the working context to the externally reported follow-up.
            this.RunSuccessfully("Track", importedKey);
            this.AssertJsonValue(this.ReadJsonObject(this.TrackingFile), "Key", importedKey);

            // Display the active task before handing work to the next engineer.
            this.RunSuccessfully("Show", "tracked");
            Assert.That(this.Output, Does.Contain(importedTitle));
            Assert.That(this.Output, Does.Not.Contain(updatedTitle));

            // Export a portable backlog snapshot for stakeholders outside the repository.
            string exportFile = Path.Combine(this.TestDirectory, "backlog.json");
            this.AssertFileDoesNotExist(exportFile);
            this.RunSuccessfully("Export", exportFile);
            var exportedIssues = this.ReadJsonObject(exportFile);
            this.AssertJsonPropertyExists(exportedIssues, primaryKey);
            this.AssertJsonPropertyExists(exportedIssues, importedKey);
            this.AssertJsonValue(exportedIssues[primaryKey]!.AsObject(), "Title", updatedTitle);
            this.AssertJsonValue(exportedIssues[importedKey]!.AsObject(), "Title", importedTitle);

            // Review accumulated changes before finalizing the repository update.
            this.RunSuccessfully("Changes");
            var changeLog = this.ReadJsonObject(this.ChangesFile)["Log"]!.AsObject();
            Assert.That(this.Output, Does.Contain($"Issue: {primaryKey}"));
            Assert.That(this.Output, Does.Contain($"Issue: {importedKey}"));
            this.AssertJsonArrayItemContains(changeLog, primaryKey, "Created new issue");
            this.AssertJsonArrayItemContains(changeLog, importedKey, "Created new issue");

            // Retire the completed release-notes task while keeping the follow-up active.
            this.RunSuccessfully("Delete", primaryKey);
            this.AssertIssueDoesNotExist(primaryKey);
            this.AssertIssueExists(importedKey);
            this.AssertJsonValue(this.ReadJsonObject(this.TrackingFile), "Key", importedKey);

            // Commit the resulting issue state so the team can share the updated backlog.
            this.RunSuccessfully("Commit");
            var committedChanges = this.ReadJsonObject(this.ChangesFile);
            using Repository repository = new Repository(this.TestDirectory);
            Assert.That(repository.Head.Tip, Is.Not.Null);
            Assert.That(repository.Head.Tip!.Message, Does.Contain(importedKey));
            Assert.That(repository.Head.Tip.Tree[".issues"], Is.Not.Null);
            Assert.That(committedChanges["Log"]!.AsObject(), Is.Empty);
        }
    }
}