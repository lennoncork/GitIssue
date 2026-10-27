using System.Threading.Tasks;

namespace GitIssue.Tool
{
    public interface IProcessRunner
    {
        Task<int> RunAsync(string fileName, params string[] arguments);
    }
}