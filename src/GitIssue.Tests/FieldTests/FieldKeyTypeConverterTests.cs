using System.ComponentModel;
using System.Globalization;
using GitIssue.Fields;
using NUnit.Framework;

namespace GitIssue.Tests.FieldTests
{
    [TestFixture]
    public class FieldKeyTypeConverterTests
    {
        [Test]
        public void ConvertsFieldKeyToAndFromString()
        {
            TypeConverter converter = new FieldKeyTypeConverter();

            FieldKey key = (FieldKey)converter.ConvertFrom(null, CultureInfo.InvariantCulture, "Title")!;
            string value = (string)converter.ConvertTo(null, CultureInfo.InvariantCulture, key, typeof(string))!;

            Assert.Multiple(() =>
            {
                Assert.That(converter.CanConvertFrom(typeof(string)), Is.True);
                Assert.That(key.ToString(), Is.EqualTo("Title"));
                Assert.That(value, Is.EqualTo("Title"));
            });
        }
    }
}