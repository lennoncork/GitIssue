# Command Reference

Run commands from the root of the Git repository containing the issue data. The examples use `gitissue`, the command from a global install. A local install is invoked as `dotnet gitissue`. If you have configured the optional `git issue` alias, you can use that prefix instead.

Most commands accept these shared options:

| Option | Default | Description |
| --- | --- | --- |
| `--name` | `.issues` | Name of the issue-data directory. Use the same name used when initializing. |
| `--path` | Current directory | Path to the working directory. |
| `--tracking` | `tracking.json` | Name of the tracked-issue file. |

Options are case-insensitive. Run `gitissue <command> --help` for parser-generated usage.

## Initialize and create

| Command | Usage | Description |
| --- | --- | --- |
| `init` | `init [--storage markdown\|json]` | Initialize issue data in the current repository. The default directory is `.issues`. Each issue is saved as Markdown unless `--storage json` is set. |
| `create` | `create [Title] [Description]` | Create an issue. If no title is given, the default editor opens. A new issue is tracked if no issue is currently tracked; `--Track` forces tracking of the new issue. |
| `fields` | `fields` | List configured fields and their types. |
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
| `convert` | `convert --to json\|markdown` | Rewrite every issue into the selected storage format and update `config.json`. |

Examples:

```shell
gitissue export issues.json
gitissue export issues.csv --Separator ";"
gitissue import issues.json
gitissue import issues.csv --Separator ";"
gitissue commit
```

CSV import/export uses a simple separator-based format with a header row. The header names must match configured field names (for example `Key;Title;Description`). The parser splits on the specified separator; it is not a full quoted-CSV parser. CSV import currently uses `Key` to find an issue and `Title` to create a new issue; it does not import the other CSV field values. The `--Update` option is accepted, but is not currently applied by the command implementation.

JSON import reads one JSON object representing an issue and requires `Key` and `Title`; `Description` is optional. For example:

```json
{
  "Key": "EXT-1",
  "Title": "Imported issue",
  "Description": "Imported issue description."
}
```

## Help

```shell
gitissue help
gitissue --help
gitissue <command> --help
gitissue version
```
