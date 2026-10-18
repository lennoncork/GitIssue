using System.Text.Json;
using System.Text.Json.Nodes;
using GitIssue.Fields;
using GitIssue.Issues;
using GitIssue.Values;
using NUnit.Framework;
using StringValue = GitIssue.Values.String;

namespace GitIssue.Tests
{
    [TestFixture]
    public class JsonSerializationTests
    {
        [Test]
        public void ToNode_UsesJsonValueImplementationsAndSerializerFallback()
        {
            JsonNode? customNode = JsonSerialization.ToNode(StringValue.Parse("custom"));
            JsonNode? numericNode = JsonSerialization.ToNode(42);

            Assert.Multiple(() =>
            {
                Assert.That(customNode!.GetValue<string>(), Is.EqualTo("custom"));
                Assert.That(numericNode!.GetValue<int>(), Is.EqualTo(42));
            });
        }

        [Test]
        public void TryGetScalar_ReturnsSupportedScalarValues()
        {
            Assert.Multiple(() =>
            {
                Assert.That(JsonSerialization.TryGetScalar(JsonValue.Create("value")!, out object? text), Is.True);
                Assert.That(text, Is.EqualTo("value"));
                Assert.That(JsonSerialization.TryGetScalar(JsonValue.Create(42L)!, out object? integer), Is.True);
                Assert.That(integer, Is.EqualTo(42L));
                Assert.That(JsonSerialization.TryGetScalar(JsonValue.Create(1.5m)!, out object? decimalValue), Is.True);
                Assert.That(decimalValue, Is.EqualTo(1.5m));
                Assert.That(JsonSerialization.TryGetScalar(JsonValue.Create(2.5d)!, out object? doubleValue), Is.True);
                Assert.That(doubleValue, Is.EqualTo(2.5d));
                Assert.That(JsonSerialization.TryGetScalar(JsonValue.Create(true)!, out object? boolean), Is.True);
                Assert.That(boolean, Is.EqualTo(true));
            });
        }

        [Test]
        public void Converters_RoundTripKeysAndTypeValues()
        {
            IssueKey issueKey = IssueKey.Create("ISS-1");
            FieldKey fieldKey = FieldKey.Create("Title");
            TypeValue typeValue = TypeValue.Create<StringValue>();

            IssueKey issueResult = JsonSerializer.Deserialize<IssueKey>(JsonSerializer.Serialize(issueKey, JsonSerialization.Options), JsonSerialization.Options);
            FieldKey fieldResult = JsonSerializer.Deserialize<FieldKey>(JsonSerializer.Serialize(fieldKey, JsonSerialization.Options), JsonSerialization.Options);
            TypeValue typeResult = JsonSerializer.Deserialize<TypeValue>(JsonSerializer.Serialize(typeValue, JsonSerialization.Options), JsonSerialization.Options);

            Assert.Multiple(() =>
            {
                Assert.That(issueResult, Is.EqualTo(issueKey));
                Assert.That(fieldResult, Is.EqualTo(fieldKey));
                Assert.That(typeResult.Type, Is.EqualTo(typeof(StringValue)));
            });
        }

        [Test]
        public void TypeValueConverter_RejectsUnknownType()
        {
            Assert.That(
                () => JsonSerializer.Deserialize<TypeValue>("\"Unknown.Type\"", JsonSerialization.Options),
                Throws.TypeOf<JsonException>());
        }
    }
}