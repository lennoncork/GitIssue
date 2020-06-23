using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace GitIssue.Fields
{
    /// <summary>
    ///     Field Interface
    /// </summary>
    public interface IField : IReadOnlyField
    {
        /// <summary>
        /// Copy's the field 
        /// </summary>
        /// <param name="other"></param>
        bool Copy([AllowNull] IField other);

        /// <summary>
        ///     Parses the input and updates the field if successful
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        bool Update(string input);

        /// <summary>
        ///     Saves any additional filed data
        /// </summary>
        /// <returns></returns>
        Task<bool> SaveAsync();
    }
}