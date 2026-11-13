# Command Reference

Run commands from the root of the Git repository containing the issue data. The examples use `gitissue`, the command from a global install. A local install is invoked as `dotnet gitissue`. If you have configured the optional `git issue` alias, you can use that prefix instead.

Most commands accept these shared options:

| Option | Default | Description |
| --- | --- | --- |
| `--name` | repository root | Directory for issue files. Defaults to the repository root. Use the same name used when initializing. |
| `--path` | Current directory | Path to the working directory. |
| `--tracking` | `.gitissue.tracking` | Name of the tracked-issue file in the repository root. |

Options are case-insensitive. Run `gitissue <command> --help` for parser-generated usage.

## Initialize and create

| Command | Usage | Description |
| --- | --- | --- |
| `init` | `init [--storage markdown\|json] [--key-provider date\|name\|template] [--key-template TEMPLATE] [--issue-type TYPE] [--layout template\|free]` | Initialize issue data in the current repository. `.gitissue.config` is written next to `.gitignore`. Each issue is saved as Markdown unless `--storage json` is set. The default key template is `{type}s/{created:yyyy}/{created:MM}/{created:dd}/{uid}`, so a `bug` issue is stored under `bugs` with no `.issues` folder. `--layout free` lets that type's files live anywhere under the type directory. `--key-provider date` uses `{created:yyyy}/{created:MM}/{created:dd}/{uid}`. `--key-provider name` uses `{uid} - {name}`. `--key-template` selects `TemplateIssueKeyProvider` and the given pattern, for example `{type}-{created}-{index}-{uid}-{slug}`. `--issue-type` sets the default type (default `Issue`). |
| `create` | `create [Title] [Description] [--type TYPE] [--location PATH] [--field NAME=VALUE ...]` | Create an issue using the selected or default type. Supply required custom fields with `--field`. `--location` sets the file path under the type directory for a free-layout type, such as `auth/login.md`. If no title is given, the default editor opens. A new issue is tracked if no issue is currently tracked; `--Track` forces tracking. |
| `fields` | `fields [--type TYPE]` | List fields for the selected or default issue type. |
| `validate` | `validate [Issue Key]` | Check issue documents against the configured fields. Omit the key to check every issue. `.`, `T`, and `Tracked` select the tracked issue. The command exits with code 1 when any field is invalid. |

## View, search, and track

| Command | Usage | Description |
| --- | --- | --- |
| `show` | `show [tracked\|all\|mine]` | Show the tracked issue (default), all issues, or `mine`. The `mine` option currently shows all issues rather than filtering by author. |
| `find` | `find [Expression] [--Format FORMAT] [--LinqName NAME]` | Find issues using a C# expression that returns `bool` for an issue. The default expression matches all issues. |
| `track` | `track [Issue Key]` | Set the tracked issue. |

Example:

```shell
gitissue find 'i => i.Title.ToString().Contains("guide")'
gitissue find 'i => i.Title.ToString().Contains("guide")' --Format "%Key %Title"
```

The default find format is `%Key %Title %Delta`. The expression parameter is evaluated by the tool; only run expressions you trust.

For commands that accept an issue key, `.`, `T`, and `Tracked` refer to the current tracked issue.

## Update and delete

| Command | Usage | Description |
| --- | --- | --- |
| `edit` | `edit [Issue Key] [Field Key] [Value]` | Update a field. If no field is specified, edit the whole issue with the default editor. If a field is supplied without a value, open that field in the editor. |
| `add` | `add [Issue Key] [Field Key] [Value]` | Add a value to an array field, for example a label. |
| `remove` | `remove [Issue Key] [Field Key] [Value]` | Remove a value from an array field. |
| `comment` | `comment [Issue Key] [Comment]` | Add a comment. If omitted, the default editor is used. |
| `delete` | `delete [Issue Key]` | Delete an issue. |

Examples:

```shell
gitissue edit ISSUE_KEY Title "Improve first-run experience"
gitissue add ISSUE_KEY Labels documentation
gitissue remove ISSUE_KEY Labels documentation
gitissue comment ISSUE_KEY "Validated on Windows and Linux."
gitissue delete ISSUE_KEY
```

## Git and data exchange

