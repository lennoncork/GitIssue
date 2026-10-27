using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GitIssue.Fields;
using GitIssue.Fields.Array;
using GitIssue.Issues;
using GitIssue.Syncs;
using NUnit.Framework;

namespace GitIssue.Tests.SyncTests
{
    [TestFixture]
    public partial class FileSyncTests
    {
        [TestFixture]
        public class Import : FileSyncTests
        {
            [Test]
            public async Task ImportsIssue()
            {
                string issue_file = Path.Combine(TestContext.CurrentContext.TestDirectory,
                    "SyncTests", "ImportFiles", "jira-issue.json");

                string config_file = Path.Combine(TestContext.CurrentContext.TestDirectory,
                    "SyncTests", "ImportFiles", "jira-config.json");

                this.Initialize(this.TestDirectory);
                FileImporter importer = new FileImporter(this.Sut) { Configuration = await SyncConfiguration.ReadAsync(config_file), ImportPath = issue_file };
                bool imported = await importer.Import();

                IIssue issue = this.Sut.Find(candidate => candidate.Title.ToString() == "This is a jira issue").Single();
                IArrayField labels = (IArrayField)issue[FieldKey.Create("Labels")];
                Assert.Multiple(() =>
                {
                    Assert.That(imported, Is.True);
                    Assert.That(issue.Description.ToString(), Is.EqualTo("Foo Bar"));
                    Assert.That(labels.Values!.Select(value => value.ToString()), Does.Contain("decision"));
                });
            }

            [Test]
            public async Task ReturnsFalseWhenImportFileDoesNotExist()
            {
                this.Initialize(this.TestDirectory);
                FileImporter importer = new FileImporter(this.Sut)
                {
                    ImportPath = Path.Combine(this.TestDirectory, "missing.json"),
                };

                bool imported = await importer.Import();

                Assert.Multiple(() =>
                {
                    Assert.That(imported, Is.False);
                    Assert.That(this.Sut.Find(issue => true), Is.Empty);
                });
            }

            [Test]
            public async Task ReturnsFalseWhenRequiredTitleIsMissing()
            {
                this.Initialize(this.TestDirectory);
                string importFile = Path.Combine(this.TestDirectory, "missing-title.json");
                File.WriteAllText(importFile, "{ \"key\": \"EXTERNAL-1\" }");
                FileImporter importer = new FileImporter(this.Sut)
                {
                    Configuration = new SyncConfiguration
                    {
                        Mapping =
                        {
                            ["Key"] = "key",
                            ["Title"] = "summary",
                        },
                    },
                    ImportPath = importFile,
                };

                bool imported = await importer.Import();

                Assert.Multiple(() =>
                {
                    Assert.That(imported, Is.False);
                    Assert.That(this.Sut.Find(issue => true), Is.Empty);
                });
            }

            [Test]
            public async Task ImportsMappedNestedPropertiesAndArrayValues()
            {
                this.Initialize(this.TestDirectory);
                string importFile = Path.Combine(this.TestDirectory, "nested.json");
                File.WriteAllText(importFile,
                    "{ \"payload\": { \"key\": \"EXTERNAL-2\", \"titles\": [ \"Nested issue\" ], \"description\": \"Nested description\", \"labels\": [ \"release\" ] } }");
                FileImporter importer = new FileImporter(this.Sut)
                {
                    Configuration = new SyncConfiguration
                    {
                        Mapping =
                        {
                            ["Key"] = "payload.key",
                            ["Title"] = "payload.titles[0]",
                            ["Description"] = "payload.description",
                            ["Labels"] = "payload.labels",
                        },
                    },
                    ImportPath = importFile,
                };

                bool imported = await importer.Import();

                IIssue issue = this.Sut.Find(candidate => candidate.Title.ToString() == "Nested issue").Single();
                IArrayField labels = (IArrayField)issue[FieldKey.Create("Labels")];
                Assert.Multiple(() =>
                {
                    Assert.That(imported, Is.True);
                    Assert.That(issue.Description.ToString(), Is.EqualTo("Nested description"));
                    Assert.That(labels.Values!.Select(value => value.ToString()), Does.Contain("release"));
                });
            }

