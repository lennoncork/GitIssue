using System;
using System.Collections.Generic;
using GitIssue.Values;
using NUnit.Framework;

namespace GitIssue.Tests.ValueTests
{
    [TestFixture]
    public class NumberValueTests : JsonValueTests<Number, double>
    {
        public class TypeConverter : NumberValueTests
        {
            [TestCaseSource(typeof(CanConvertTestCases))]
            public bool CanConvert(Type type)
            {
                return HasConverter(type);
            }

            [TestCaseSource(typeof(ConvertFromStringTestCases))]
            public Number Convert(object value)
            {
                return (Number)UseConverter(value);
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
                    yield return new TestCaseData("123")
                        .Returns(Number.Parse("123"))
                        .SetName("{m}FromStringWithCorrectNumber");
                    yield return new TestCaseData(new ValueMetadata("123", ""))
                        .Returns(Number.Parse("123"))
                        .SetName("{m}FromMetadataWithNumber");
                }
            }
        }

        public new class ToString : NumberValueTests
        {
            [TestCaseSource(typeof(ToStringTestCases))]
            public string Test(Number label)
            {
                return label.ToString();
            }

            public class ToStringTestCases : ValueTestCases
            {
                public override IEnumerator<TestCaseData> GetEnumerator()
                {
                    yield return new TestCaseData(Number.Parse("123.456"))
                        .Returns("123.456")
                        .SetName("ReturnNumberValue");
                }
            }
        }

        public class TryParse : NumberValueTests
        {
            [TestCaseSource(typeof(TryParseTestCases))]
            public bool Test(string value, Number expected)
            {
                if (Number.TryParse(value, out var result))
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
                    yield return new TestCaseData("321.99", Number.Parse("321.99"))
                        .Returns(true)
                        .SetName("ParsesNumberSuccessfully");
                    yield return new TestCaseData("foo", null)
                        .Returns(false)
                        .SetName("FailsWithStringInput");
                }
            }
        }

        public class Item : NumberValueTests
        {
            [TestCaseSource(typeof(GetItemTestCases))]
            public double Tests(Number number)
            {
                return base.GetItem(number);
            }

            public class GetItemTestCases : ValueTestCases
            {
                public override IEnumerator<TestCaseData> GetEnumerator()
                {
                    yield return new TestCaseData(Number.Parse("456.9"))
                        .Returns(456.9)
                        .SetName("ReturnsDouble");
                }
            }
        }

        public class ToJson : NumberValueTests
        {
            [TestCaseSource(typeof(ConvertToJsonTestCases))]
            public string Tests(Number number)
            {
                return base.ConvertToJson(number);
            }

            public class ConvertToJsonTestCases : ValueTestCases
            {
                public override IEnumerator<TestCaseData> GetEnumerator()
                {
                    yield return new TestCaseData(Number.Parse("321.99"))
                        .Returns("321.99")
                        .SetName("ConvertsToJsonString");
                }
            }
        }

        public new class Equals : NumberValueTests
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
                    yield return new TestCaseData(Number.Parse("321.99"), Number.Parse("321.99"))
                        .Returns(true)
                        .SetName("ReturnsTrueForSameNumber");
                    yield return new TestCaseData(Number.Parse("321.99"), null)
                        .Returns(false)
                        .SetName("ReturnsFalseComparedToNull");
                }
            }
        }
    }
}