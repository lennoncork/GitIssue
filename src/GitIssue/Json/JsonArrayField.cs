using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GitIssue.Fields;
using GitIssue.Keys;
using Newtonsoft.Json.Linq;

namespace GitIssue.Json
{
    public class JsonArrayField
    {

    }

    /// <summary>
    /// Issue field that serialized to json
    /// </summary>
    /// <typeparam name="T">the fields value type</typeparam>
    public class JsonArrayField<T> : ArrayField<T>, IJsonField
    {
        /// <summary>
        /// Initialized a new instance of the <see cref="JsonArrayField{T}"/> class
        /// </summary>
        /// <param name="key">the field key</param>
        /// <param name="values">the array of field values</param>
        public JsonArrayField(FieldKey key, T[] values) : base(key, values)
        {
        }

        /// <inheritdoc/>
        public override Task<bool> SaveAsync() => Task.FromResult(true);

        /// <inheritdoc/>
        public JToken ToJson() => new JArray(this.Values);
    }
}
