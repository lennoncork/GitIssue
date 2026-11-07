# Configuration

GitIssue reads issue-repository configuration from `config.json` in the issue-data directory. The default directory is `.issues`, so the default configuration path is `.issues/config.json`.

Use the fields command to inspect the fields recognized by the current repository:

```shell
gitissue fields
```

## Default fields

| Field | Type | Required | Notes |
| --- | --- | --- | --- |
| `Key` | Scalar issue key | Yes | Identifies an issue. |
| `Title` | Scalar string | Yes | Short issue summary. |
| `Description` | Scalar string | Yes | Longer issue description. |
| `Author` | Scalar signature | Yes | Author associated with the issue. |
| `Created` | Scalar date/time | Yes | Creation time. |
| `Updated` | Scalar date/time | Yes | Last update time. |
| `Labels` | Array of labels | No | Labels for categorization. |
| `Comments` | Array of strings | No | Issue comments. |

Required fields are part of the default schema. Array fields support `add` and `remove`; use `edit` for scalar fields.

## Changing the issue directory

Initialization and later commands must use the same issue-data directory name:

```shell
gitissue init --name work-items
gitissue create "Example issue" "Created in the custom directory." --name work-items
```

The `--path` option selects the working directory, and `--tracking` changes the tracked-issue file name. These options are available on commands that use the shared options.

## Editing `config.json`

The configuration defines the key provider and each field's field/value types, metadata, and required status. Make a backup before editing it manually: changing a field's type or name can make existing issue data unreadable or prevent commands from working. Use the `fields` command afterward to confirm that the repository still loads and reports the expected schema.

Interactive commands use `joe` as the default editor. Although some commands expose editor-related options, editor selection is not consistently applied across commands in the current implementation.
