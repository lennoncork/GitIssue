using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GitIssue
{
    public static class SafeResultExtensions
    {
        public static async Task<SafeResult> WithSafeResult(this Task task)
        {
            try
            {
                await task;
                return SafeResult.Success();
            }
            catch (Exception e)
            {
                return SafeResult.Fail(e);
            }
        }

        public static async Task<SafeResult<T>> WithSafeResult<T>(this Task<T> task)
        {
            try
            {
                return new SafeResult<T>(await task);
            }
            catch (Exception e)
            {
                return SafeResult<T>.Fail(e);
            }
        }
    }
}
