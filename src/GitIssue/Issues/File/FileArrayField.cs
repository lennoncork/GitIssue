using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using GitIssue.Fields;
using GitIssue.Fields.Array;
using GitIssue.Issues.Json;
using Newtonsoft.Json.Linq;
using FieldInfo = GitIssue.Fields.FieldInfo;

namespace GitIssue.Issues.File
{
    /// <summary>
    ///     Provides an issue field for an array of values persisted to disk
    /// </summary>
    public abstract class FileArrayField
    {
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
                var generic = typeof(FileArrayField<>);
                var specific = generic.MakeGenericType((Type) info.ValueType);
                var read = specific.GetMethod(nameof(ReadAsync), BindingFlags.Public | BindingFlags.Static);
                if (read != null)
                {
                    object[] args = {issueRoot, key, info};
                    var task = (Task)read.Invoke(null, args)!;
                    await task;
                    var result = (IField)task.GetType().GetProperty("Result")?.GetValue(task)!;
                    return result;
                }
                return null!;
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Failed to read field {key} on issue {issueRoot.Key}", e);
            }
        }
    }

    /// <summary>
    ///     Provides an issue field for an array of values persisted to disk
    /// </summary>
    public class FileArrayField<T> : ArrayField<T>, IJsonField
    {
        private readonly IssueRoot issueRoot;

        /// <summary>
        ///     Initializes a new instance of the <see cref="FileArrayField{T}" /> class
        /// </summary>
        /// <param name="issueRoot"></param>
        /// <param name="key"></param>
        /// <param name="values"></param>
        public FileArrayField(IssueRoot issueRoot, FieldKey key, T[] values)
            : base(key, values)
        {
            this.issueRoot = issueRoot;
        }

        /// <summary>
        ///     Gets the directory used to save the fields values
        /// </summary>
        public string DirectoryPath => Path.Combine(issueRoot.IssuePath, Key.ToString());

        /// <inheritdoc />
        public override async Task<bool> SaveAsync()
        {
            try
            {
                if (Directory.Exists(DirectoryPath) == false)
                    Directory.CreateDirectory(DirectoryPath);

                var count = 0;
                foreach (var value in Values)
                {
                    var file = Path.Combine(DirectoryPath, count.ToString());
                    await using var stream = new FileStream(file, FileMode.Create, FileAccess.ReadWrite);
                    await using var writer = new StreamWriter(stream);
                    await writer.WriteAsync(value?.ToString());
                }

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
            return new JArray(Values);
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
                var directory = Path.Combine(issueRoot.IssuePath, key.ToString());

                if (Directory.Exists(directory) == false)
                    return new FileArrayField<T>(issueRoot, key, new T[] { });

                var values = new List<T>();
                foreach (var fieldFile in Directory.EnumerateFiles(directory))
                {
                    var content = await System.IO.File.ReadAllTextAsync(fieldFile);
                    if (TryParse(content, out var result))
                    {
                        values.Add(result);
                        continue;
                    }

                    throw new SerializationException($"Unable to convert field content to type {typeof(T)}");
                }

                return new FileArrayField<T>(issueRoot, key, values.ToArray());
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Failed to read field {key} on issue {issueRoot.Key}", e);
            }
        }
    }
}