| Command | Usage | Description |
| --- | --- | --- |
| `changes` | `changes` | Display the issue change log. |
| `commit` | `commit` | Commit changes to issue data in Git. |
| `export` | `export [File] [--Overwrite] [--Separator VALUE]` | Export issues to a `.json` or `.csv` file. Existing output files are not overwritten unless `--Overwrite` is specified. |
| `import` | `import [File] [--Update] [--Separator VALUE]` | Import issues from a `.json` or `.csv` file. JSON import expects `Key` and `Title` properties; `Description` is optional. |
| `sync` | `sync [File]` | Import issues from a JSON file. |
| `convert` | `convert --to json\|markdown` | Rewrite every issue into the selected storage format and update `.gitissue.config`. |
| `rekey` | `rekey [--dry-run]` | Rewrite issue keys and file names from the current `KeyTemplate` and each issue's stored type. `--dry-run` prints the planned renames without writing. |

Examples:

```shell
gitissue export issues.json
gitissue export issues.csv --Separator ";"
gitissue import issues.json
gitissue import issues.csv --Separator ";"
gitissue commit
```

CSV import/export uses a simple separator-based format with a header row. The header names must match configured field names (for example `Key;Title;Description`). The parser splits on the specified separator; it is not a full quoted-CSV parser. Export includes the union of configured fields and `IssueType`. Import uses the type to select a schema, imports matching field values, and uses the default type if the column is absent. `--Update` permits updating existing issues; it does not change an issue's type.

JSON import reads one JSON object representing an issue and requires `Key` and `Title`; `Description` and `IssueType` are optional. A missing type uses the configured default. Include any required type-specific fields. For example:

```json
{
  "Key": "EXT-1",
  "Title": "Imported issue",
  "Description": "Imported issue description."
}
```

## MCP server

`gitissue mcp` exposes the repository issues to an MCP client. The server reads and writes the working tree and does not commit. Logs go to standard error because standard output carries the MCP protocol.

| Command | Usage | Description |
| --- | --- | --- |
| `mcp serve` | `mcp serve [--path <directory>] [--name <folder>]` | Run the stdio MCP server. This is the default action. |
| `mcp install` | `mcp install [--scope workspace\|user] [--dry-run] [--insiders] [--command <command>] [--server-arg <arg>]` | Add the server to Visual Studio Code Copilot. The default file is `.vscode/mcp.json`. `--scope user` writes the user profile `mcp.json`. |
| `mcp uninstall` | `mcp uninstall [--scope workspace\|user] [--dry-run] [--insiders]` | Remove the `gitissue` entry and leave other servers in place. |

```shell
gitissue mcp install
gitissue mcp serve
```

After install, reload Visual Studio Code or run **MCP: List Servers**, then trust the `gitissue` server. A global tool uses command `gitissue`. For a local tool, pass `--command dotnet`. A custom issue directory is recorded with `--name`:

```shell
gitissue mcp install --command dotnet --name work-items
```

The server provides tools for listing, reading, finding, creating, and updating issues, plus the field schema, tracked issue, and change log. `list_issue_types` lists configured types; `list_fields` accepts an optional type, and `create_issue` accepts a type and initial field values. `find_issues` uses structured filters. It does not evaluate the C# expressions accepted by `gitissue find`.

## Web server

`gitissue web serve` runs a local website for the issues in the working tree. The ASP.NET host runs in the `gitissue` process and writes its log to the terminal. It is separate from `gitissue mcp serve`, which speaks MCP on standard input and output.

| Command | Usage | Description |
| --- | --- | --- |
| `web serve` | `web serve [--host <address>] [--port <port>] [--open] [--token <token>]` | Run the website. The default address is `http://127.0.0.1:5080`. `--open` launches a browser after the server responds. A host other than loopback requires `--token`. |

```shell
gitissue web serve
gitissue web serve --open
gitissue web serve --host 0.0.0.0 --token "$GITISSUE_TOKEN"
```

`web` with no action also starts the server. Shared options `--path` and `--name` select the repository and issue directory.

The landing page shows the tracked issue and a table of issues. Search matches
the issue key, title, or description without regard to case. Results are
displayed 10 per page, and the previous and next links retain the search.
Select an issue key to open its details. Each configured field has a separate
save button; the issue key and issue type are read-only and the updated timestamp is set
automatically on save. Array fields use the same syntax as
the CLI: `+value` adds, `-value` removes, and `[value1, value2]` replaces the
contents. `[]` clears the array. Validation errors are shown on the page.
Changes are saved to the working tree and are not committed automatically.

When `--token` is specified, every request must include the
`Authorization: Bearer <token>` header. An ordinary browser does not send this
header automatically, including when `--open` is used. Use a trusted,
authenticated reverse proxy that adds the header for browser requests, or
another client configured to supply it. The server itself uses HTTP, not HTTPS;
do not expose the token or server over an untrusted network without appropriate
transport security and access controls.

## Help

```shell
gitissue help
gitissue --help
gitissue <command> --help
gitissue version
```
