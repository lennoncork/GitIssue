using System.ComponentModel;
using GitIssue.Values;
using NUnit.Framework;
using String = GitIssue.Values.String;

namespace GitIssue.Tests.ValueTests
{
    [TestFixture]
    public class ValueTypeConverterTests
    {
        [TestFixture]
        public class StringConverter : ValueTypeConverterTests
        {
            [Test]
            public void StringTypeConverter_ConvertTo_String_Uses_ToString()
            {
                String value = String.Parse("string");

                TypeConverter converter = TypeDescriptor.GetConverter(typeof(String));

                object? result = converter.ConvertTo(value, typeof(string));

                Assert.That(result, Is.EqualTo("string"));
            }
        }

        [TestFixture]
        public class EnumConverter : ValueTypeConverterTests
        {
            [Test]
            public void EnumTypeConverter_Converts_From_ValueMetadata()
            {
                TypeConverter converter = TypeDescriptor.GetConverter(typeof(Enumerated));

                Assert.That(converter.CanConvertFrom(typeof(ValueMetadata)), Is.True);

                ValueMetadata metadata = new ValueMetadata("S1", "[S1, S2, S3]");

                object? result = converter.ConvertFrom(metadata);

                Assert.That(result, Is.TypeOf<Enumerated>());
                Enumerated enumerated = (Enumerated)result!;
                Assert.That(enumerated.Item, Is.EqualTo("S1"));
            }
        }
    }
}