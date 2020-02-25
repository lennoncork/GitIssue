using System;
using System.Collections.Generic;
using GitIssue.Values;
using NUnit.Framework;
using String = GitIssue.Values.String;

namespace GitIssue.Tests.ValueTests
{
    [TestFixture]
    public class StringValueTests : JsonValueTests<String, string>
    {
        public class TypeConverter : StringValueTests
        {
            [TestCaseSource(typeof(CanConvertTestCases))]
            public bool CanConvert(Type type)
            {
                return HasConverter(type);
            }

            [TestCaseSource(typeof(ConvertFromStringTestCases))]
            public String Convert(object value)
            {
                return (String)UseConverter(value);
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
                    yield return new TestCaseData("string")
                        .Returns(String.Parse("string"))
                        .SetName("{m}FromStringWithCorrectLabel");
                    yield return new TestCaseData(new ValueMetadata("string", ""))
                        .Returns(String.Parse("string"))
                        .SetName("{m}FromMetadataWithString");
                }
            }
        }

        public class TryParse : StringValueTests
        {
            [TestCaseSource(typeof(TryParseTestCases))]
            public bool Test(string value, String expected)
            {
                if (String.TryParse(value, out var result))
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
                    yield return new TestCaseData("str", String.Parse("str"))
                        .Returns(true)
                        .SetName("ParsesLabelSuccessfully");
                }
            }
        }

        public class Item : StringValueTests
        {
            [TestCaseSource(typeof(Item.GetItemTestCases))]
            public string Tests(String str)
            {
                return base.GetItem(str);
            }

            public class GetItemTestCases : ValueTestCases
            {
                public override IEnumerator<TestCaseData> GetEnumerator()
                {
                    yield return new TestCaseData(String.Parse("string"))
                        .Returns("string")
                        .SetName("ReturnsString");
                }
            }
        }

        public class ToJson : StringValueTests
        {
            [TestCaseSource(typeof(ToJson.ConvertToJsonTestCases))]
            public string Tests(String str)
            {
                return base.ConvertToJson(str);
            }

            public class ConvertToJsonTestCases : ValueTestCases
            {
                public override IEnumerator<TestCaseData> GetEnumerator()
                {
                    yield return new TestCaseData(String.Parse("string"))
                        .Returns("\"string\"")
                        .SetName("ConvertsToJsonString");
                }
            }
        }

        public new class Equals : StringValueTests
        {
            [TestCaseSource(typeof(Equals.EqualsTestCases))]
            public bool Tests(object first, object second)
            {
                return base.Equals(first, second);
            }

            public class EqualsTestCases : ValueTestCases
            {
                public override IEnumerator<TestCaseData> GetEnumerator()
                {
                    yield return new TestCaseData(String.Parse("string"), String.Parse("string"))
                        .Returns(true)
                        .SetName("ReturnsTrueForSameLabel");
                    yield return new TestCaseData(String.Parse("string"), null)
                        .Returns(false)
                        .SetName("ReturnsFalseComparedToNull");
                }
            }
        }
    }
}