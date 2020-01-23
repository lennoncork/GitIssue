using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using GitIssue.Fields;
using Newtonsoft.Json.Linq;

namespace GitIssue.Json
{
    /// <summary>
    /// 
    /// </summary>
    public class JsonValueField { }

    /// <summary>
    /// Issue field that serialized to json
    /// </summary>
    /// <typeparam name="T">the fields value type</typeparam>
    public class JsonValueField<T> : ValueField<T>, IJsonField
    {
        /// <summary>
        /// Initialized a new instance of the <see cref="JsonValueField{T}"/> class
        /// </summary>
        /// <param name="key">the field key</param>
        public JsonValueField(FieldKey key) : base(key, default(T))
        {
        }

        /// <summary>
        /// Initialized a new instance of the <see cref="JsonValueField{T}"/> class
        /// </summary>
        /// <param name="key">the field key</param>
        /// <param name="value">the field value</param>
        public JsonValueField(FieldKey key, T value) : base(key, value)
        {
        }

        /// <inheritdoc/>
        public override Task<bool> SaveAsync() => Task.FromResult(true);

        /// <inheritdoc/>
        public JToken ToJson() => new JValue(this.Value);
    }
}
