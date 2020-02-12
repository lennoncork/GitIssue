using System.Threading.Tasks;
using GitIssue.Issues;

namespace GitIssue.Fields
{
    /// <summary>
    ///     Interface defining
    /// </summary>
    public interface IFieldReader
    {
        /// <summary>
        ///     Gets a value indicating if this <see cref="IFieldReader" /> can create a new field
        /// </summary>
        /// <param name="info">the field info</param>
        /// <returns>true if can create</returns>
        public bool CanCreateField(FieldInfo info);

        /// <summary>
        ///     Gets a value indicating if this <see cref="IFieldReader" /> can read an existing field
        /// </summary>
        /// <param name="info">the field info</param>
        /// <returns>true if can read</returns>
        public bool CanReadField(FieldInfo info);

        /// <summary>
        ///     Creates a new field
        /// </summary>
        /// <param name="issue">the issue</param>
        /// <param name="key">the field key</param>
        /// <param name="info">the field info</param>
        /// <returns></returns>
        public IField CreateField(Issue issue, FieldKey key, FieldInfo info);

        /// <summary>
        ///     Creates a new field with specified data type
        /// </summary>
        /// <typeparam name="T">the data type</typeparam>
        /// <param name="issue">the issue</param>
        /// <param name="key">the field key</param>
        /// <param name="info">the field info</param>
        /// <returns></returns>
        public IField CreateField<T>(Issue issue, FieldKey key, FieldInfo info);

        /// <summary>
        ///     Reads an existing field
        /// </summary>
        /// <param name="issue">the issue</param>
        /// <param name="key">the field key</param>
        /// <param name="info">the field info</param>
        /// <returns></returns>
        public Task<IField> ReadFieldAsync(Issue issue, FieldKey key, FieldInfo info);

        /// <summary>
        ///     Reads an existing field with specified data type
        /// </summary>
        /// <typeparam name="T">the data type</typeparam>
        /// <param name="issue">the issue</param>
        /// <param name="key">the field key</param>
        /// <param name="info">the field info</param>
        /// <returns></returns>
        public Task<IField> ReadFieldAsync<T>(Issue issue, FieldKey key, FieldInfo info);
    }
}