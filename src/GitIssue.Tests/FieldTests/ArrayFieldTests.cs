using System.Collections;
using System.Threading.Tasks;
using GitIssue.Fields;
using GitIssue.Fields.Array;
using NUnit.Framework;

namespace GitIssue.Tests.FieldTests
{
    [TestFixture]
    public class ArrayFieldTests
    {
        private class TestArrayField : ArrayField<string>
        {
            public TestArrayField(FieldKey key, params string[] values)
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

        [TestFixture]
        public class Basic : ArrayFieldTests
        {
            [Test]
            public void Copy_Copies_Values_From_Other()
            {
                FieldKey key = FieldKey.Create("labels");
                TestArrayField source = new TestArrayField(key, "one", "two");
                TestArrayField target = new TestArrayField(key, "initial");

                target.Copy(source);

                Assert.That(target.Values, Is.EqualTo(new[] { "one", "two" }));
            }

            [Test]
            public void Count_Indexer_Values_And_ValueType_Work()
            {
                TestArrayField field = new TestArrayField(FieldKey.Create("labels"), "one", "two");

                Assert.That(field.Count, Is.EqualTo(2));
                Assert.That(field[0], Is.EqualTo("one"));

                field[1] = "two-modified";

                Assert.That(field.Values, Is.EqualTo(new[] { "one", "two-modified" }));
                Assert.That(field.ValueType, Is.EqualTo(typeof(string)));
            }

            [Test]
            public void Equals_Returns_False_When_Sequences_Differ()
            {
                FieldKey key = FieldKey.Create("labels");
                TestArrayField first = new TestArrayField(key, "one", "two");
                TestArrayField second = new TestArrayField(key, "one");

                Assert.That(first.Equals(second), Is.False);
            }

            [Test]
            public void Equals_Returns_True_When_Sequences_Equal()
            {
                FieldKey key = FieldKey.Create("labels");
                TestArrayField first = new TestArrayField(key, "one", "two");
                TestArrayField second = new TestArrayField(key, "one", "two");

                Assert.That(first.Equals(second), Is.True);
            }

            [Test]
            public void ToString_Formats_As_Array()
            {
                TestArrayField field = new TestArrayField(FieldKey.Create("labels"), "one", "two");

                Assert.That(field.ToString(), Is.EqualTo("[one, two]"));
            }
        }

        [TestFixture]
        public class Update : ArrayFieldTests
        {
            [Test]
            public void Update_With_Invalid_Format_Returns_False_And_Does_Not_Change()
            {
                TestArrayField field = new TestArrayField(FieldKey.Create("labels"), "one");

                bool result = field.Update("invalid");

                Assert.That(result, Is.False);
                Assert.That(field.Values, Is.EqualTo(new[] { "one" }));
            }

            [Test]
            public void Update_With_List_Replaces_Values()
            {
                TestArrayField field = new TestArrayField(FieldKey.Create("labels"), "old");

                bool result = field.Update("[one, two]");

                Assert.That(result, Is.True);
                Assert.That(field.Values, Is.EqualTo(new[] { "one", "two" }));
            }

            [Test]
            public void Update_With_Minus_Removes_Value_When_Present()
            {
                TestArrayField field = new TestArrayField(FieldKey.Create("labels"), "one", "two");

                bool result = field.Update("-two");

                Assert.That(result, Is.True);
                Assert.That(field.Values, Is.EqualTo(new[] { "one" }));
            }

            [Test]
            public void Update_With_Plus_Adds_Value_When_Not_Present()
            {
                TestArrayField field = new TestArrayField(FieldKey.Create("labels"), "one");

                bool result = field.Update("+two");

                Assert.That(result, Is.True);
                Assert.That(field.Values, Is.EqualTo(new[] { "one", "two" }));
            }
        }

        [TestFixture]
        public class Interfaces : ArrayFieldTests
        {
            [Test]
            public void IArrayField_TryParse_Parses_String_Input()
            {
                TestArrayField field = new TestArrayField(FieldKey.Create("labels"), "initial");
                IArrayField arrayField = field;

                bool success = arrayField.TryParse("value", out object? value);

                Assert.That(success, Is.True);
                Assert.That(value, Is.EqualTo("value"));
            }

            [Test]
            public void IArrayField_Values_Setter_Filters_To_T_Type()
            {
                TestArrayField field = new TestArrayField(FieldKey.Create("labels"), "initial");
                IArrayField arrayField = field;

                arrayField.Values = new object[] { "one", 2, "three" };

                Assert.That(field.Values, Is.EqualTo(new[] { "one", "three" }));
            }

            [Test]
            public void IList_Members_Work()
            {
                TestArrayField field = new TestArrayField(FieldKey.Create("labels"), "one");
                IList list = field;

                int index = list.Add("two");
                Assert.That(index, Is.EqualTo(1));
                Assert.That(list.Contains("two"), Is.True);

                int idx = list.IndexOf("two");
                Assert.That(idx, Is.EqualTo(1));

                list.Insert(1, "inserted");
                Assert.That(field.Values, Is.EqualTo(new[] { "one", "inserted", "two" }));

                list.Remove("inserted");
                Assert.That(field.Values, Is.EqualTo(new[] { "one", "two" }));

                list.RemoveAt(1);
                Assert.That(field.Values, Is.EqualTo(new[] { "one" }));

                list.Clear();
                Assert.That(field.Values, Is.Empty);
            }
        }
    }
}