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

## Issue storage

`Storage` selects the file written for each issue. New repositories use `Markdown`: one `.md` document per issue. `gitissue init --storage json` selects one `.json` document per issue. A configuration that omits `Storage` stays on JSON so an existing repository keeps its current files.

```shell
gitissue init
gitissue init --storage json
```

A Markdown issue is `.issues/<key-path>/<key>.md`. Markdig parses the document. Each configured field is a level-2 heading, in the order stored in `config.json`. The level-1 heading repeats the issue key. Labels and other single-value arrays are bullet lists. Comments are level-3 blocks. `config.json`, `tracking.json`, and `changes.json` stay JSON, and `export` / `import` still use JSON or CSV.

```markdown
# 2026-11-08-ABCD1234

## Title

Save issues as markdown

## Description

Issue text, including nested markdown.

## Author

Ada Lovelace <ada@example.com>
```

`gitissue validate` checks every field against the schema. A required field must be present. Text other than `Description` must not be empty. Signatures, dates, emails, URLs, versions, numbers, labels, and enumerated values must parse. A label is one token. An unknown `##` heading, a duplicate field, or list text that is not a list is an error. Save, edit, and convert refuse to write a document that fails these checks.

`gitissue convert --to markdown` and `gitissue convert --to json` rewrite every issue and update `Storage`. The command stops when one issue already has both a `.json` and a `.md` file.

## Issue keys

Issue keys are generated from `KeyTemplate` in `config.json`. `IssueType` supplies the `{type}` token. `KeyProvider` selects the implementation; the template is what actually shapes the key. A `/` in the template becomes a directory. Other characters, including `-`, stay in the folder name. The issue key is the same string with `/` replaced by `-`.

```shell
gitissue init
gitissue init --key-provider date
gitissue init --key-provider name
gitissue init --issue-type FEAT --key-template "{type}-{created:yyyyMMdd}-{index:D3}-{uid}-{slug}"
```

| Token | Meaning | Default format |
| --- | --- | --- |
| `{type}` | `IssueType` from configuration, letters, digits, and underscore only | `ISSUE` |
| `{created}` | Issue creation time. Use .NET date format specifiers, for example `{created:yyyy}`, `{created:MM}`, `{created:dd}`, or `{created:yyyy-MM-dd}` | `yyyy-MM-dd` |
| `{index}` | Sequential integer assigned in this repository. Use `{index:D3}` for `001`. Unique across clones only if `{uid}` is also present | unpadded |
| `{uid}` | Eight-character hexadecimal id | n/a |
| `{name}` or `{title}` | Sanitized issue title, with spaces | n/a |
| `{slug}` | Lowercase hyphenated title | n/a |

| Provider | Default template | Key example | Directory layout |
| --- | --- | --- | --- |
| `DateIssueKeyProvider` | `{created:yyyy}/{created:MM}/{created:dd}/{uid}` | `2026-11-09-A1B2C3D4` | `.issues/2026/11/09/A1B2C3D4/` |
| `NameIssueKeyProvider` | `{uid} - {name}` | `A1B2C3D4 - Add login page` | `.issues/A1B2C3D4 - Add login page/` |
| `TemplateIssueKeyProvider` | the configured `KeyTemplate` | depends on the template | depends on `/` in the template |

If `KeyTemplate` is set, it overrides the provider's default. Existing repositories that still name `FileIssueKeyProvider` keep date-based behavior until you change the template.

After you edit `KeyTemplate` or `IssueType`, regenerate on-disk names:

```shell
gitissue rekey --dry-run
gitissue rekey
```

`rekey` locates issues by their stored `.json` or `.md` files, so they can be renamed even when the previous key no longer matches the new template. It rebuilds each key from the current template using the issue title, created time, type, and the unique id already present in the existing key. Quote keys that contain spaces, or pass the eight-character unique id to commands that resolve keys.

`{index}` is convenient, but two clones can mint the same index before they push. Keep `{uid}` in the template when the repository is distributed.

## Editing `config.json`

The configuration defines the key provider, key template, issue type, and each field's field/value types, metadata, and required status. Make a backup before editing it manually: changing a field's type or name can make existing issue data unreadable or prevent commands from working. Use the `fields` command afterward to confirm that the repository still loads and reports the expected schema.

`FieldType` describes the shape of the field. `Value` is one value and `Array` is a list. `ValueType` is the kind of value, such as `String`, `Label`, or `DateTime`. These names are the same for Markdown and JSON issue files. The fields are still read and written with the JSON value model. Configurations that still say `JsonValue` or `JsonArray` continue to load.

```json
"Title": {
  "FieldType": "Value",
  "ValueType": "String",
  "Required": true
},
"Labels": {
  "FieldType": "Array",
  "ValueType": "Label"
}
```

Interactive commands use `joe` as the default editor. Although some commands expose editor-related options, editor selection is not consistently applied across commands in the current implementation.
