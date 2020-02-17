using System;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using GitIssue.Fields;
using GitIssue.Fields.Value;
using GitIssue.Issues.Json;
using Newtonsoft.Json.Linq;

namespace GitIssue.Issues.File
{
    /// <summary>
    ///     Provides an issue field for a single value persisted to disk
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FileValueField<T> : ValueField<T>, IJsonField
    {
        private readonly IssueRoot issueRoot;

        /// <summary>
        ///     Initializes a new instance of the <see cref="FileValueField{T}" /> class
        /// </summary>
        /// <param name="root">the issue root</param>
        /// <param name="key">the field key</param>
        public FileValueField(IssueRoot root, FieldKey key)
            : base(key, default!)
        {
            this.issueRoot = root;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="FileValueField{T}" /> class
        /// </summary>
        /// <param name="root">the issue root</param>
        /// <param name="key">the field key</param>
        /// <param name="value">the field value</param>
        public FileValueField(IssueRoot root, FieldKey key, T value)
            : base(key, value)
        {
            this.issueRoot = root;
        }

        /// <summary>
        ///     Gets the file used to save the fields value
        /// </summary>
        public string FilePath => Path.Combine(issueRoot.IssuePath, Key);

        /// <inheritdoc />
        public override async Task<bool> SaveAsync()
        {
            try
            {
                await using var stream = new FileStream(FilePath, FileMode.Create, FileAccess.ReadWrite);
                await using var writer = new StreamWriter(stream);
                await writer.WriteAsync(Value?.ToString());
                return true;
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Failed to save field {Key} on issue {issueRoot.Key}", e);
            }
        }

        /// <inheritdoc />
        public override Task<string> ExportAsync()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public JToken ToJson()
        {
            return new JValue(Value);
        }

        /// <summary>
        ///     Asynchronously reads the field from disk
        /// </summary>
        /// <param name="issueRoot">the issue root</param>
        /// <param name="key">the field key</param>
        /// <param name="info">the field info</param>
        /// <returns>A task that represents the asynchronous read operation</returns>
        public static async Task<IField> ReadAsync(IssueRoot issueRoot, FieldKey key, FieldInfo info)
        {
            try
            {
                var fieldFile = Path.Combine(issueRoot.IssuePath, key.ToString());
                var content = await System.IO.File.ReadAllTextAsync(fieldFile);
                var field = new FileValueField<T>(issueRoot, key);
                if (field.TryParse(content, out T value))
                {
                    field.Value = value;
                }
                throw new SerializationException($"Unable to convert field content to type {typeof(T)}");
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Failed to read field {key} on issue {issueRoot.Key}", e);
            }
        }
    }
}