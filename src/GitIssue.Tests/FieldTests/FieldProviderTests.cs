using System.Threading.Tasks;
using GitIssue.Fields;
using GitIssue.Fields.Array;
using GitIssue.Fields.Value;
using NUnit.Framework;
using StringValue = GitIssue.Values.String;

namespace GitIssue.Tests.FieldTests
{
    [TestFixture]
    public class FieldProviderTests
    {
        private class StringArrayField : ArrayField<StringValue>
        {
            public StringArrayField(FieldKey key, params StringValue[] values)
                : base(key, values)
            {
            }

            public override Task<string> ExportAsync()
            {
                return Task.FromResult(string.Empty);
            }

            public override Task<bool> SaveAsync()
            {
                return Task.FromResult(true);
            }
        }

        private class StringValueField : ValueField<StringValue>
        {
            public StringValueField(FieldKey key, StringValue value)
                : base(key, value)
            {
            }

            public override Task<string> ExportAsync()
            {
                return Task.FromResult(string.Empty);
            }

            public override Task<bool> SaveAsync()
            {
                return Task.FromResult(true);
            }
        }

        [TestFixture]
        public class AsArray : FieldProviderTests
        {
            [Test]
            public void AsArray_Returns_Null_When_Field_Is_Not_ArrayField()
            {
                FieldKey key = FieldKey.Create("field");
                StringValueField valueField = new StringValueField(key, StringValue.Parse("one"));

                FieldProvider provider = new FieldProvider(null!, key, () => valueField);

                StringValue[] result = provider.AsArray<StringValue>();

                Assert.That(result, Is.Null);
            }

            [Test]
            public void AsArray_Returns_Values_From_ArrayField()
            {
                FieldKey key = FieldKey.Create("field");
                StringArrayField arrayField = new StringArrayField(key, StringValue.Parse("one"), StringValue.Parse("two"));

                FieldProvider provider = new FieldProvider(null!, key, () => arrayField);

                StringValue[] result = provider.AsArray<StringValue>();

                Assert.That(result, Is.Not.Null);
                Assert.That(result, Is.EqualTo(new[] { StringValue.Parse("one"), StringValue.Parse("two") }));
            }
        }

        [TestFixture]
        public class AsValue : FieldProviderTests
        {
            [Test]
            public void AsValue_Returns_Default_When_Field_Is_Not_ValueField()
            {
                FieldKey key = FieldKey.Create("field");
                StringArrayField arrayField = new StringArrayField(key, StringValue.Parse("one"));

                FieldProvider provider = new FieldProvider(null!, key, () => arrayField);

                StringValue result = provider.AsValue<StringValue>();

                Assert.That(result, Is.EqualTo(default(StringValue)));
            }

            [Test]
            public void AsValue_Returns_Value_From_ValueField()
            {
                FieldKey key = FieldKey.Create("field");
                StringValueField valueField = new StringValueField(key, StringValue.Parse("one"));

                FieldProvider provider = new FieldProvider(null!, key, () => valueField);

                StringValue result = provider.AsValue<StringValue>();

                Assert.That(result, Is.EqualTo(StringValue.Parse("one")));
            }
        }
    }
}