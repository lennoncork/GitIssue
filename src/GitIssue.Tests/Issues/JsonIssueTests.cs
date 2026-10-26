using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GitIssue.Exceptions;
using GitIssue.Fields;
using GitIssue.Issues;
using GitIssue.Issues.File;
using GitIssue.Issues.Json;
using Moq;
using NUnit.Framework;
using StringValue = GitIssue.Values.String;

namespace GitIssue.Tests.Issues
{
    [TestFixture]
    public class JsonIssueTests
    {
        private class TestJsonIssue : JsonIssue
        {
            public TestJsonIssue(IssueRoot root) : base(root)
            {
            }

            public void AddField(FieldKey key, IField field)
            {
                this.fields[key] = field;
            }
        }

        [TestFixture]
        public class Basics : JsonIssueTests
        {
            [Test]
            public void Count_Keys_Item_ContainsKey_And_TryGetValue_Work()
            {
                string repoDir = Helpers.CreateTempDirectory();
                RepositoryRoot repoRoot = RepositoryRoot.Open(repoDir, null);
                IssueKey issueKey = IssueKey.Create("ISS-JSON-1");
                IssueRoot issueRoot = new IssueRoot(repoRoot, issueKey, "issue1");

                TestJsonIssue issue = new TestJsonIssue(issueRoot);

                FieldKey key1 = FieldKey.Create("field1");
                FieldKey key2 = FieldKey.Create("field2");
                IField field1 = Moqs.CreateField(key1, "value1");
                IField field2 = Moqs.CreateField(key2, "value2");

                issue.AddField(key1, field1);
                issue.AddField(key2, field2);

                Assert.That(issue.Count, Is.EqualTo(2));
                Assert.That(issue.Keys, Is.EquivalentTo(new[] { key1, key2 }));
                Assert.That(issue[key1], Is.SameAs(field1));
                Assert.That(issue.ContainsKey(key1), Is.True);

                bool found = issue.TryGetValue(key2, out IField foundField);
                Assert.That(found, Is.True);
                Assert.That(foundField, Is.SameAs(field2));

                FieldKey missingKey = FieldKey.Create("missing");
                bool missing = issue.TryGetValue(missingKey, out IField? missingField);
                Assert.That(missing, Is.False);
                Assert.That(missingField, Is.Null);

                List<FieldKey> enumeratedKeys = new List<FieldKey>();
                foreach (KeyValuePair<FieldKey, IField> kvp in issue)
                {
                    enumeratedKeys.Add(kvp.Key);
                }

                Assert.That(enumeratedKeys, Is.EquivalentTo(new[] { key1, key2 }));
            }

            [Test]
            public void Json_Property_Returns_Expected_Path()
            {
                string repoDir = Helpers.CreateTempDirectory();
                RepositoryRoot repoRoot = RepositoryRoot.Open(repoDir, null);
                IssueKey issueKey = IssueKey.Create("ISS-JSON-2");
                IssueRoot issueRoot = new IssueRoot(repoRoot, issueKey, null);

                TestJsonIssue issue = new TestJsonIssue(issueRoot);

                string expected = JsonIssue.GetJsonFile(issueRoot);

                Assert.That(issue.Json, Is.EqualTo(expected));
            }

            [Test]
            public void SaveAsJsonAsync_PreservesExistingFileWhenSerializationFails()
            {
                string repoDir = Helpers.CreateTempDirectory();
                RepositoryRoot repoRoot = RepositoryRoot.Open(repoDir, null);
                TestJsonIssue issue = new TestJsonIssue(new IssueRoot(repoRoot, IssueKey.Create("issue"), "issue"));
                FieldKey key = FieldKey.Create("field");
                Mock<IJsonField> field = new Mock<IJsonField>(MockBehavior.Strict);
                field.SetupGet(value => value.Key).Returns(key);
                field.Setup(value => value.ToJson()).Throws<InvalidOperationException>();
                issue.AddField(key, field.Object);
                string path = Path.Combine(repoDir, "issue.json");
                File.WriteAllText(path, "preserve");

                Assert.That(async () => await issue.SaveAsJsonAsync(path),
                    Throws.TypeOf<InvalidOperationException>());
                Assert.That(File.ReadAllText(path), Is.EqualTo("preserve"));
            }

