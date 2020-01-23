namespace GitIssue.Fields
{
    /// <summary>
    /// Interface for field key provider
    /// </summary>
    public interface IFieldKeyProvider
    {
        /// <summary>
        /// Creates a new key from the specified string
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        FieldKey FromString(string key);
    }
}
