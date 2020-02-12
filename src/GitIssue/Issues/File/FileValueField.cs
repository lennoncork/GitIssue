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
        /// <param name="issueRoot"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public FileValueField(IssueRoot issueRoot, FieldKey key, T value)
            : base(key, value)
        {
            this.issueRoot = issueRoot;
        }

        /// <summary>
        ///     Gets the file used to save the fields value
        /// </summary>
        public string FilePath => Path.Combine(issueRoot.IssuePath, Key.ToString());

        /// <inheritdoc />
        public override async Task<bool> SaveAsync()
        {
            try
            {
                await using var stream = new FileStream(FilePath, FileMode.Create, FileAccess.ReadWrite);
                await using var writer = new StreamWriter(stream);
                await writer.WriteAsync(Value.ToString());
                return true;
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Failed to save field {Key} on issue {issueRoot.Key}", e);
            }
        }

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
                if (TryParse(content, out var result)) return new FileValueField<T>(issueRoot, key, result);
                throw new SerializationException($"Unable to convert field content to type {typeof(T)}");
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Failed to read field {key} on issue {issueRoot.Key}", e);
            }
        }
    }
}