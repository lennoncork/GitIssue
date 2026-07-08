using System.ComponentModel;
using System.Threading.Tasks;
using GitIssue.Fields;
using GitIssue.Values;
using NUnit.Framework;

namespace GitIssue.Tests.ValueTests
{
    [TestFixture]
    public class TypeValueTests
    {
        [TestFixture]
        public class TypeConverterFixture : TypeValueTests
        {
            [Test]
            public void CanConvertFromString()
            {
                TypeConverter converter = TypeDescriptor.GetConverter(typeof(TypeValue));

                Assert.That(converter.CanConvertFrom(typeof(string)), Is.True);
            }

            [Test]
            public void ConvertFromString_Parses_TypeValue()
            {
                TypeConverter converter = TypeDescriptor.GetConverter(typeof(TypeValue));

                object? result = converter.ConvertFrom(typeof(Field).FullName!);

                Assert.That(result, Is.InstanceOf<TypeValue>());
                TypeValue tv = (TypeValue)result!;
                Assert.That(tv.Type, Is.EqualTo(typeof(Field)));
            }

            [Test]
            public void ConvertToString_Uses_TypeValue_ToString()
            {
                TypeConverter converter = TypeDescriptor.GetConverter(typeof(TypeValue));
                TypeValue tv = TypeValue.Create<Field>();

                object? result = converter.ConvertTo(tv, typeof(string));

                Assert.That(result, Is.EqualTo(tv.ToString()));
            }
        }

        [TestFixture]
        public class TryParseAndCreate : TypeValueTests
        {
            [Test]
            public void Create_From_Type_And_Generic()
            {
                TypeValue fromType = TypeValue.Create(typeof(Field));
                TypeValue fromGeneric = TypeValue.Create<Field>();

                Assert.That(fromType.Type, Is.EqualTo(typeof(Field)));
                Assert.That(fromGeneric.Type, Is.EqualTo(typeof(Field)));
            }

            [Test]
            public void TryParse_Parses_TypeName()
            {
                bool success = TypeValue.TryParse(typeof(Field).FullName!, out TypeValue tv);

                Assert.That(success, Is.True);
                Assert.That(tv.Type, Is.EqualTo(typeof(Field)));
            }

            [Test]
            public void TryParse_With_Unknown_Type_Sets_Default_TypeValue()
            {
                bool success = TypeValue.TryParse("Not.A.Type", out TypeValue tv);

                // Implementation returns true but tv.Type will be default (null) when type cannot be resolved
                Assert.That(success, Is.True);
                Assert.That(tv.Type, Is.EqualTo(default(TypeValue).Type));
            }
        }

        [TestFixture]
        public class TryCreate : TypeValueTests
        {
            private class TestField : Field
            {
                public TestField(FieldKey key) : base(key)
                {
                }

                public override bool Copy(IField? other)
                {
                    return false;
                }

                public override bool Equals(IField? other)
                {
                    return object.ReferenceEquals(this, other);
                }

                public override Task<string> ExportAsync()
                {
                    return Task.FromResult(string.Empty);
                }

                public override Task<bool> SaveAsync()
                {
                    return Task.FromResult(true);
                }

                public override bool Update(string input)
                {
                    return false;
                }
            }

            [Test]
            public void TryCreate_Fails_For_Incompatible_Type()
            {
                TypeValue tv = TypeValue.Create<Field>();
                bool success = tv.TryCreate(out TestField result, FieldKey.Create("test"));

                Assert.That(success, Is.False);
                Assert.That(result, Is.Null);
            }

            [Test]
            public void TryCreate_Succeeds_For_Compatible_Type()
            {
                TypeValue tv = TypeValue.Create<TestField>();
                bool success = tv.TryCreate(out TestField result, FieldKey.Create("test"));

                Assert.That(success, Is.True);
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Key, Is.EqualTo(FieldKey.Create("test")));
            }
        }
    }
}