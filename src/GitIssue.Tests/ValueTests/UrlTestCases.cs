using System;
using System.Collections.Generic;
using GitIssue.Values;
using NUnit.Framework;

namespace GitIssue.Tests.ValueTests
{
    [TestFixture]
    public class UrlValueTests : JsonValueTests<Url, string>
    {
        public class TypeConverter : UrlValueTests
        {
            [TestCaseSource(typeof(CanConvertTestCases))]
            public bool CanConvert(Type type)
            {
                return HasConverter(type);
            }

            [TestCaseSource(typeof(ConvertFromStringTestCases))]
            public Url Convert(object value)
            {
                return (Url)UseConverter(value);
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
                    yield return new TestCaseData("http://www.google.com/")
                        .Returns(Url.Parse("http://www.google.com/"))
                        .SetName("{m}FromStringWithCorrectUrl");
                    yield return new TestCaseData(new ValueMetadata("http://www.google.com/", ""))
                        .Returns(Url.Parse("http://www.google.com/"))
                        .SetName("{m}FromMetadataWithString");
                }
            }
        }

        public class TryParse : UrlValueTests
        {
            [TestCaseSource(typeof(TryParseTestCases))]
            public bool Test(string value, Url expected)
            {
                if (Url.TryParse(value, out var result))
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
                    yield return new TestCaseData("http://www.google.com/", Url.Parse("http://www.google.com/"))
                        .Returns(true)
                        .SetName("ParsesUrlSuccessfully");
                }
            }
        }

        public new class ToString : UrlValueTests
        {
            [TestCaseSource(typeof(ToStringTestCases))]
            public string Test(Url value)
            {
                return value.ToString();
            }

            public class ToStringTestCases : ValueTestCases
            {
                public override IEnumerator<TestCaseData> GetEnumerator()
                {
                    yield return new TestCaseData(Url.Parse("http://www.google.com/"))
                        .Returns("http://www.google.com/")
                        .SetName("ReturnsEmailString");
                }
            }
        }

        public class Item : UrlValueTests
        {
            [TestCaseSource(typeof(GetItemTestCases))]
            public string Tests(Url value)
            {
                return base.GetItem(value);
            }

            public class GetItemTestCases : ValueTestCases
            {
                public override IEnumerator<TestCaseData> GetEnumerator()
                {
                    yield return new TestCaseData(Url.Parse("http://www.google.com/"))
                        .Returns("http://www.google.com/")
                        .SetName("ReturnsString");
                }
            }
        }

        public class ToJson : UrlValueTests
        {
            [TestCaseSource(typeof(ConvertToJsonTestCases))]
            public string Tests(Url value)
            {
                return base.ConvertToJson(value);
            }

            public class ConvertToJsonTestCases : ValueTestCases
            {
                public override IEnumerator<TestCaseData> GetEnumerator()
                {
                    yield return new TestCaseData(Url.Parse("http://www.google.com/"))
                        .Returns("\"http://www.google.com/\"")
                        .SetName("ReturnsJsonString");
                }
            }
        }

        public new class Equals : UrlValueTests
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
                    yield return new TestCaseData(Url.Parse("http://www.google.com/"), Url.Parse("http://www.google.com/"))
                        .Returns(true)
                        .SetName("ReturnsTrueForSameUrl"); 
                    yield return new TestCaseData(Url.Parse("http://www.google.com/"), Url.Parse("http://www.apple.com/"))
                        .Returns(false)
                        .SetName("ReturnsFalseForDifferentUrl");
                    yield return new TestCaseData(Url.Parse("http://www.google.com/"), null)
                        .Returns(false)
                        .SetName("ReturnsFalseComparedToNull");
                }
            }
        }
    }
}