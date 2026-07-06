using System.Threading.Tasks;
using GitIssue.Fields;
using Moq;
using NUnit.Framework;

namespace GitIssue.Tests.FieldTests
{
    [TestFixture]
    public class EmptyFieldTests
    {
        [TestFixture]
        public class Basic : EmptyFieldTests
        {
            [Test]
            public void Copy_Always_Returns_True()
            {
                EmptyField field = new EmptyField(FieldKey.Create("empty"));
                IField other = new Mock<IField>(MockBehavior.Strict).Object;

                bool result = field.Copy(other);

                Assert.That(result, Is.True);
            }

            [Test]
            public void Equals_Returns_False_For_NonEmptyField()
            {
                EmptyField empty = new EmptyField(FieldKey.Create("empty"));
                IField other = new Mock<IField>(MockBehavior.Strict).Object;

                Assert.That(empty.Equals(other), Is.False);
            }

            [Test]
            public void Equals_Returns_True_For_Other_EmptyField()
            {
                FieldKey key = FieldKey.Create("empty");
                EmptyField first = new EmptyField(key);
                EmptyField second = new EmptyField(key);

                Assert.That(first.Equals(second), Is.True);
            }

            [Test]
            public async Task ExportAsync_Returns_Empty_String()
            {
                EmptyField field = new EmptyField(FieldKey.Create("empty"));

                string result = await field.ExportAsync();

                Assert.That(result, Is.EqualTo(string.Empty));
            }

            [Test]
            public async Task SaveAsync_Returns_False()
            {
                EmptyField field = new EmptyField(FieldKey.Create("empty"));

                bool result = await field.SaveAsync();

                Assert.That(result, Is.False);
            }

            [Test]
            public void Update_Returns_False_And_Does_Not_Change_Key()
            {
                FieldKey key = FieldKey.Create("empty");
                EmptyField field = new EmptyField(key);

                bool result = field.Update("anything");

                Assert.That(result, Is.False);
                Assert.That(field.Key, Is.EqualTo(key));
            }
        }
    }
}