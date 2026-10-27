using System.Threading.Tasks;

namespace GitIssue.Tool.Commands
{
    /// <summary>
    ///     The GitIssueCommand
    /// </summary>
    public abstract class Command<T> where T : Options
    {
        /// <summary>
        ///     Gets the process exit code for this command. Defaults to <c>0</c>.
        /// </summary>
        public int ExitCode { get; protected set; }

        /// <summary>
        ///     Executes the command
        /// </summary>
        /// <param name="options">the command options</param>
        /// <returns></returns>
        public abstract Task Exec(T options);
    }
}