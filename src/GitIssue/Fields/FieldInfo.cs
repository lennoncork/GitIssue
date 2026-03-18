using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using GitIssue.Exceptions;
using GitIssue.Issues;
using GitIssue.Issues.Json;
using GitIssue.Values;
using Newtonsoft.Json;

namespace GitIssue.Fields
{
    /// <summary>
    ///     FieldInfo Class
    /// </summary>
    [JsonObject]
    public class FieldInfo
    {
        private static IFieldReader[]? readers;

        /// <summary>
        ///     Initializes a new instance of the <see cref="FieldInfo" /> class
        /// </summary>
        public FieldInfo() : this(false)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="FieldInfo" /> class
        /// </summary>
        /// <param name="required"></param>
        public FieldInfo(bool required)
        {
            this.Required = required;
        }

        /// <summary>
        ///     Gets or sets the metadata for the field
        /// </summary>
        [JsonProperty]
        [DefaultValue("")]
        public virtual string FieldMetadata { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the data type for the field
        /// </summary>
        [JsonProperty]
        public virtual TypeValue FieldType { get; set; } = TypeValue.Create<JsonValueField>();

        /// <summary>
        ///     Gets the list of field readers
        /// </summary>
        [JsonIgnore]
        public IFieldReader[] Readers =>
            FieldInfo.readers ??= this.GetType().Assembly.GetTypes()
                .Where(t => t.IsPublic)
                .Where(t => !t.IsGenericType)
                .Where(t => !t.IsAbstract)
                .Where(t => typeof(IFieldReader).IsAssignableFrom(t))
                .Where(t => t.GetConstructor(Type.EmptyTypes) != null)
                .Select(Activator.CreateInstance)
                .Cast<IFieldReader>()
                .ToArray();

        /// <summary>
        ///     Gets or sets if the field is required on a given issue
        /// </summary>
        [JsonProperty]
        [DefaultValue(false)]
        public bool Required { get; set; }

        /// <summary>
        ///     Gets or sets the metadata for the values
        /// </summary>
        [JsonProperty]
        [DefaultValue("")]
        public virtual string ValueMetadata { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the value type for the field
        /// </summary>
        [JsonProperty]
        public virtual TypeValue ValueType { get; set; } = TypeValue.Create<string>();

        /// <summary>
        ///     Creates a new issue field
        /// </summary>
        /// <param name="issue"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public IField CreateField(Issue issue, FieldKey key)
        {
            foreach (IFieldReader provider in this.Readers)
            {
                if (provider.CanCreateField(this))
                {
                    return provider.CreateField(issue, key, this);
                }
            }

            throw new IssueNotFoundException("Unable to create issue with given field");
        }

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
        ///     Reads an existing issue field
        /// </summary>
        /// <param name="issue"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<IField> ReadFieldAsync(Issue issue, FieldKey key)
        {
            foreach (IFieldReader provider in this.Readers)
            {
                if (provider.CanReadField(this))
                {
                    return await provider.ReadFieldAsync(issue, key, this);
                }
            }

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
        ///     Initializes a new instance of the <see cref="FieldInfo" /> class
        /// </summary>
        public FieldInfo() : this(false)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="FieldInfo" /> class
        /// </summary>
        /// <param name="required"></param>
        public FieldInfo(bool required) : base(required)
        {
            this.Required = required;
        }

        /// <summary>
        ///     Gets or sets the data type
        /// </summary>
        [JsonProperty]
        public override TypeValue ValueType
        {
            get => TypeValue.Create<T>();
            set => throw new ArgumentException("Cannot set data type for generic data info");
        }
    }

    /// <summary>
    ///     FieldInfo Class
    /// </summary>
    [JsonObject]
    public class FieldInfo<T1, T2> : FieldInfo<T1>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="FieldInfo" /> class
        /// </summary>
        public FieldInfo() : this(false)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="FieldInfo" /> class
        /// </summary>
        /// <param name="required"></param>
        public FieldInfo(bool required) : base(required)
        {
            this.Required = required;
        }

        /// <summary>
        ///     Gets or sets the field type
        /// </summary>
        [JsonProperty]
        public override TypeValue FieldType
        {
            get => TypeValue.Create<T2>();
            set => throw new ArgumentException("Cannot set data type for generic field info");
        }
    }
}