            [Test]
            public void SetField_WithField_CopiesTheUpdatedValue()
            {
                string repoDir = Helpers.CreateTempDirectory();
                RepositoryRoot repoRoot = RepositoryRoot.Open(repoDir, null);
                TestJsonIssue issue = new TestJsonIssue(new IssueRoot(repoRoot, IssueKey.Create("issue"), "issue"));
                FieldKey key = FieldKey.Create("field");
                JsonValueField<StringValue> existing = new JsonValueField<StringValue>(key, StringValue.Parse("before"));
                issue.AddField(key, existing);

                issue.SetField(key).WithField(new JsonValueField<StringValue>(key, StringValue.Parse("after")));

                Assert.That(existing.Value, Is.EqualTo(StringValue.Parse("after")));
            }
        }

        [TestFixture]
        public class Read : JsonIssueTests
        {
            [Test]
            public async Task ReadAsync_Parses_The_Issue_File_Once()
            {
                string repoDir = Helpers.CreateTempDirectory();
                RepositoryRoot repoRoot = RepositoryRoot.Open(repoDir, null);
                IssueRoot issueRoot = new IssueRoot(repoRoot, IssueKey.Create("ISS-JSON-READ"), "read-once");
                IssueConfiguration configuration = new IssueConfiguration();
                StringValue title = StringValue.Parse("Read once");
                StringValue description = StringValue.Parse("Parsed a single time");

                JsonIssue created = new JsonIssue(issueRoot, configuration.Fields);
                created.Title = title;
                created.Description = description;
                await created.SaveAsync();

                JsonIssueExtensions.ResetFileReadCount();
                IIssue? fromJson = await JsonIssue.ReadAsync(issueRoot, configuration.Fields);
                int jsonReads = JsonIssueExtensions.FileReadCount;

                JsonIssueExtensions.ResetFileReadCount();
                IIssue? fromFile = await FileIssue.ReadAsync(issueRoot, configuration.Fields);
                int fileReads = JsonIssueExtensions.FileReadCount;

                Assert.That(jsonReads, Is.EqualTo(1));
                Assert.That(fileReads, Is.EqualTo(1));
                Assert.That(fromJson, Is.Not.Null);
                Assert.That(fromFile, Is.Not.Null);
                Assert.That(fromJson!.Title, Is.EqualTo(title));
                Assert.That(fromJson.Description, Is.EqualTo(description));
                Assert.That(fromFile!.Title, Is.EqualTo(title));
                Assert.That(fromFile.Description, Is.EqualTo(description));
            }
        }

        [TestFixture]
        public class Delete : JsonIssueTests
        {
            [Test]
            public async Task DeleteAsync_Deletes_Json_File_And_Directory_When_Present()
            {
                string repoDir = Helpers.CreateTempDirectory();
                RepositoryRoot repoRoot = RepositoryRoot.Open(repoDir, null);
                IssueKey issueKey = IssueKey.Create("ISS-JSON-4");
                IssueRoot issueRoot = new IssueRoot(repoRoot, issueKey, "issue4");

                Directory.CreateDirectory(issueRoot.IssuePath);
                string jsonPath = JsonIssue.GetJsonFile(issueRoot);
                File.WriteAllText(jsonPath, "{}");

                bool result = await JsonIssue.DeleteAsync(issueRoot);

                Assert.That(result, Is.True);
                Assert.That(File.Exists(jsonPath), Is.False);
                Assert.That(Directory.Exists(issueRoot.IssuePath), Is.False);
            }

            [Test]
            public void DeleteAsync_Throws_When_IssuePath_Does_Not_Exist()
            {
                string repoDir = Helpers.CreateTempDirectory();
                RepositoryRoot repoRoot = RepositoryRoot.Open(repoDir, null);
                IssueKey issueKey = IssueKey.Create("ISS-JSON-3");
                IssueRoot issueRoot = new IssueRoot(repoRoot, issueKey, "nonexistent");

                Assert.That(
                    async () => await JsonIssue.DeleteAsync(issueRoot),
                    Throws.TypeOf<IssueNotFoundException>());
            }
        }
    }
}