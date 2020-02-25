using System;
using System.Collections.Generic;
using GitIssue.Values;
using NUnit.Framework;

namespace GitIssue.Tests.ValueTests
{
    [TestFixture]
    public class LabelValueTests : JsonValueTests<Label, string>
    {
        public class TypeConverter : LabelValueTests
        {
            [TestCaseSource(typeof(CanConvertTestCases))]
            public bool CanConvert(Type type)
            {
                return HasConverter(type);
            }

            [TestCaseSource(typeof(ConvertFromStringTestCases))]
            public Label Convert(object value)
            {
                return (Label)UseConverter(value);
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
                    yield return new TestCaseData("label")
                        .Returns(Label.Parse("label"))
                        .SetName("{m}FromStringWithCorrectLabel");
                    yield return new TestCaseData("Label")
                        .Returns(Label.Parse("label"))
                        .SetName("{m}FromStringAsLowerCase");
                    yield return new TestCaseData("label ")
                        .Returns(Label.Parse("label"))
                        .SetName("{m}FromStringAndTrimsEndOfInput");
                    yield return new TestCaseData(" label")
                        .Returns(Label.Parse("label"))
                        .SetName("{m}FromStringAndTrimsStartOfInput");
                    yield return new TestCaseData(new ValueMetadata("String", ""))
                        .Returns(Label.Parse("String"))
                        .SetName("{m}FromMetadataWithStringLabel");
                }
            }
        }

        public class TryParse : LabelValueTests
        {
            [TestCaseSource(typeof(TryParseTestCases))]
            public bool Test(string value, Label expected)
            {
                if (Label.TryParse(value, out var result))
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
                    yield return new TestCaseData("label", Label.Parse("label"))
                        .Returns(true)
                        .SetName("ParsesLabelSuccessfully");
                }
            }
        }

        public new class ToString : LabelValueTests
        {
            [TestCaseSource(typeof(ToStringTestCases))]
            public string Test(Label label)
            {
                return label.ToString();
            }

            public class ToStringTestCases : ValueTestCases
            {
                public override IEnumerator<TestCaseData> GetEnumerator()
                {
                    yield return new TestCaseData(Label.Parse("label"))
                        .Returns("label")
                        .SetName("ReturnLabelValue");
                }
            }
        }

        public class Item : LabelValueTests
        {
            [TestCaseSource(typeof(GetItemTestCases))]
            public string Tests(Label label)
            {
                return base.GetItem(label);
            }

            public class GetItemTestCases : ValueTestCases
            {
                public override IEnumerator<TestCaseData> GetEnumerator()
                {
                    yield return new TestCaseData(Label.Parse("label"))
                        .Returns("label")
                        .SetName("ReturnsStringLabel");
                }
            }
        }

        public class ToJson : LabelValueTests
        {
            [TestCaseSource(typeof(ConvertToJsonTestCases))]
            public string Tests(Label label)
            {
                return base.ConvertToJson(label);
            }

            public class ConvertToJsonTestCases : ValueTestCases
            {
                public override IEnumerator<TestCaseData> GetEnumerator()
                {
                    yield return new TestCaseData(Label.Parse("string"))
                        .Returns("\"string\"")
                        .SetName("ConvertsToJsonString");
                }
            }
        }

        public new class Equals : LabelValueTests
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
                    yield return new TestCaseData(Label.Parse("label"), Label.Parse("label"))
                        .Returns(true)
                        .SetName("ReturnsTrueForSameLabel");
                    yield return new TestCaseData(Label.Parse("label"), null)
                        .Returns(false)
                        .SetName("ReturnsFalseComparedToNull");
                }
            }
        }
    }
}