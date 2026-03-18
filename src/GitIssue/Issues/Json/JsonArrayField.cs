using System.Threading.Tasks;
using GitIssue.Fields;
using GitIssue.Fields.Array;
using GitIssue.Values;
using Newtonsoft.Json.Linq;

namespace GitIssue.Issues.Json
{
    /// <summary>
    ///     Generic alias class for JsonArray
    /// </summary>
    public class JsonArrayField : TypeAlias
    {
        /// <summary>
        ///     Creates a new instance of the JsonArrayField
        /// </summary>
        public JsonArrayField() : base("JsonArray")
        {
        }
    }

    /// <summary>
    ///     Issue field that serialized to json
    /// </summary>
    /// <typeparam name="T">the fields value type</typeparam>
    public class JsonArrayField<T> : ArrayField<T>, IJsonField
    {
        /// <summary>
        ///     Initialized a new instance of the <see cref="JsonArrayField{T}" /> class
        /// </summary>
        /// <param name="key">the field key</param>
        /// <param name="values">the array of field values</param>
        public JsonArrayField(FieldKey key, T[] values) : base(key, values)
        {
        }

        /// <inheritdoc />
        public override Task<bool> SaveAsync()
        {
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public JToken ToJson()
        {
            JArray array = new JArray();
            foreach (T value in this.Values)
            {
                if (value is IJsonValue jValue)
                {
                    array.Add(jValue.ToJson());
                    continue;
                }

                array.Add(new JValue(value));
            }

            return array;
        }

        /// <inheritdoc />
        public override Task<string> ExportAsync()
        {
            return Task.FromResult(this.ToString());
        }
    }
}