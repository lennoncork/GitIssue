using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace GitIssue.Syncs
{
    /// <summary>
    ///     Imports issues from a JSON description
    /// </summary>
    public class FileImporter : Importer
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="FileImporter" /> class
        /// </summary>
        /// <param name="manager"></param>
        public FileImporter(IIssueManager manager) : base(manager)
        {
        }

        /// <summary>
        ///     Gets or sets the file to import
        /// </summary>
        public string ImportPath { get; set; } = "import.json";

        /// <inheritdoc />
        public override async Task<bool> Import()
        {
            if (!File.Exists(this.ImportPath))
            {
                return false;
            }

            string content = await File.ReadAllTextAsync(this.ImportPath);
            JsonObject json = JsonNode.Parse(content) as JsonObject ?? throw new JsonException($"Unable to deserialize {this.ImportPath} as a JSON object.");
            return await this.Import(json);
        }
    }
}