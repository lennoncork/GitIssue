using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GitIssue.Values;

namespace GitIssue.Fields.Array
{
    /// <summary>
    ///     An issue field with an array of values
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ArrayField<T> : Field, IArrayField<T>
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

        /// <inheritdoc />
        public Type ValueType => typeof(T);

        /// <inheritdoc />
        public T[] Values
        {
            get => values.ToArray();
            set => values = new List<T>(value);
        }

        bool IList.IsFixedSize => ((IList) values).IsFixedSize;

        /// <inheritdoc />
        bool IList.IsReadOnly => ((IList) values).IsReadOnly;

        /// <inheritdoc />
        int ICollection.Count => ((ICollection) values).Count;

        /// <inheritdoc />
        bool ICollection.IsSynchronized => ((ICollection) values).IsSynchronized;

        /// <inheritdoc />
        object ICollection.SyncRoot => ((ICollection) values).SyncRoot;

        /// <inheritdoc />
        public int Count => values.Count;

        bool ICollection<T>.IsReadOnly => ((ICollection<T>) values).IsReadOnly;

        /// <inheritdoc />
        public T this[int index]
        {
            get => values[index];
            set => values[index] = value;
        }

        /// <inheritdoc />
        object? IList.this[int index]
        {
            get => ((IList) values)[index];
            set => ((IList) values)[index] = value;
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
                    if (!values.Contains(result))
                        values.Add(result);
                return Task.FromResult(true);
            }

            if (input.StartsWith('-'))
            {
                if (TryParse(input.TrimStart('-'), out result))
                    if (values.Contains(result))
                        values.Remove(result);
                return Task.FromResult(true);
            }

            if (input.StartsWith('[') && input.EndsWith(']'))
            {
                this.values.Clear();
                var values = input.TrimStart('[').TrimEnd(']').Split(',');
                foreach (var value in values)
                    if (TryParse(value.Trim(), out result))
                        if (!this.values.Contains(result))
                            this.values.Add(result);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        /// <inheritdoc />
        void IList.RemoveAt(int index)
        {
            values.RemoveAt(index);
        }

        /// <inheritdoc />
        int IList.Add(object? value)
        {
            return ((IList) values).Add(value);
        }

        /// <inheritdoc />
        void IList.Clear()
        {
            values.Clear();
        }

        /// <inheritdoc />
        bool IList.Contains(object? value)
        {
            return ((IList) values).Contains(value);
        }

        /// <inheritdoc />
        int IList.IndexOf(object? value)
        {
            return ((IList) values).IndexOf(value);
        }

        /// <inheritdoc />
        void IList.Insert(int index, object? value)
        {
            ((IList) values).Insert(index, value);
        }

        void IList.Remove(object? value)
        {
            ((IList) values).Remove(value);
        }

        void ICollection.CopyTo(System.Array array, int index)
        {
            ((IList) values).CopyTo(array, index);
        }

        /// <inheritdoc />
        public int IndexOf(T item)
        {
            return values.IndexOf(item);
        }

        /// <inheritdoc />
        public void Insert(int index, T item)
        {
            values.Insert(index, item);
        }

        /// <inheritdoc />
        public void RemoveAt(int index)
        {
            values.RemoveAt(index);
        }

        /// <inheritdoc />
        public void Add(T item)
        {
            values.Add(item);
        }

        /// <inheritdoc />
        public bool Contains(T item)
        {
            return values.Contains(item);
        }

        /// <inheritdoc />
        public void CopyTo(T[] array, int arrayIndex)
        {
            values.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc />
        public bool Remove(T item)
        {
            return values.Remove(item);
        }

        /// <inheritdoc />
        public void Clear()
        {
            values.Clear();
        }

        /// <inheritdoc />
        bool IArrayField.TryParse(string input, out object? value)
        {
            if (ValueExtensions.TryParse(input, out T result))
            {
                value = result;
                return true;
            }

            value = null;
            return false;
        }

        /// <inheritdoc />
        public bool TryParse(string input, out T value)
        {
            return ValueExtensions.TryParse(input, out value);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append("[");
            for (var i = 0; i < values.Count; i++)
            {
                if (i > 0) builder.Append(", ");
                builder.Append(Values[i]);
            }
            builder.Append("]");
            return builder.ToString();
        }
    }
}