namespace GitIssue.Fields
{
    /// <summary>
    /// Provides an interface for a provider that extracts values from fields
    /// </summary>
    public interface IFieldProvider
    {
        /// <summary>
        /// Extracts the value from the field
        /// </summary>
        /// <typeparam name="T">the value data type</typeparam>
        /// <returns>the field value, or default</returns>
        T AsValue<T>();

        /// <summary>
        /// Extracts the value array from the field
        /// </summary>
        /// <typeparam name="T">the value data type</typeparam>
        /// <returns>the array of field values, or null</returns>
        T[] AsArray<T>();
    }

}
