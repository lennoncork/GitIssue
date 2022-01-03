using System;
using System.Collections.Generic;
using GitIssue.Values;
using NUnit.Framework;

namespace GitIssue.Tests.ValueTests
{
    [TestFixture]
    public class EmailValueTests : JsonValueTests<Email, string>
    {
        [TestFixture]
        public class TypeConverter : EmailValueTests
        {
            [TestCaseSource(typeof(CanConvertTestCases))]
            public bool CanConvert(Type type)
            {
                return HasConverter(type);
            }

            [TestCaseSource(typeof(ConvertFromStringTestCases))]
            public Email Convert(object value)
            {
                return (Email)UseConverter(value);
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
                    yield return new TestCaseData("foo.bar@gmail.com")
                        .Returns(Email.Parse("foo.bar@gmail.com"))
                        .SetName("{m}FromStringWithCorrectLabel");
                    yield return new TestCaseData(new ValueMetadata("foo.bar@gmail.com", ""))
                        .Returns(Email.Parse("foo.bar@gmail.com"))
                        .SetName("{m}FromMetadataWithString");
                }
            }
        }

        [TestFixture]
        public class TryParse : EmailValueTests
        {
            [TestCaseSource(typeof(TryParseTestCases))]
            public bool Test(string value, Email expected)
            {
                if (Email.TryParse(value, out var result))
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
                    yield return new TestCaseData("foo.bar@gmail.com", Email.Parse("foo.bar@gmail.com"))
                        .Returns(true)
                        .SetName("ParsesEmailSuccessfully");
                }
            }
        }

        [TestFixture]
        public new class ToString : EmailValueTests
        {
            [TestCaseSource(typeof(ToStringTestCases))]
            public string Test(Email value)
            {
                return value.ToString();
            }

            public class ToStringTestCases : ValueTestCases
            {
                public override IEnumerator<TestCaseData> GetEnumerator()
                {
                    yield return new TestCaseData(Email.Parse("foo.bar@gmail.com"))
                        .Returns("foo.bar@gmail.com")
                        .SetName("ReturnsEmailString");
                }
            }
        }

        [TestFixture]
        public class Item : EmailValueTests
        {
            [TestCaseSource(typeof(GetItemTestCases))]
            public string Tests(Email value)
            {
                return base.GetItem(value);
            }

            public class GetItemTestCases : ValueTestCases
            {
                public override IEnumerator<TestCaseData> GetEnumerator()
                {
                    yield return new TestCaseData(Email.Parse("foo.bar@gmail.com"))
                        .Returns("foo.bar@gmail.com")
                        .SetName("ReturnsString");
                }
            }
        }

        [TestFixture]
        public class ToJson : EmailValueTests
        {
            [TestCaseSource(typeof(ConvertToJsonTestCases))]
            public string Tests(Email value)
            {
                return base.ConvertToJson(value);
            }

            public class ConvertToJsonTestCases : ValueTestCases
            {
                public override IEnumerator<TestCaseData> GetEnumerator()
                {
                    yield return new TestCaseData(Email.Parse("foo.bar@gmail.com"))
                        .Returns("\"foo.bar@gmail.com\"")
                        .SetName("ReturnsJsonString");
                }
            }
        }

        [TestFixture]
        public new class Equals : EmailValueTests
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
                    yield return new TestCaseData(Email.Parse("foo.bar@gmail.com"), Email.Parse("foo.bar@gmail.com"))
                        .Returns(true)
                        .SetName("ReturnsTrueForSameEmail");
                    yield return new TestCaseData(Email.Parse("foo.bar@gmail.com"), Email.Parse("bar.foo@gmail.com"))
                        .Returns(false)
                        .SetName("ReturnsFalseForDifferentEmail");
                    yield return new TestCaseData(Email.Parse("foo.bar@gmail.com"), null)
                        .Returns(false)
                        .SetName("ReturnsFalseComparedToNull");
                }
            }
        }
    }
}