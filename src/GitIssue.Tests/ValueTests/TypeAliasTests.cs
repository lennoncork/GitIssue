using System;
using System.Linq;
using GitIssue.Fields;
using GitIssue.Values;
using NUnit.Framework;
using ValueString = GitIssue.Values.String;

namespace GitIssue.Tests.ValueTests
{
    [TestFixture]
    public class TypeAliasTests
    {
        [TestFixture]
        public class AliasAttributeTests : TypeAliasTests
        {
            [Test]
            public void GetAliasAttribute_Returns_Attribute_For_Value_Type()
            {
                TypeAliasAttribute? attribute = TypeAlias.GetAliasAttribute(typeof(ValueString));

                Assert.That(attribute, Is.Not.Null);
                Assert.That(attribute!.Alias, Is.EqualTo("String"));
            }

            [Test]
            public void FromType_Creates_Alias_For_Value_Type()
            {
                TypeAlias? alias = TypeAlias.FromType(typeof(ValueString));

                Assert.That(alias, Is.Not.Null);
                Assert.That(alias!.Alias, Is.EqualTo("String"));
            }
        }

        [TestFixture]
        public class ClassificationTests : TypeAliasTests
        {
            [Test]
            public void IsFieldType_Recognizes_Field_Types()
            {
                Assert.That(TypeAlias.IsFieldType(typeof(Field)), Is.True);
                Assert.That(TypeAlias.IsFieldType(typeof(ValueString)), Is.False);
            }

            [Test]
            public void IsValueType_Recognizes_Value_Types()
            {
                Assert.That(TypeAlias.IsValueType(typeof(ValueString)), Is.True);
                Assert.That(TypeAlias.IsValueType(typeof(object)), Is.False);
            }
        }

        [TestFixture]
        public class AliasesCollectionTests : TypeAliasTests
        {
            [Test]
            public void Aliases_Contains_Entry_For_String_Value_Type()
            {
                bool hasStringAlias = TypeAlias.Aliases.Any(a =>
                {
                    return a.TryParse(typeof(ValueString), out string alias) && alias == "String";
                });

                Assert.That(hasStringAlias, Is.True, "Expected an alias entry for the String value type.");
            }
        }
    }
}
