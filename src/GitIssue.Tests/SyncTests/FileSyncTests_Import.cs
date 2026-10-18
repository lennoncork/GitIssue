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