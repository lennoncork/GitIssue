using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using GitIssue.Fields;
using Newtonsoft.Json.Linq;
using FieldInfo = GitIssue.Fields.FieldInfo;

namespace GitIssue.Issues
{
    
    /// <summary>
    /// Provides an issue field for a single value persisted to disk
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FileValueField<T> : ValueField<T>, IJsonField
    {
        private readonly IssueRoot issueRoot;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileValueFieldReader"/> class
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
        /// Gets the file used to save the fields value
        /// </summary>
        public string FilePath => Path.Combine(this.issueRoot.IssuePath, this.Key.ToString());

        /// <inheritdoc/>
        public override async Task<bool> SaveAsync()
        {
            try
            {
                await using FileStream stream = new FileStream(this.FilePath, FileMode.Create, FileAccess.ReadWrite);
                await using StreamWriter writer = new StreamWriter(stream);
                await writer.WriteAsync(this.Value.ToString());
                return true;
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Failed to save field {this.Key} on issue {this.issueRoot.Key}", e);
            }
        }

        /// <summary>
        /// Asynchronously reads the field from disk
        /// </summary>
        /// <param name="issueRoot">the issue root</param>
        /// <param name="key">the field key</param>
        /// <param name="info">the field info</param>
        /// <returns>A task that represents the asynchronous read operation</returns>
        public static async Task<IField> ReadAsync(IssueRoot issueRoot, FieldKey key, FieldInfo info)
        {
            try
            {
                string fieldFile = Path.Combine(issueRoot.IssuePath, key.ToString());
                string content = await System.IO.File.ReadAllTextAsync(fieldFile);
                if (TryParse(content, out T result))
                {
                    return new FileValueField<T>(issueRoot, key, result);
                }
                throw new SerializationException($"Unable to convert field content to type {typeof(T)}");
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Failed to read field {key} on issue {issueRoot.Key}", e);
            }
        }

        /// <summary>
        /// Tries to parse the string input to the output value
        /// </summary>
        /// <param name="input">the input string</param>
        /// <param name="value">the output value</param>
        /// <returns></returns>
        internal static bool TryParse(string input, out T value)
        {
            TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
            if (converter.CanConvertFrom(typeof(string)))
            {
                value = (T)converter.ConvertFrom(input);
                return true;
            }
            value = default(T);
            return false;
        }

        /// <inheritdoc/>
        public JToken ToJson() => new JValue(this.Value);
    }
}