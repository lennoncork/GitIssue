using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace GitIssue.Fields
{
    /// <summary>
    /// Implementation of an empty field
    /// </summary>
    public class EmptyField : Field
    {
        /// <summary>
        /// Initializes a new instance of an empty field
        /// </summary>
        /// <param name="key"></param>
        public EmptyField(FieldKey key) : base(key)
        {
        }

        /// <inheritdoc/>
        public override bool Copy([AllowNull] IField other) => true;

        /// <inheritdoc/>
        public override bool Equals([AllowNull] IField other) => other is EmptyField;

        /// <inheritdoc/>
        public override Task<string> ExportAsync() => Task.FromResult(string.Empty);

        /// <inheritdoc/>
        public override Task<bool> SaveAsync() => Task.FromResult(false);

        /// <inheritdoc/>
        public override bool Update(string input) => false;
    }
}
