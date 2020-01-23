using System;

namespace GitIssue
{
    /// <summary>
    /// Helper class for dealing with tasks
    /// </summary>
    public class SafeResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SafeResult"/> class
        /// </summary>
        /// <param name="success">the success of the task</param>
        public SafeResult(bool success = false)
        {
            this.IsSuccess = success;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SafeResult"/> class
        /// </summary>
        /// <param name="exception">the exception thrown</param>
        public SafeResult(Exception exception) : this(false)
        {
            this.Exception = exception;
        }

        /// <summary>
        /// Returns true if the command is a success
        /// </summary>
        public bool IsSuccess { get; set; } = false;

        /// <summary>
        /// Captures the exception if failed
        /// </summary>
        public Exception Exception { get; set; } = null;

        /// <summary>
        /// Returns a successful command
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public static SafeResult Success() => new SafeResult(true);

        /// <summary>
        /// Returns a failed command
        /// </summary>
        /// <returns></returns>
        public static SafeResult Fail() => new SafeResult();

        /// <summary>
        /// Returns a failed command from an exception
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static SafeResult Fail(Exception e) => new SafeResult(e);
    }

    /// <summary>
    /// Helper class for dealing with tasks
    /// </summary>
    public class SafeResult<T> : SafeResult
    {
        /// <summary>
        /// Returns a failed command
        /// </summary>
        /// <returns></returns>
        public new static SafeResult<T> Fail() => new SafeResult<T>();

        /// <summary>
        /// Returns a failed command from an exception
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public new static SafeResult<T> Fail(Exception e) => new SafeResult<T>(e);

        /// <summary>
        /// Returns a successful command with a response
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public static SafeResult<T> Success(T result) => new SafeResult<T>(result);

        /// <summary>
        /// Initializes a new instance of the <see cref="SafeResult"/> class
        /// </summary>
        /// <param name="exception"></param>
        public SafeResult(Exception exception) : base(exception)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SafeResult"/> class
        /// </summary>
        public SafeResult(bool success = false) : base(success)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SafeResult"/> class
        /// </summary>
        /// <param name="result"></param>
        public SafeResult(T result) : base(true)
        {
            this.Result = result;
        }

        /// <summary>
        /// The task result
        /// </summary>
        public T Result { get; set; }
    }
}
