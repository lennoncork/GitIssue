using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GitIssue
{
    /// <summary>
    /// Extension methods for safely getting the result of a task
    /// </summary>
    public static class SafeResultExtensions
    {
        /// <summary>
        /// Safely returns the result of a task
        /// </summary>
        /// <param name="task">the task to get the result from</param>
        /// <returns></returns>
        public static SafeResult WithSafeResult(this Task task) =>
            Task.Run(task.WithSafeResultAsync).Result;

        /// <summary>
        /// Safely returns the result of a task
        /// </summary>
        /// <param name="task">the task to get the result from</param>
        /// <returns></returns>
        public static SafeResult<T> WithSafeResult<T>(this Task<T> task) => 
            Task.Run(task.WithSafeResultAsync<T>).Result;

        /// <summary>
        /// Safely returns the result of a task
        /// </summary>
        /// <param name="task">the task to get the result from</param>
        /// <returns></returns>
        public static async Task<SafeResult> WithSafeResultAsync(this Task task)
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

        /// <summary>
        /// Safely returns the result of a task
        /// </summary>
        /// <param name="task">the task to get the result from</param>
        /// <returns></returns>
        public static async Task<SafeResult<T>> WithSafeResultAsync<T>(this Task<T> task)
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
