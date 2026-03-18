using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
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
        public int Count => this.values.Count;

        /// <inheritdoc />
        public T this[int index]
        {
            get => this.values[index];
            set => this.values[index] = value;
        }

        /// <inheritdoc />
        public T[] Values
        {
            get => this.values.ToArray();
            set => this.values = new List<T>(value);
        }

        /// <inheritdoc />
        public Type ValueType => typeof(T);

        /// <inheritdoc />
        int ICollection.Count => ((ICollection)this.values).Count;

        bool IList.IsFixedSize => ((IList)this.values).IsFixedSize;

        /// <inheritdoc />
        bool IList.IsReadOnly => ((IList)this.values).IsReadOnly;

        bool ICollection<T>.IsReadOnly => ((ICollection<T>)this.values).IsReadOnly;

        /// <inheritdoc />
        bool ICollection.IsSynchronized => ((ICollection)this.values).IsSynchronized;

        /// <inheritdoc />
        object? IList.this[int index]
        {
            get => ((IList)this.values)[index];
            set => ((IList)this.values)[index] = value;
        }

        /// <inheritdoc />
        object ICollection.SyncRoot => ((ICollection)this.values).SyncRoot;

        /// <inheritdoc />
        object[]? IArrayField.Values
        {
            get => this.Values.Cast<object>().ToArray();
            set
            {
                if (value == null)
                {
                    return;
                }

                this.values.Clear();
                foreach (object v in value)
                {
                    if (v is T result)
                    {
                        this.values.Add(result);
                    }
                }
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("[");
            for (int i = 0; i < this.values.Count; i++)
            {
                if (i > 0)
                {
                    builder.Append(", ");
                }

                builder.Append(this.Values[i]);
            }

            builder.Append("]");
            return builder.ToString();
        }

        /// <inheritdoc />
        public bool TryParse(string input, out T value)
        {
            return ValueExtensions.TryParse(input, out value);
        }

        /// <inheritdoc />
        public void Add(T item)
        {
            this.values.Add(item);
        }

        /// <inheritdoc />
        public void Clear()
        {
            this.values.Clear();
        }

        /// <inheritdoc />
        public bool Contains(T item)
        {
            return this.values.Contains(item);
        }

        /// <inheritdoc />
        public void CopyTo(T[] array, int arrayIndex)
        {
            this.values.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc />
        public bool Remove(T item)
        {
            return this.values.Remove(item);
        }

        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator()
        {
            foreach (T value in this.Values)
            {
                yield return value;
            }
        }

        /// <inheritdoc />
        public override bool Equals([AllowNull] IField other)
        {
            if (other is IArrayField<T> valueField)
            {
                return this.Values?.SequenceEqual(valueField.Values) ?? false;
            }

            return false;
        }

        /// <inheritdoc />
        public override bool Copy([AllowNull] IField other)
        {
            if (other is IArrayField<T> valueField)
            {
                this.Values = valueField.Values.ToArray();
            }

            return false;
        }

        /// <inheritdoc />
        public override bool Update(string input)
        {
            T result;
            if (input.StartsWith('+'))
            {
                if (this.TryParse(input.TrimStart('+'), out result))
                {
                    if (!this.values.Contains(result))
                    {
                        this.values.Add(result);
                    }
                }

                return true;
            }

            if (input.StartsWith('-'))
            {
                if (this.TryParse(input.TrimStart('-'), out result))
                {
                    if (this.values.Contains(result))
                    {
                        this.values.Remove(result);
                    }
                }

                return true;
            }

            if (input.StartsWith('[') && input.EndsWith(']'))
            {
                this.values.Clear();
                string[] values = input.TrimStart('[').TrimEnd(']').Split(',');
                foreach (string value in values)
                {
                    if (this.TryParse(value.Trim(), out result))
                    {
                        if (!this.values.Contains(result))
                        {
                            this.values.Add(result);
                        }
                    }
                }

                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public int IndexOf(T item)
        {
            return this.values.IndexOf(item);
        }

        /// <inheritdoc />
        public void Insert(int index, T item)
        {
            this.values.Insert(index, item);
        }

        /// <inheritdoc />
        public void RemoveAt(int index)
        {
            this.values.RemoveAt(index);
        }

        /// <inheritdoc />
        int IList.Add(object? value)
        {
            return ((IList)this.values).Add(value);
        }

        /// <inheritdoc />
        void IList.Clear()
        {
            this.values.Clear();
        }

        /// <inheritdoc />
        bool IList.Contains(object? value)
        {
            return ((IList)this.values).Contains(value);
        }

        void ICollection.CopyTo(System.Array array, int index)
        {
            ((IList)this.values).CopyTo(array, index);
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <inheritdoc />
        int IList.IndexOf(object? value)
        {
            return ((IList)this.values).IndexOf(value);
        }

        /// <inheritdoc />
        void IList.Insert(int index, object? value)
        {
            ((IList)this.values).Insert(index, value);
        }

        void IList.Remove(object? value)
        {
            ((IList)this.values).Remove(value);
        }

        /// <inheritdoc />
        void IList.RemoveAt(int index)
        {
            this.values.RemoveAt(index);
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
    }
}