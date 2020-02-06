using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using GitIssue.Keys;

namespace GitIssue.Fields
{
    /// <summary>
    ///     An issue field with an array of values
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ArrayField<T> : Field, IEnumerable<T>
    {
        private List<T> values;

        /// <summary>
        ///     Creates a new instance of the <see cref="ArrayField{T}" /> class
        /// </summary>
        /// <param name="key"></param>
        /// <param name="values"></param>
        protected ArrayField(FieldKey key, T[] values) : base(key)
        {
            this.values = new List<T>(values);
        }

        /// <summary>
        ///     Field values
        /// </summary>
        public T[] Values
        {
            get => this.values.ToArray();
            set => this.values = new List<T>(value);
        }

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator()
        {
            foreach (var value in Values) yield return value;
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc />
        public override Task<bool> UpdateAsync(string input)
        {
            if (TryParse(input, out T result))
            {
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        /// <summary>
        ///     Tries to parse the string input to the output value
        /// </summary>
        /// <param name="input">the input string</param>
        /// <param name="value">the output value</param>
        /// <returns></returns>
        internal static bool TryParse(string input, out T value)
        {
            var converter = TypeDescriptor.GetConverter(typeof(T));
            if (converter.CanConvertFrom(typeof(string)))
            {
                value = (T)converter.ConvertFrom(input);
                return true;
            }
            value = default;
            return false;
        }
    }
}