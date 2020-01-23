using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GitIssue.Fields
{
    /// <summary>
    /// An issue field with an array of values
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ArrayField<T> : Field, IEnumerable<T>
    {
        /// <summary>
        /// Creates a new instance of the <see cref="ArrayField{T}"/> class
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        protected ArrayField(FieldKey key, T[] value) : base(key)
        {
            this.Values = value;
        }

        /// <summary>
        /// Field values
        /// </summary>
        public T[] Values { get; set; }

        /// <inheritdoc/>
        public IEnumerator<T> GetEnumerator()
        {
            foreach (var value in Values)
            {
                yield return value;
            }
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}
