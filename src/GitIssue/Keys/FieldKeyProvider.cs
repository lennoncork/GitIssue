namespace GitIssue.Keys
{
    /// <summary>
    ///     Base class for FieldKeyProviders
    /// </summary>
    public abstract class FieldKeyProvider : IFieldKeyProvider
    {
        /// <summary>
        ///     Generates a new FieldKey from a string
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public abstract FieldKey FromString(string key);
    }
}