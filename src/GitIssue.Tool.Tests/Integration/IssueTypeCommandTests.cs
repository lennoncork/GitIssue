using System;
using System.Collections.Generic;
using System.IO;
using GitIssue.Fields;
using GitIssue.Issues.Json;
using NUnit.Framework;
using String = GitIssue.Values.String;

namespace GitIssue.Tool.Tests.Integration
{
    [TestFixture]
    public class IssueTypeCommandTests : IntegrationTestBase
    {
        [Test]
        public void CreateAndConvertPreserveTypeSpecificFields()
        {
            this.InitializeRepository();
            IssueConfiguration configuration = IssueConfiguration.Read(this.ConfigurationFile);
            Dictionary<FieldKey, FieldInfo> bugFields = new Dictionary<FieldKey, FieldInfo>(configuration.GetSchema("Issue"))
            {
                ["Severity"] = new FieldInfo<String, JsonValueField>(true),
                ["Platform"] = new FieldInfo<String, JsonValueField>(true),
            };
            configuration.Issues.Add(new IssueTypeConfiguration { Name = "BUG", Fields = bugFields });
            configuration.Save(this.ConfigurationFile);

            Assert.That(this.Run("Create", "Bug report", "A bug", "--type", "BUG", "--field", "Severity=high", "Platform=linux"),
                Is.Zero, this.Error);
            string key = this.FindIssueKey("Bug report");
            Assert.That(this.ReadIssue(key)["IssueType"]!.GetValue<string>(), Is.EqualTo("BUG"));
            Assert.That(this.ReadIssue(key)["Severity"]!.GetValue<string>(), Is.EqualTo("high"));
            Assert.That(this.Run("Fields", "--type", "BUG"), Is.Zero, this.Error);
            Assert.That(this.Output, Does.Contain("Severity"));
            Assert.That(this.Run("Convert", "--to", "markdown"), Is.Zero, this.Error);
            Assert.That(this.Run("Convert", "--to", "json"), Is.Zero, this.Error);
            Assert.That(this.ReadIssue(key)["IssueType"]!.GetValue<string>(), Is.EqualTo("BUG"));
            Assert.That(this.ReadIssue(key)["Severity"]!.GetValue<string>(), Is.EqualTo("high"));

            string csv = Path.Combine(this.TestDirectory, "typed.csv");
            Assert.That(this.Run("Export", csv), Is.Zero, this.Error);
            Assert.That(this.Run("Delete", key), Is.Zero, this.Error);
            Assert.That(this.Run("Import", csv), Is.Zero, this.Error);
            string imported = this.FindIssueKey("Bug report");
            Assert.That(this.ReadIssue(imported)["IssueType"]!.GetValue<string>(), Is.EqualTo("BUG"));
            Assert.That(this.ReadIssue(imported)["Severity"]!.GetValue<string>(), Is.EqualTo("high"));

            string json = Path.Combine(this.TestDirectory, "typed.json");
            File.WriteAllText(json, "{\"Key\":\"external-1\",\"Title\":\"JSON import\",\"Description\":\"A bug\",\"IssueType\":\"BUG\",\"Severity\":\"low\",\"Platform\":\"linux\"}");
            Assert.That(this.Run("Import", json), Is.Zero, this.Error);
            string importedJson = this.FindIssueKey("JSON import");
            Assert.That(this.ReadIssue(importedJson)["IssueType"]!.GetValue<string>(), Is.EqualTo("BUG"));
            Assert.That(this.ReadIssue(importedJson)["Severity"]!.GetValue<string>(), Is.EqualTo("low"));

            File.WriteAllText(json, "{\"Key\":\"external-2\",\"Title\":\"Invalid type\",\"IssueType\":null}");
            Assert.That(this.Run("Import", json), Is.Not.Zero);

            var invalid = this.ReadIssue(importedJson);
            invalid["IssueType"] = "UNKNOWN";
            File.WriteAllText(this.GetIssueFile(importedJson), invalid.ToJsonString());
            Assert.That(this.Run("Validate", importedJson), Is.EqualTo(1));
            Assert.That(this.Output, Does.Contain("Unknown issue type"));

            string duplicate = invalid.ToJsonString().Replace(
                "\"IssueType\":\"UNKNOWN\"", "\"IssueType\":\"BUG\",\"IssueType\":\"BUG\"", StringComparison.Ordinal);
            File.WriteAllText(this.GetIssueFile(importedJson), duplicate);
            Assert.That(this.Run("Validate", importedJson), Is.EqualTo(1));
            Assert.That(this.Output, Does.Contain("IssueType must appear once"));
        }
    }
}
