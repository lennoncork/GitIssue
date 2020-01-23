using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace GitIssue.Fields
{
    /// <summary>
    /// Abstract field class
    /// </summary>
    public abstract class Field : IField
    {
        /// <summary>
        /// Creates a new instance of the <see cref="Field"/> class
        /// </summary>
        /// <param name="key">the field key</param>
        protected Field(FieldKey key)
        {
            this.Key = key;
        }

        /// <summary>
        /// Gets the key for the field
        /// </summary>
        public FieldKey Key { get; protected set; }

        /// <inheritdoc/>
        public abstract Task<bool> SaveAsync();
    }
}
