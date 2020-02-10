using GitIssue.Keys;

namespace GitIssue.Issues
{
    /// <summary>
    ///     Field key provider for files
    /// </summary>
    public class FileFieldKeyProvider : FieldKeyProvider
    {
        /// <summary>
        ///     Creates a new field key
        /// </summary>
        /// <param name="key">the key string</param>
        /// <returns></returns>
        public override FieldKey FromString(string key)
        {
            return FieldKey.Create(key);
        }
    }
}