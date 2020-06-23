using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace GitIssue.Fields
{
    /// <summary>
    ///     Abstract field class
    /// </summary>
    public abstract class Field : IField
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="Field" /> class
        /// </summary>
        /// <param name="key">the field key</param>
        protected Field(FieldKey key)
        {
            Key = key;
        }

        /// <summary>
        ///     Gets the key for the field
        /// </summary>
        public FieldKey Key { get; protected set; }

        /// <inheritdoc />
        public abstract bool Update(string input);

        /// <inheritdoc />
        public abstract Task<bool> SaveAsync();

        /// <inheritdoc />
        public abstract Task<string> ExportAsync();

        /// <inheritdoc />
        public abstract bool Copy([AllowNull] IField other);

        /// <inheritdoc />
        public abstract bool Equals([AllowNull] IField other);
    }
}