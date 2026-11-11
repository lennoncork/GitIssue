using System;
using GitIssue.Issues;
using NUnit.Framework;

namespace GitIssue.Tests.Issues
{
    [TestFixture]
    public class IssueKeyTemplateTests
    {
        [Test]
        public void DateTemplate_RendersNestedPathAndHyphenatedKey()
        {
            IssueKeyTemplate template = IssueKeyTemplate.Parse(IssueKeyTemplate.Date);
            IssueKeyContext context = new IssueKeyContext
            {
                Created = new DateTime(2026, 11, 9),
                Uid = "ABCD1234",
            };

            Assert.That(template.RenderKey(context), Is.EqualTo("2026-11-09-ABCD1234"));
            Assert.That(template.RenderPath(context), Is.EqualTo(System.IO.Path.Combine("2026", "11", "09", "ABCD1234")));
        }

        [Test]
        public void DateTemplate_MatchesKeyAndPath()
        {
            IssueKeyTemplate template = IssueKeyTemplate.Parse(IssueKeyTemplate.Date);

            Assert.That(template.TryMatch("2026-11-09-ABCD1234", out IssueKeyContext fromKey), Is.True);
            Assert.That(fromKey.Uid, Is.EqualTo("ABCD1234"));
            Assert.That(fromKey.Created, Is.EqualTo(new DateTime(2026, 11, 9)));

            Assert.That(template.TryMatch("2026/11/09/ABCD1234", out IssueKeyContext fromPath), Is.True);
            Assert.That(fromPath.Uid, Is.EqualTo("ABCD1234"));
        }

        [Test]
        public void NameTemplate_RendersReadableKey()
        {
            IssueKeyTemplate template = IssueKeyTemplate.Parse(IssueKeyTemplate.Name);
            IssueKeyContext context = new IssueKeyContext
            {
                Uid = "ABCD1234",
                Name = "Add login page",
            };

            Assert.That(template.RenderKey(context), Is.EqualTo("ABCD1234 - Add login page"));
            Assert.That(template.RenderPath(context), Is.EqualTo("ABCD1234 - Add login page"));
        }

        [Test]
        public void CustomTemplate_RendersTypeIndexUidAndSlug()
        {
            IssueKeyTemplate template = IssueKeyTemplate.Parse("{type}-{created:yyyyMMdd}-{index:D3}-{uid}-{slug}");
            IssueKeyContext context = new IssueKeyContext
            {
                Type = "FEAT",
                Created = new DateTime(2026, 11, 9),
                Index = 7,
                Uid = "ABCD1234",
                Name = "Add login page",
            };

            Assert.That(template.RenderKey(context), Is.EqualTo("FEAT-20261109-007-ABCD1234-add-login-page"));
            Assert.That(template.TryMatch("FEAT-20261109-007-ABCD1234-add-login-page", out IssueKeyContext matched), Is.True);
            Assert.That(matched.Type, Is.EqualTo("FEAT"));
            Assert.That(matched.Index, Is.EqualTo(7));
            Assert.That(matched.Uid, Is.EqualTo("ABCD1234"));
        }

        [Test]
        public void Parse_RejectsUnknownToken()
        {
            Assert.That(() => IssueKeyTemplate.Parse("{unknown}"), Throws.ArgumentException);
        }

        [Test]
        public void SanitizeSlug_LowercasesAndHyphenates()
        {
            Assert.That(IssueKeyTemplate.SanitizeSlug("Add Login Page"), Is.EqualTo("add-login-page"));
        }

        [Test]
        public void SanitizeType_StripsInvalidCharacters()
        {
            Assert.That(IssueKeyTemplate.SanitizeType("FEAT-1"), Is.EqualTo("FEAT1"));
            Assert.That(IssueKeyTemplate.SanitizeType("   "), Is.EqualTo("ISSUE"));
        }
    }
}