            [Test]
            public async Task ReturnsFalseWhenMappedArrayIndexDoesNotExist()
            {
                this.Initialize(this.TestDirectory);
                string importFile = Path.Combine(this.TestDirectory, "missing-index.json");
                File.WriteAllText(importFile, "{ \"payload\": { \"key\": \"EXTERNAL-3\", \"titles\": [ \"Only title\" ] } }");
                FileImporter importer = new FileImporter(this.Sut)
                {
                    Configuration = new SyncConfiguration
                    {
                        Mapping =
                        {
                            ["Key"] = "payload.key",
                            ["Title"] = "payload.titles[1]",
                        },
                    },
                    ImportPath = importFile,
                };

                bool imported = await importer.Import();

                Assert.Multiple(() =>
                {
                    Assert.That(imported, Is.False);
                    Assert.That(this.Sut.Find(issue => true), Is.Empty);
                });
            }

            [Test]
            public async Task ReimportUpdatesExistingIssueInsteadOfCreatingDuplicate()
            {
                this.Initialize(this.TestDirectory);
                string importFile = Path.Combine(this.TestDirectory, "reimport.json");
                File.WriteAllText(importFile,
                    "{ \"key\": \"EXTERNAL-9\", \"summary\": \"Original title\", \"description\": \"Original description\" }");
                SyncConfiguration configuration = new SyncConfiguration
                {
                    Mapping =
                    {
                        ["Key"] = "key",
                        ["Title"] = "summary",
                        ["Description"] = "description",
                    },
                };
                FileImporter importer = new FileImporter(this.Sut)
                {
                    Configuration = configuration,
                    ImportPath = importFile,
                };

                bool firstImport = await importer.Import();
                IIssue created = this.Sut.Find(candidate => candidate.Title.ToString() == "Original title").Single();
                IssueKey createdKey = created.Key;

                File.WriteAllText(importFile,
                    "{ \"key\": \"EXTERNAL-9\", \"summary\": \"Updated title\", \"description\": \"Updated description\" }");
                FileImporter secondImporter = new FileImporter(this.Sut)
                {
                    Configuration = configuration,
                    ImportPath = importFile,
                };
                bool secondImport = await secondImporter.Import();

                IIssue[] issues = this.Sut.Find(issue => true).ToArray();
                IIssue updated = issues.Single();
                string mappingFile = Path.Combine(importer.Root.ImportPath, "EXTERNAL-9.import");
                string mappingContents = File.Exists(mappingFile) ? File.ReadAllText(mappingFile) : string.Empty;

                Assert.Multiple(() =>
                {
                    Assert.That(firstImport, Is.True);
                    Assert.That(secondImport, Is.True);
                    Assert.That(issues.Length, Is.EqualTo(1));
                    Assert.That(updated.Key, Is.EqualTo(createdKey));
                    Assert.That(updated.Title.ToString(), Is.EqualTo("Updated title"));
                    Assert.That(updated.Description.ToString(), Is.EqualTo("Updated description"));
                    Assert.That(File.Exists(mappingFile), Is.True);
                    Assert.That(mappingContents, Is.EqualTo(createdKey.ToString()));
                });
            }

            [Test]
            public void SaveMapping_RejectsPathOutsideImportDirectory()
            {
                this.Initialize(this.TestDirectory);
                string sentinel = Path.Combine(this.TestDirectory, "sentinel.txt");
                File.WriteAllText(sentinel, "preserve");
                FileImporter importer = new FileImporter(this.Sut);
                SyncedIssue synced = new SyncedIssue(importer, IssueKey.Create("local"), IssueKey.Create("../../sentinel.txt"));

                Assert.ThrowsAsync<ArgumentException>(async () => await synced.SaveAsync());
                Assert.That(File.ReadAllText(sentinel), Is.EqualTo("preserve"));
            }
        }
    }
}