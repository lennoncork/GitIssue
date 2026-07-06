using GitIssue.Fields;
using GitIssue.Formatters;
using GitIssue.Issues;
using NUnit.Framework;

namespace GitIssue.Tests.FormattingTests
{
    [TestFixture]
    public class DelegateFormatterTests
    {
        [TestFixture]
        public class Format : DelegateFormatterTests
        {
            [Test]
            public void Default_Formats_Field_Using_ToString()
            {
                FieldKey key = FieldKey.Create(nameof(Format.Default_Formats_Field_Using_ToString));
                const string value = "field-value";
                IField field = Moqs.CreateField(key, value);
                DelegateFormatter formatter = DelegateFormatter.Default;

                string? output = formatter.Format(field);

                Assert.That(output, Is.EqualTo(field.ToString()));
            }

            [Test]
            public void Default_Formats_Issue_Using_ToString()
            {
                IReadOnlyIssue issue = Moqs.CreateIssue(nameof(Format.Default_Formats_Issue_Using_ToString));
                DelegateFormatter formatter = DelegateFormatter.Default;

                string? output = formatter.Format(issue);

                Assert.That(output, Is.EqualTo(issue.ToString()));
            }

            [Test]
            public void Uses_Both_Delegates_When_Provided()
            {
                IReadOnlyIssue issue = Moqs.CreateIssue(nameof(Format.Uses_Both_Delegates_When_Provided));
                FieldKey key = FieldKey.Create(nameof(Format.Uses_Both_Delegates_When_Provided));
                const string value = "field-value";
                IField field = Moqs.CreateField(key, value);

                DelegateFormatter formatter = new DelegateFormatter(
                    i => $"ISSUE:{i.Key}",
                    f => $"FIELD:{f.Key}");

                string? issueOutput = formatter.Format(issue);
                string? fieldOutput = formatter.Format(field);

                Assert.That(issueOutput, Is.EqualTo($"ISSUE:{issue.Key}"));
                Assert.That(fieldOutput, Is.EqualTo($"FIELD:{field.Key}"));
            }

            [Test]
            public void Uses_Field_Formatter_Delegate()
            {
                FieldKey key = FieldKey.Create(nameof(Format.Uses_Field_Formatter_Delegate));
                const string value = "field-value";
                IField field = Moqs.CreateField(key, value);
                DelegateFormatter formatter = new DelegateFormatter((IField f) => $"FIELD:{f.Key}");

                string? output = formatter.Format(field);

                Assert.That(output, Is.EqualTo($"FIELD:{field.Key}"));
            }

            [Test]
            public void Uses_Issue_Formatter_Delegate()
            {
                IReadOnlyIssue issue = Moqs.CreateIssue(nameof(Format.Uses_Issue_Formatter_Delegate));
                DelegateFormatter formatter = new DelegateFormatter((IReadOnlyIssue i) => $"ISSUE:{i.Key}");

                string? output = formatter.Format(issue);

                Assert.That(output, Is.EqualTo($"ISSUE:{issue.Key}"));
            }
        }
    }
}