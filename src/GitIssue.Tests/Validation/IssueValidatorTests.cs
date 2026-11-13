using System.Collections.Generic;
using System.Linq;
using GitIssue.Fields;
using GitIssue.Issues;
using GitIssue.Issues.Json;
using GitIssue.Validation;
using GitIssue.Values;
using NUnit.Framework;

namespace GitIssue.Tests.Validation
{
    [TestFixture]
    public class IssueValidatorTests
    {
        private static readonly IssueKey Key = IssueKey.Create("ISSUE-1");

        [TestCase("{", "invalid JSON")]
        [TestCase("[]", "issue file is not a JSON object")]
        [TestCase("null", "issue file is not a JSON object")]
        public void ValidateJson_RejectsInvalidDocuments(string json, string expected)
        {
            IReadOnlyList<IssueValidationError> errors = IssueValidator.ValidateJson(json, Key, new IssueConfiguration().GetSchema("ISSUE"));

            Assert.That(errors.Select(error => error.Message), Has.Some.Contains(expected));
        }

        [Test]
        public void ValidateJson_ReportsUnknownFieldAndMismatchedKey()
        {
            string json = """
                {
                  "Key": "ISSUE-2",
                  "Title": "Title",
                  "Description": "Description",
                  "Author": "Ada <ada@example.com>",
                  "Created": "2026-11-08T03:25:00Z",
                  "Updated": "2026-11-08T03:25:00Z",
                  "Extra": "value"
                }
                """;

            IReadOnlyList<IssueValidationError> errors = IssueValidator.ValidateJson(json, Key, new IssueConfiguration().GetSchema("ISSUE"));

            Assert.Multiple(() =>
            {
                Assert.That(errors.Select(error => error.ToString()), Has.Some.Contains("Extra: unknown field"));
                Assert.That(errors.Select(error => error.ToString()), Has.Some.Contains("does not match issue key"));
            });
        }

        [Test]
        public void ValidateJson_ReportsWrongShapesAndInvalidArrayItems()
        {
            Dictionary<FieldKey, FieldInfo> fields = new Dictionary<FieldKey, FieldInfo>
            {
                { FieldKey.Create("Title"), new FieldInfo<GitIssue.Values.String, JsonValueField>(true) },
                { FieldKey.Create("Labels"), new FieldInfo<Label, JsonArrayField>(true) },
                { FieldKey.Create("OtherLabels"), new FieldInfo<Label, JsonArrayField>(false) },
            };
            string json = """{"Title": ["wrong"], "Labels": ["valid", null, {}, "two words"], "OtherLabels": "wrong"}""";

            IReadOnlyList<IssueValidationError> errors = IssueValidator.ValidateJson(json, Key, fields);

            Assert.Multiple(() =>
            {
                Assert.That(errors.Select(error => error.ToString()), Has.Some.Contains("Title: value is not a scalar"));
                Assert.That(errors.Select(error => error.ToString()), Has.Some.Contains("Labels: array item is not a value"));
                Assert.That(errors.Select(error => error.ToString()), Has.Some.Contains("not a valid Label"));
                Assert.That(errors.Select(error => error.ToString()), Has.Some.Contains("OtherLabels: value is not an array"));
            });
        }

        [TestCase("{}", "required field is missing")]
        [TestCase("{\"Labels\":null}", "required field is missing")]
        [TestCase("{\"Labels\":[]}", "required field is empty")]
        public void ValidateJson_ReportsMissingOrEmptyRequiredArray(string json, string expected)
        {
            Dictionary<FieldKey, FieldInfo> fields = new Dictionary<FieldKey, FieldInfo>
            {
                { FieldKey.Create("Labels"), new FieldInfo<Label, JsonArrayField>(true) },
            };

            IReadOnlyList<IssueValidationError> errors = IssueValidator.ValidateJson(json, Key, fields);

            Assert.That(errors.Select(error => error.Message), Has.Some.Contains(expected));
        }
    }
}
