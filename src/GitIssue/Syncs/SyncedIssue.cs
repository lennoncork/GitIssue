using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GitIssue.Issues;

namespace GitIssue.Syncs
{
    /// <summary>
    /// Maps issues and imports
    /// </summary>
    public struct SyncedIssue
    {
        private readonly IImporter importer;

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncedIssue"/> class
        /// </summary>
        /// <param name="importer"></param>
        /// <param name="kvp"></param>
        public SyncedIssue(IImporter importer, KeyValuePair<IssueKey, IssueKey> kvp)
        {
            this.importer = importer;
            this.IssueKey = kvp.Key;
            this.ImportKey = kvp.Value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncedIssue"/> class
        /// </summary>
        /// <param name="importer"></param>
        /// <param name="key"></param>
        /// <param name="import"></param>
        public SyncedIssue(IImporter importer, IssueKey key, IssueKey import)
        {
            this.importer = importer;
            this.IssueKey = key;
            this.ImportKey = import;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncedIssue"/> class
        /// </summary>
        /// <param name="importer"></param>
        /// <param name="key"></param>
        /// <param name="import"></param>
        public SyncedIssue(IImporter importer, string key, string import)
        {
            this.importer = importer;
            this.IssueKey = key;
            this.ImportKey = import;
        }

        /// <summary>
        /// Gets the issue key
        /// </summary>
        public IssueKey IssueKey { get; }

        /// <summary>
        /// Gets the import key
        /// </summary>
        public IssueKey ImportKey { get; }

        /// <summary>
        /// Gets a value indicating if this issue key is valid
        /// </summary>
        public bool IsValid => this.IssueKey != IssueKey.None || this.ImportKey != IssueKey.None;

        /// <summary>
        ///     Implicit cast to string
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator KeyValuePair<IssueKey, IssueKey>(SyncedIssue value)
        {
            return new KeyValuePair<IssueKey, IssueKey>(value.IssueKey, value.ImportKey);
        }

        /// <summary>
        /// Reads the imported mapping
        /// </summary>
        /// <param name="importer"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static async Task<SyncedIssue> ReadAsync(IImporter importer, string path)
        {
            if (File.Exists(path) == false)
                return default!;

            var import = Path.GetFileNameWithoutExtension(path);
            var key = await File.ReadAllTextAsync(path);
            return new SyncedIssue(importer, key, import);
        }

        /// <summary>
        /// Saves the imported file
        /// </summary>
        /// <returns></returns>
        public async Task<bool> SaveAsync()
        {
            if (Directory.Exists(importer.Root.ImportPath) == false)
                Directory.CreateDirectory(importer.Root.ImportPath);

            var path = Path.Combine(importer.Root.ImportPath, this.ImportKey.ToString());
            var key = this.IssueKey.ToString();
            await File.WriteAllTextAsync(path, key);
            return true;
        }
    }
}
