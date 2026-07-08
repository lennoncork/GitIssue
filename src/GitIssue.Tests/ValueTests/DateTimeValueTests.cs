using System;
using System.Collections.Generic;
using System.Globalization;
using GitIssue.Values;
using NUnit.Framework;
using DateTime = System.DateTime;
using DateTimeValue = GitIssue.Values.DateTime;

namespace GitIssue.Tests.ValueTests
{
    [TestFixture]
    public class DateTimeValueTests : JsonValueTests<DateTimeValue, DateTime>
    {
        [TestFixture]
        public class TypeConverter : DateTimeValueTests
        {
            [TestCaseSource(typeof(CanConvertTestCases))]
            public bool CanConvert(Type type)
            {
                return this.HasConverter(type);
            }

            [TestCaseSource(typeof(ConvertFromStringTestCases))]
            public DateTimeValue Convert(object value)
            {
                return (DateTimeValue)this.UseConverter(value);
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
                    const string dt = "2020-01-02T03:04:05Z";
                    DateTimeValue.TryParse(dt, out DateTimeValue parsed);
                    yield return new TestCaseData(dt)
                        .Returns(parsed)
                        .SetName("{m}FromStringWithDateTime");
                    yield return new TestCaseData(new ValueMetadata(dt, string.Empty))
                        .Returns(parsed)
                        .SetName("{m}FromMetadataWithDateTime");
                }
            }
        }

        [TestFixture]
        public class TryParse : DateTimeValueTests
        {
            [TestCaseSource(typeof(TryParseTestCases))]
            public bool Test(string value, DateTimeValue expected)
            {
                if (DateTimeValue.TryParse(value, out DateTimeValue result))
                {
                    Assert.That(expected.Equals(result), Is.True);
                    return true;
                }

                return false;
            }

            public class TryParseTestCases : ValueTestCases
            {
                public override IEnumerator<TestCaseData> GetEnumerator()
                {
                    const string valid = "2020-01-02T03:04:05Z";
                    DateTimeValue.TryParse(valid, out DateTimeValue parsed);
                    yield return new TestCaseData(valid, parsed)
                        .Returns(true)
                        .SetName("ParsesValidDateTime");
                    yield return new TestCaseData("not-a-date", default(DateTimeValue))
                        .Returns(false)
                        .SetName("ReturnsFalseForInvalidDateTime");
                }
            }
        }

        [TestFixture]
        public new class ToString : DateTimeValueTests
        {
            [TestCaseSource(typeof(ToStringTestCases))]
            public string Test(DateTimeValue value)
            {
                return value.ToString();
            }

            public class ToStringTestCases : ValueTestCases
            {
                public override IEnumerator<TestCaseData> GetEnumerator()
                {
                    DateTime dt = DateTime.SpecifyKind(new DateTime(2020, 1, 2, 3, 4, 5), DateTimeKind.Utc);
                    DateTimeValue dv = dt;
                    yield return new TestCaseData(dv)
                        .Returns(dt.ToString(CultureInfo.InvariantCulture))
                        .SetName("ReturnsInvariantString");
                }
            }
        }

        [TestFixture]
        public class Item : DateTimeValueTests
        {
            [TestCaseSource(typeof(GetItemTestCases))]
            public DateTime Tests(DateTimeValue value)
            {
                return base.GetItem(value);
            }

            public class GetItemTestCases : ValueTestCases
            {
                public override IEnumerator<TestCaseData> GetEnumerator()
                {
                    DateTime dt = new DateTime(2021, 2, 3, 4, 5, 6);
                    DateTimeValue dv = dt;
                    yield return new TestCaseData(dv)
                        .Returns(dt)
                        .SetName("ReturnsUnderlyingDateTime");
                }
            }
        }

        [TestFixture]
        public class ToJson : DateTimeValueTests
        {
            [TestCaseSource(typeof(ConvertToJsonTestCases))]
            public string Tests(DateTimeValue value)
            {
                return base.ConvertToJson(value);
            }

            public class ConvertToJsonTestCases : ValueTestCases
            {
                public override IEnumerator<TestCaseData> GetEnumerator()
                {
                    DateTime dt = new DateTime(2020, 1, 2, 3, 4, 5);
                    DateTimeValue dv = dt;
                    yield return new TestCaseData(dv)
                        .Returns("\"2020-01-02T03:04:05\"")
                        .SetName("ConvertsToJsonIsoLikeString");
                }
            }
        }

        [TestFixture]
        public new class Equals : DateTimeValueTests
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
                    DateTime dt = new DateTime(2020, 1, 2, 3, 4, 5);
                    DateTimeValue dv1 = dt;
                    DateTimeValue dv2 = dt;
                    yield return new TestCaseData(dv1, dv2)
                        .Returns(true)
                        .SetName("ReturnsTrueForSameDateTime");
                    yield return new TestCaseData(dv1, null)
                        .Returns(false)
                        .SetName("ReturnsFalseComparedToNull");
                }
            }
        }
    }
}