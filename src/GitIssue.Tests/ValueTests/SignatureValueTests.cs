using System;
using System.Collections.Generic;
using GitIssue.Values;
using NUnit.Framework;

namespace GitIssue.Tests.ValueTests
{
    [TestFixture]
    public class SignatureValueTests : JsonValueTests<Signature>
    {
        [TestFixture]
        public class TypeConverter : SignatureValueTests
        {
            [TestCaseSource(typeof(CanConvertTestCases))]
            public bool CanConvert(Type type)
            {
                return HasConverter(type);
            }

            [TestCaseSource(typeof(ConvertFromStringTestCases))]
            public Signature Convert(object value)
            {
                return (Signature)UseConverter(value);
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
                    yield return new TestCaseData("Foo Bar <foo.bar@gmail.com>")
                        .Returns(Signature.Parse("Foo Bar <foo.bar@gmail.com>"))
                        .SetName("{m}FromStringWithCorrectLabel");
                    yield return new TestCaseData(new ValueMetadata("Foo Bar <foo.bar@gmail.com>", ""))
                        .Returns(Signature.Parse("Foo Bar <foo.bar@gmail.com>"))
                        .SetName("{m}FromMetadataWithString");
                }
            }
        }

        [TestFixture]
        public class TryParse : SignatureValueTests
        {
            [TestCaseSource(typeof(TryParseTestCases))]
            public bool Test(string value, Signature expected)
            {
                if (Signature.TryParse(value, out var result))
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
                    yield return new TestCaseData("Foo Bar <foo.bar@gmail.com>", Signature.Parse("Foo Bar <foo.bar@gmail.com>"))
                        .Returns(true)
                        .SetName("ParsesSignatureSuccessfully");
                }
            }
        }

        [TestFixture]
        public new class ToString : SignatureValueTests
        {
            [TestCaseSource(typeof(ToStringTestCases))]
            public string Test(Signature value)
            {
                return value.ToString();
            }

            public class ToStringTestCases : ValueTestCases
            {
                public override IEnumerator<TestCaseData> GetEnumerator()
                {
                    yield return new TestCaseData(Signature.Parse("Foo Bar <foo.bar@gmail.com>"))
                        .Returns("Foo Bar <foo.bar@gmail.com>")
                        .SetName("ReturnsSignatureString");
                }
            }
        }

        [TestFixture]
        public class ToJson : SignatureValueTests
        {
            [TestCaseSource(typeof(ConvertToJsonTestCases))]
            public string Tests(Signature value)
            {
                return base.ConvertToJson(value);
            }

            public class ConvertToJsonTestCases : ValueTestCases
            {
                public override IEnumerator<TestCaseData> GetEnumerator()
                {
                    yield return new TestCaseData(Signature.Parse("Foo Bar <foo.bar@gmail.com>"))
                        .Returns("\"Foo Bar <foo.bar@gmail.com>\"")
                        .SetName("ReturnsJsonString");
                }
            }
        }

        [TestFixture]
        public new class Equals : SignatureValueTests
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
                    yield return new TestCaseData(Signature.Parse("Foo Bar <foo.bar@gmail.com>"), Signature.Parse("Foo Bar <foo.bar@gmail.com>"))
                        .Returns(true)
                        .SetName("ReturnsTrueForSameSignature"); 
                    yield return new TestCaseData(Signature.Parse("Foo Bar <foo.bar@gmail.com>"), Signature.Parse("Bar Foo <bar.foo@gmail.com>"))
                        .Returns(false)
                        .SetName("ReturnsFalseForDifferentSignature");
                    yield return new TestCaseData(Signature.Parse("Foo Bar <foo.bar@gmail.com>"), null)
                        .Returns(false)
                        .SetName("ReturnsFalseComparedToNull");
                }
            }
        }
    }
}