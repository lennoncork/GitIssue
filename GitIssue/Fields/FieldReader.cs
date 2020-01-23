using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using GitIssue.Issues;
using GitIssue.Json;

namespace GitIssue.Fields
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class FieldReader : IFieldReader
    {
        /// <inheritdoc/>
        public abstract bool CanCreateField(FieldInfo info);

        /// <inheritdoc/>
        public abstract bool CanReadField(FieldInfo info);

        /// <inheritdoc/>
        public IField CreateField(Issue issue, FieldKey key, FieldInfo info)
        {
            var method = this.GetType().GetMethods()
                .Where(m => m.IsPublic)
                .Where(m => m.Name == nameof(CreateField))
                .Where(m => m.IsGenericMethodDefinition)
                .Select(m => m.MakeGenericMethod(info.DataType.Type))
                .FirstOrDefault();

            if (method != null)
            {
                object[] args = { issue, key, info };
                return (IField)method.Invoke(this, args);
            }

            return null;
        }

        /// <inheritdoc/>
        public abstract IField CreateField<T>(Issue issue, FieldKey key, FieldInfo info);

        /// <inheritdoc/>
        public async Task<IField> ReadFieldAsync(Issue issue, FieldKey key, FieldInfo info)
        {
            var method = this.GetType().GetMethods()
                .Where(m => m.IsPublic)
                .Where(m => m.Name == nameof(ReadFieldAsync))
                .Where(m => m.IsGenericMethodDefinition)
                .Select(m => m.MakeGenericMethod(info.DataType.Type))
                .FirstOrDefault();
            
            if (method != null)
            {
                object[] args = { issue, key, info };
                Task task = (Task)method.Invoke(this, args);
                await task;
                var result = task.GetType().GetProperty("Result")?.GetValue(task);
                return result as IField;
            }

            return null;
        }

        /// <inheritdoc/>
        public abstract Task<IField> ReadFieldAsync<T>(Issue issue, FieldKey key, FieldInfo info);
    }
}
