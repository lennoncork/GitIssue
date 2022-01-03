using System;

namespace GitIssue
{
    /// <summary>
    ///     Helper class for dealing with tasks
    /// </summary>
    public class SafeResult
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SafeResult" /> class
        /// </summary>
        /// <param name="success">the success of the task</param>
        public SafeResult(bool success = false)
        {
            IsSuccess = success;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SafeResult" /> class
        /// </summary>
        /// <param name="exception">the exception thrown</param>
        public SafeResult(Exception exception) : this()
        {
            Exception = exception;
        }

        /// <summary>
        ///     Returns true if the command is a success
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        ///     Captures the exception if failed
        /// </summary>
        public Exception? Exception { get; set; }

        /// <summary>
        ///     Returns a successful command
        /// </summary>
        /// <returns></returns>
        public static SafeResult Success()
        {
            return new SafeResult(true);
        }

        /// <summary>
        ///     Returns a failed command
        /// </summary>
        /// <returns></returns>
        public static SafeResult Fail()
        {
            return new SafeResult();
        }

        /// <summary>
        ///     Returns a failed command from an exception
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static SafeResult Fail(Exception e)
        {
            return new SafeResult(e);
        }
    }

    /// <summary>
    ///     Helper class for dealing with tasks
    /// </summary>
    public class SafeResult<T> : SafeResult
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SafeResult" /> class
        /// </summary>
        /// <param name="exception"></param>
        public SafeResult(Exception exception) : base(exception)
        {
            Result = default!;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SafeResult" /> class
        /// </summary>
        public SafeResult(bool success = false) : base(success)
        {
            Result = default!;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SafeResult" /> class
        /// </summary>
        /// <param name="result"></param>
        public SafeResult(T result) : base(true)
        {
            Result = result;
        }

        /// <summary>
        ///     The task result
        /// </summary>
        public T Result { get; set; }

        /// <summary>
        ///     Returns a failed command
        /// </summary>
        /// <returns></returns>
        public new static SafeResult<T> Fail()
        {
            return new SafeResult<T>();
        }

        /// <summary>
        ///     Returns a failed command from an exception
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public new static SafeResult<T> Fail(Exception e)
        {
            return new SafeResult<T>(e);
        }

        /// <summary>
        ///     Returns a successful command with a response
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public static SafeResult<T> Success(T result)
        {
            return new SafeResult<T>(result);
        }

        /// <summary>
        ///     Checks it a result was returned from the task
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool HasResult(out T result)
        {
            if (IsSuccess)
            {
                result = Result;
                return true;
            }

            result = default!;
            return false;
        }

        /// <summary>
        /// Gets the result of the command
        /// </summary>
        /// <param name="throwIfFailed">throw a <see cref="SafeResultException"/> if the task failed</param>
        /// <returns></returns>
        public T GetResult(bool throwIfFailed = true)
        {
            if (this.IsSuccess)
                return this.Result;

            if (throwIfFailed == false)
                return default!;

            if (this.Exception == null)
                throw new SafeResultException("Task execution failed");
            throw new SafeResultException("Task execution failed", this.Exception);
        }
    }
}