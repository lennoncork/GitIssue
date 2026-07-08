using System;
using System.Collections.Generic;
using GitIssue.Values;
using NUnit.Framework;

namespace GitIssue.Tests.ValueTests
{
    [TestFixture]
    public class MarkdownValueTests : JsonValueTests<Markdown, string>
    {
        [TestFixture]
        public class TypeConverter : MarkdownValueTests
        {
            [TestCaseSource(typeof(CanConvertTestCases))]
            public bool CanConvert(Type type)
            {
                return this.HasConverter(type);
            }

            [TestCaseSource(typeof(ConvertFromStringTestCases))]
            public Markdown Convert(object value)
            {
                return (Markdown)this.UseConverter(value);
            }

            public class CanConvertTestCases : ValueTestCases
            {
                public override IEnumerator<TestCaseData> GetEnumerator()
                {
                    yield return new TestCaseData(typeof(string))
                        .Returns(true)
                        .SetName("{m}FromString");
                    yield return new TestCaseData(typeof(ValueMetadata))
                        .Returns(true)
                        .SetName("{m}FromMetadata");
                }
            }

            public class ConvertFromStringTestCases : ValueTestCases
            {
                public override IEnumerator<TestCaseData> GetEnumerator()
                {
                    const string markdown = "# Heading";
                    Markdown.TryParse(markdown, out Markdown parsed);
                    yield return new TestCaseData(markdown)
                        .Returns(parsed)
                        .SetName("{m}FromStringWithMarkdown");
                    yield return new TestCaseData(new ValueMetadata(markdown, string.Empty))
                        .Returns(parsed)
                        .SetName("{m}FromMetadataWithMarkdown");
                }
            }
        }

        [TestFixture]
        public class TryParse : MarkdownValueTests
        {
            [TestCaseSource(typeof(TryParseTestCases))]
            public bool Test(string value, Markdown expected)
            {
                if (Markdown.TryParse(value, out Markdown result))
                {
                    Assert.That(expected, Is.EqualTo(result));
                    return true;
                }

                return false;
            }

            public class TryParseTestCases : ValueTestCases
            {
                public override IEnumerator<TestCaseData> GetEnumerator()
                {
                    const string markdown = "# Heading";
                    Markdown.TryParse(markdown, out Markdown parsed);
                    yield return new TestCaseData(markdown, parsed)
                        .Returns(true)
                        .SetName("ParsesMarkdownSuccessfully");
                }
            }
        }

        [TestFixture]
        public new class ToString : MarkdownValueTests
        {
            [TestCaseSource(typeof(ToStringTestCases))]
            public string Test(Markdown value)
            {
                return value.ToString();
            }

            public class ToStringTestCases : ValueTestCases
            {
                public override IEnumerator<TestCaseData> GetEnumerator()
                {
                    const string markdown = "# Heading";
                    Markdown.TryParse(markdown, out Markdown parsed);
                    yield return new TestCaseData(parsed)
                        .Returns(markdown)
                        .SetName("ReturnsMarkdownString");
                }
            }
        }

        [TestFixture]
        public class Item : MarkdownValueTests
        {
            [TestCaseSource(typeof(GetItemTestCases))]
            public string Tests(Markdown value)
            {
                return base.GetItem(value);
            }

            public class GetItemTestCases : ValueTestCases
            {
                public override IEnumerator<TestCaseData> GetEnumerator()
                {
                    const string markdown = "# Heading";
                    Markdown.TryParse(markdown, out Markdown parsed);
                    yield return new TestCaseData(parsed)
                        .Returns(markdown)
                        .SetName("ReturnsString");
                }
            }
        }

        [TestFixture]
        public class ToJson : MarkdownValueTests
        {
            [TestCaseSource(typeof(ConvertToJsonTestCases))]
            public string Tests(Markdown value)
            {
                return base.ConvertToJson(value);
            }

            public class ConvertToJsonTestCases : ValueTestCases
            {
                public override IEnumerator<TestCaseData> GetEnumerator()
                {
                    const string markdown = "# Heading";
                    Markdown.TryParse(markdown, out Markdown parsed);
                    yield return new TestCaseData(parsed)
                        .Returns("\"# Heading\"")
                        .SetName("ReturnsJsonString");
                }
            }
        }

        [TestFixture]
        public new class Equals : MarkdownValueTests
        {
            [TestCaseSource(typeof(EqualsTestCases))]
            public bool Tests(object first, object second)
            {
                return base.Equals(first, second);
            }

            public class EqualsTestCases : ValueTestCases
            {
                public override IEnumerator<TestCaseData> GetEnumerator()
                {
                    const string markdown = "# Heading";
                    Markdown.TryParse(markdown, out Markdown parsed);
                    Markdown.TryParse(markdown, out Markdown parsedSame);
                    yield return new TestCaseData(parsed, parsedSame)
                        .Returns(true)
                        .SetName("ReturnsTrueForSameMarkdown");
                    Markdown.TryParse("# Other", out Markdown parsedOther);
                    yield return new TestCaseData(parsed, parsedOther)
                        .Returns(false)
                        .SetName("ReturnsFalseForDifferentMarkdown");
                    yield return new TestCaseData(parsed, null)
                        .Returns(false)
                        .SetName("ReturnsFalseComparedToNull");
                }
            }
        }
    }
}