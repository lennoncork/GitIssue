using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using GitIssue.Exceptions;
using GitIssue.Json;
using GitIssue.Keys;
using Newtonsoft.Json;

namespace GitIssue.Fields
{
    /// <summary>
    ///     FieldInfo Class
    /// </summary>
    [JsonObject]
    public class FieldInfo
    {
        private static IFieldReader[] readers;

        /// <summary>
        ///     Gets the list of field readers
        /// </summary>
        [JsonIgnore]
        public IFieldReader[] Readers
        {
            get
            {
                if (readers == null)
                    readers = GetType().Assembly.GetTypes()
                        .Where(t => t.IsPublic)
                        .Where(t => t.IsGenericType == false)
                        .Where(t => t.IsAbstract == false)
                        .Where(t => typeof(IFieldReader).IsAssignableFrom(t))
                        .Where(t => t.GetConstructor(Type.EmptyTypes) != null)
                        .Select(Activator.CreateInstance)
                        .Cast<IFieldReader>()
                        .ToArray();
                return readers;
            }
        }

        /// <summary>
        ///     Gets or sets if the field is required on a given issue
        /// </summary>
        [JsonProperty]
        [DefaultValue(false)]
        public bool Required { get; set; }

        /// <summary>
        ///     Gets or sets the data type for the field
        /// </summary>
        [JsonProperty]
        public virtual FieldType FieldType { get; set; } = FieldType.Create<JsonValueField>();

        /// <summary>
        ///     Gets or sets the data type for the field
        /// </summary>
        [JsonProperty]
        public virtual FieldType DataType { get; set; } = FieldType.Create<string>();

        /// <summary>
        ///     Determines if the field is valid
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public virtual bool IsValid(IField field)
        {
            return false;
        }

        /// <summary>
        ///     Creates a new issue field
        /// </summary>
        /// <param name="issue"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public IField CreateField(Issue issue, FieldKey key)
        {
            foreach (var provider in Readers)
                if (provider.CanCreateField(this))
                    return provider.CreateField(issue, key, this);
            throw new IssueNotFoundException("Unable to create issue with given field");
        }

        /// <summary>
        ///     Reads an existing issue field
        /// </summary>
        /// <param name="issue"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<IField> ReadFieldAsync(Issue issue, FieldKey key)
        {
            foreach (var provider in Readers)
                if (provider.CanReadField(this))
                    return await provider.ReadFieldAsync(issue, key, this);
            throw new IssueNotFoundException("Unable to read issue with given field");
        }
    }

    /// <summary>
    ///     FieldInfo Class
    /// </summary>
    [JsonObject]
    public class FieldInfo<T> : FieldInfo
    {
        /// <summary>
        ///     Gets or sets the field type
        /// </summary>
        [JsonProperty]
        public override FieldType DataType
        {
            get => FieldType.Create<T>();
            set => throw new ArgumentException("Cannot set data type for generic field info");
        }
    }
}