using System.Threading.Tasks;
using GitIssue.Fields;
using GitIssue.Fields.Value;
using GitIssue.Values;
using Newtonsoft.Json.Linq;

namespace GitIssue.Issues.Json
{
    /// <summary>
    ///     Generic alias class for JsonValue
    /// </summary>
    public class JsonValueField : TypeAlias
    {
        /// <summary>
        ///     Creates a new instance of the JsonValueField
        /// </summary>
        public JsonValueField() : base("JsonValue")
        {
        }
    }

    /// <summary>
    ///     Issue field that serialized to json
    /// </summary>
    /// <typeparam name="T">the fields value type</typeparam>
    public class JsonValueField<T> : ValueField<T>, IJsonField
    {
        /// <summary>
        ///     Initialized a new instance of the <see cref="JsonValueField{T}" /> class
        /// </summary>
        /// <param name="key">the field key</param>
        public JsonValueField(FieldKey key) : base(key, default!)
        {
        }

        /// <summary>
        ///     Initialized a new instance of the <see cref="JsonValueField{T}" /> class
        /// </summary>
        /// <param name="key">the field key</param>
        /// <param name="value">the field value</param>
        public JsonValueField(FieldKey key, T value) : base(key, value)
        {
        }

        /// <inheritdoc />
        public override Task<bool> SaveAsync()
        {
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public override Task<string> ExportAsync()
        {
            return Task.FromResult(ToString());
        }

        /// <inheritdoc />
        public JToken ToJson()
        {
            if (Value is IJsonValue value)
                return value.ToJson();
            return new JValue(Value);
        }
    }
}