using System.Threading.Tasks;
using GitIssue.Keys;

namespace GitIssue.Fields
{
    /// <summary>
    ///     Field Interface
    /// </summary>
    public interface IField
    {
        /// <summary>
        ///     Gets the field's key
        /// </summary>
        FieldKey Key { get; }

        /// <summary>
        /// Parses the input and updates the field if successful
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<bool> UpdateAsync(string input);

        /// <summary>
        ///     Saves any additional filed data
        /// </summary>
        /// <returns></returns>
        Task<bool> SaveAsync();
    }
}