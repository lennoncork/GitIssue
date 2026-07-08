using System;
using System.Collections.Generic;
using GitIssue.Values;
using NUnit.Framework;
using VersionValue = GitIssue.Values.Version;

namespace GitIssue.Tests.ValueTests
{
    [TestFixture]
    public class VersionValueTests : JsonValueTests<VersionValue>
    {
        [TestFixture]
        public class TypeConverter : VersionValueTests
        {
            [TestCaseSource(typeof(CanConvertTestCases))]
            public bool CanConvert(Type type)
            {
                return this.HasConverter(type);
            }

            [TestCaseSource(typeof(ConvertFromStringTestCases))]
            public VersionValue Convert(object value)
            {
                return (VersionValue)this.UseConverter(value);
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
                    const string v = "1.2.3-alpha+build";
                    VersionValue.Parse(v);
                    yield return new TestCaseData(v)
                        .Returns(VersionValue.Parse(v))
                        .SetName("{m}FromStringWithVersion");
                    yield return new TestCaseData(new ValueMetadata(v, string.Empty))
                        .Returns(VersionValue.Parse(v))
                        .SetName("{m}FromMetadataWithVersion");
                }
            }
        }

        [TestFixture]
        public class TryParse : VersionValueTests
        {
            [TestCaseSource(typeof(TryParseTestCases))]
            public bool Test(string value, VersionValue expected)
            {
                if (VersionValue.TryParse(value, out VersionValue result))
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
                    const string valid = "1.2.3-alpha+build";
                    yield return new TestCaseData(valid, VersionValue.Parse(valid))
                        .Returns(true)
                        .SetName("ParsesValidVersion");
                    yield return new TestCaseData("not-a-version", default(VersionValue))
                        .Returns(false)
                        .SetName("ReturnsFalseForInvalidVersion");
                }
            }
        }

        [TestFixture]
        public new class ToString : VersionValueTests
        {
            [TestCaseSource(typeof(ToStringTestCases))]
            public string Test(VersionValue value)
            {
                return value.ToString();
            }

            public class ToStringTestCases : ValueTestCases
            {
                public override IEnumerator<TestCaseData> GetEnumerator()
                {
                    const string v = "1.2.3-alpha+build";
                    yield return new TestCaseData(VersionValue.Parse(v))
                        .Returns(v)
                        .SetName("ReturnsVersionString");
                }
            }
        }

        [TestFixture]
        public class ToJson : VersionValueTests
        {
            [TestCaseSource(typeof(ConvertToJsonTestCases))]
            public string Tests(VersionValue value)
            {
                return base.ConvertToJson(value);
            }

            public class ConvertToJsonTestCases : ValueTestCases
            {
                public override IEnumerator<TestCaseData> GetEnumerator()
                {
                    const string v = "1.2.3-alpha+build";
                    yield return new TestCaseData(VersionValue.Parse(v))
                        .Returns("\"" + v + "\"")
                        .SetName("ConvertsToJsonString");
                }
            }
        }

        [TestFixture]
        public new class Equals : VersionValueTests
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
                    const string v = "1.2.3";
                    yield return new TestCaseData(VersionValue.Parse(v), VersionValue.Parse(v))
                        .Returns(true)
                        .SetName("ReturnsTrueForSameVersion");
                    yield return new TestCaseData(VersionValue.Parse(v), null)
                        .Returns(false)
                        .SetName("ReturnsFalseComparedToNull");
                }
            }
        }
    }
}