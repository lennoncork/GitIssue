using System;
using System.IO;
using System.Threading.Tasks;

namespace GitIssue
{
    /// <summary>
    ///     The paths for the folders
    /// </summary>
    public static class Paths
    {
        internal static string GetContainedPath(string root, string path)
        {
            string rootPath = Path.GetFullPath(root);
            string candidatePath = Path.GetFullPath(Path.Combine(rootPath, path));
            string relativePath = Path.GetRelativePath(rootPath, candidatePath);

            if (relativePath == "." ||
                Path.IsPathRooted(relativePath) ||
                relativePath.Equals("..", StringComparison.Ordinal) ||
                relativePath.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal) ||
                relativePath.StartsWith($"..{Path.AltDirectorySeparatorChar}", StringComparison.Ordinal))
            {
                throw new ArgumentException($"Path '{path}' must be contained within '{root}'.", nameof(path));
            }

            return candidatePath;
        }

        internal static async Task WriteAllTextAsync(string path, string? content)
        {
            string destinationPath = Path.GetFullPath(path);
            string directory = Path.GetDirectoryName(destinationPath)
                ?? throw new ArgumentException($"Path '{path}' does not have a parent directory.", nameof(path));
            Directory.CreateDirectory(directory);

            string temporaryPath = Path.Combine(directory, $".{Path.GetFileName(path)}.{Guid.NewGuid():N}.tmp");
            try
            {
                await File.WriteAllTextAsync(temporaryPath, content ?? string.Empty);
                File.Move(temporaryPath, destinationPath, true);
            }
            finally
            {
                if (File.Exists(temporaryPath))
                {
                    File.Delete(temporaryPath);
                }
            }
        }

        /// <summary>
        ///     Gets or sets the change log file name
        /// </summary>
        public static string ChangeLogFileName { get; set; } = "changes.json";

        /// <summary>
        ///     Gets or sets the config file name
        /// </summary>
        public static string ConfigFileName { get; set; } = "config.json";

        /// <summary>
        ///     Gets or sets the name of the git folder
        /// </summary>
        public static string GitFolderName { get; set; } = ".git";

        /// <summary>
        ///     Gets or sets the name of the issues folder
        /// </summary>
        public static string IssueRootFolderName { get; set; } = ".issues";

        /// <summary>
        ///     Gets or sets the tracked issue file name
        /// </summary>
        public static string TrackedIssueFileName { get; set; } = "tracking.json";
    }
}