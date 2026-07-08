using System.Collections.Generic;
using GitIssue.Values;
using NUnit.Framework;

namespace GitIssue.Tests.ValueTests
{
    [TestFixture]
    public class EnumeratedValueTests : JsonValueTests<Enumerated, string>
    {
        [TestFixture]
        public class TryParse : EnumeratedValueTests
        {
            [TestCaseSource(typeof(TryParseTestCases))]
            public bool Test(ValueMetadata metadata, bool expectedSuccess, string expectedValue)
            {
                bool success = Enumerated.TryParse(metadata, out Enumerated result);

                Assert.That(success, Is.EqualTo(expectedSuccess));
                if (success)
                {
                    Assert.That(result.Item, Is.EqualTo(expectedValue));
                }

                return success;
            }

            public class TryParseTestCases : ValueTestCases
            {
                public override IEnumerator<TestCaseData> GetEnumerator()
                {
                    ValueMetadata validMeta = new ValueMetadata("S1", "[S1, S2, S3]");
                    yield return new TestCaseData(validMeta, true, "S1")
                        .Returns(true)
                        .SetName("ParsesValueWhenInMetadataList");

                    ValueMetadata invalidMeta = new ValueMetadata("S4", "[S1, S2, S3]");
                    yield return new TestCaseData(invalidMeta, false, string.Empty)
                        .Returns(false)
                        .SetName("ReturnsFalseWhenValueNotInList");
                }
            }
        }

        [TestFixture]
        public new class ToString : EnumeratedValueTests
        {
            [TestCaseSource(typeof(ToStringTestCases))]
            public string Test(Enumerated value)
            {
                return value.ToString();
            }

            public class ToStringTestCases : ValueTestCases
            {
                public override IEnumerator<TestCaseData> GetEnumerator()
                {
                    Enumerated e = new Enumerated("S1", new[] { "S1", "S2" });
                    yield return new TestCaseData(e)
                        .Returns("S1")
                        .SetName("ReturnsItemString");
                }
            }
        }

        [TestFixture]
        public class Item : EnumeratedValueTests
        {
            [TestCaseSource(typeof(GetItemTestCases))]
            public string Tests(Enumerated value)
            {
                return base.GetItem(value);
            }

            public class GetItemTestCases : ValueTestCases
            {
                public override IEnumerator<TestCaseData> GetEnumerator()
                {
                    Enumerated e = new Enumerated("S2", new[] { "S1", "S2" });
                    yield return new TestCaseData(e)
                        .Returns("S2")
                        .SetName("ReturnsUnderlyingString");
                }
            }
        }

        [TestFixture]
        public class ToJson : EnumeratedValueTests
        {
            [TestCaseSource(typeof(ConvertToJsonTestCases))]
            public string Tests(Enumerated value)
            {
                return base.ConvertToJson(value);
            }

            public class ConvertToJsonTestCases : ValueTestCases
            {
                public override IEnumerator<TestCaseData> GetEnumerator()
                {
                    Enumerated e = new Enumerated("S1", new[] { "S1", "S2" });
                    yield return new TestCaseData(e)
                        .Returns("\"S1\"")
                        .SetName("ConvertsToJsonString");
                }
            }
        }

        [TestFixture]
        public new class Equals : EnumeratedValueTests
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
                    Enumerated e1 = new Enumerated("S1", new[] { "S1", "S2" });
                    Enumerated e2 = new Enumerated("S1", new[] { "S1", "S2" });
                    yield return new TestCaseData(e1, e2)
                        .Returns(true)
                        .SetName("ReturnsTrueForSameEnumerated");
                    yield return new TestCaseData(e1, null)
                        .Returns(false)
                        .SetName("ReturnsFalseComparedToNull");
                }
            }
        }
    }
}