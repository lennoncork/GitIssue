using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GitIssue.Keys;
using GitIssue.Values;

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
            T result;
            if (input.StartsWith('+'))
            {
                if (TryParse(input.TrimStart('+'), out result))
                {
                    if (!this.values.Contains(result))
                        this.values.Add(result);
                }
                return Task.FromResult(true);
            }

            if (input.StartsWith('-'))
            {
                if (TryParse(input.TrimStart('-'), out result))
                {
                    if(this.values.Contains(result))
                        this.values.Remove(result);
                }
                return Task.FromResult(true);
            }

            if (input.StartsWith('[') && input.EndsWith(']'))
            {
                this.values.Clear();
                string[] values = input.TrimStart('[').TrimEnd(']').Split(',');
                foreach (var value in values)
                {
                    if (TryParse(value.Trim(), out result))
                    {
                        if (!this.values.Contains(result))
                            this.values.Add(result);
                    }
                }
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
        internal static bool TryParse(string input, out T value) => ValueExtensions.TryParse(input, out value);

        /// <inheritdoc />
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < this.values.Count; i++)
            {
                if (i > 0) builder.Append(", ");
                builder.Append(this.Values[i]);
            }
            return builder.ToString();
        }
    }
}