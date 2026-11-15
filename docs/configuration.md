# Configuration

GitIssue reads issue-repository configuration from `.gitissue.config` in the repository root, next to `.gitignore`. Issue files are stored in that same directory. A `bug` issue lives under `bugs`, with no `.issues` folder.

## Project name

`ProjectName` is the title of the local website. `gitissue init --project-name "Apollo"` writes it into `.gitissue.config`. When the property is omitted or blank, the website title is GitIssue.

```shell
gitissue init --project-name "Apollo"
```

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
| `IssueType` | Scalar string | Automatically managed | Selects the issue's schema; read-only through issue commands. |

Required fields are part of the default schema. Array fields support `add` and `remove`; use `edit` for scalar fields.

## Changing the issue directory

By default, issue files are stored in the repository root. `--name` selects a subdirectory instead. Use the same name for later commands. `.gitissue.config`, `.gitissue.tracking`, and `.gitissue.changes` stay in the repository root either way.

```shell
gitissue init --name work-items
gitissue create "Example issue" "Created in the custom directory." --name work-items
```

The `--path` option selects the working directory, and `--tracking` changes the tracked-issue file name. These options are available on commands that use the shared options.

## Issue storage

`Storage` selects the file written for each issue. New repositories use `Markdown`: one `.md` document per issue. `gitissue init --storage json` selects one `.json` document per issue. `init` always writes `Storage`. A configuration that omits it uses Markdown.

```shell
gitissue init
gitissue init --storage json
```

A Markdown issue is `<key-path>/<key>.md` in the repository root, for example `bugs/2026/11/13/UID/bugs-2026-11-13-UID.md`. Markdig parses the document. Each configured field is a level-2 heading, in the order stored in `.gitissue.config`. The level-1 heading repeats the issue key. Labels and other single-value arrays are bullet lists. Comments are level-3 blocks. `.gitissue.config`, `.gitissue.tracking`, and `.gitissue.changes` stay JSON, and `export` / `import` still use JSON or CSV.

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

`gitissue convert --to markdown` and `gitissue convert --to json` rewrite every issue and update `Storage`. Before writing, the command validates the source documents and stops if any document is invalid or an issue already has both a `.json` and a `.md` file. Correct the reported errors before converting so unknown or invalid fields are not discarded.

## Multiple issue types

Every schema lives in the `Issues` array. `gitissue init` writes one entry, named `Issue` unless `--issue-type` selects another name, and stores the default fields on that entry. The root `IssueType` selects which entry is used when a command does not pass `--type`. Each entry supplies its complete `Fields` schema, including the required core fields shown below. Names are case-sensitive and use ASCII letters and digits. `IssueType` is reserved: do not define it in `Fields`.

```json
{
  "IssueType": "BUG",
  "KeyProvider": "DateIssueKeyProvider",
  "KeyTemplate": "{type}s/{created:yyyy}/{created:MM}/{created:dd}/{uid}",
  "Storage": "Markdown",
  "Issues": [
    {
      "Name": "BUG",
      "Fields": {
        "Key": { "FieldType": "Value", "ValueType": "IssueKey", "Required": true },
        "Title": { "FieldType": "Value", "ValueType": "String", "Required": true },
        "Description": { "FieldType": "Value", "ValueType": "String", "Required": true },
        "Author": { "FieldType": "Value", "ValueType": "Signature", "Required": true },
        "Created": { "FieldType": "Value", "ValueType": "DateTime", "Required": true },
        "Updated": { "FieldType": "Value", "ValueType": "DateTime", "Required": true },
        "Severity": { "FieldType": "Value", "ValueType": "String", "Required": true }
      }
    },
    {
      "Name": "FEATURE",
      "Fields": {
        "Key": { "FieldType": "Value", "ValueType": "IssueKey", "Required": true },
        "Title": { "FieldType": "Value", "ValueType": "String", "Required": true },
        "Description": { "FieldType": "Value", "ValueType": "String", "Required": true },
        "Author": { "FieldType": "Value", "ValueType": "Signature", "Required": true },
        "Created": { "FieldType": "Value", "ValueType": "DateTime", "Required": true },
        "Updated": { "FieldType": "Value", "ValueType": "DateTime", "Required": true },
        "Proposal": { "FieldType": "Value", "ValueType": "String", "Required": true }
      }
    }
  ]
}
```

The default type must match an entry in `Issues`. Use `gitissue fields --type BUG` to inspect its fields and `gitissue create "Crash" "Details" --type BUG --field Severity=high` to create one. Multiple initial values follow the same `--field` option, for example `--field Severity=high Platform=linux`. When no title is supplied, the editor prompts for the selected type's fields.

The issue document stores its own `IssueType` as a JSON property or Markdown `## IssueType` section. It determines the schema independently of the key and cannot be changed through field editing. Documents without it use the configured default, even if their key contains another type; the type is written on their next save. A document with an invalid type, or a type not listed in `Issues`, is rejected. Changing the default `IssueType` does not change the stored type of existing issues.

## Issue keys

Issue keys are generated from `KeyTemplate` in `.gitissue.config`. The issue's stored `IssueType` supplies the `{type}` token. `KeyProvider` selects the implementation; the template is what actually shapes the key. A `/` in the template becomes a directory. Other characters, including `-`, stay in the folder name. The issue key is the same string with `/` replaced by `-`.

A new repository uses `{type}s/{created:yyyy}/{created:MM}/{created:dd}/{uid}`. `{type}s` is the type name followed by `s`, and it is the first directory. A `bug` issue is stored under `bugs`; an `Issue` is stored under `Issues`. `--key-provider date` keeps the older year/month/day layout. Markdown storage omits a trailing `{uid}` directory, so issues created on the same day share that folder and the file is named after the key. JSON storage keeps the `{uid}` directory.

## Layout

Each issue type has a `Layout`. `Template` is the default: `gitissue create` writes the key-template folders, and the document is named after the key. `Free` keeps a short key `{type}-{uid}` inside the file. The file can live anywhere under the type directory, for example `specifications/auth/login.md`. Pass `--location auth/login.md` when creating one. The uid is a random value stored in the `Key` heading. It is not derived from the folder, so moving the file does not change the issue.

GitIssue finds an issue by the `Key` heading in the document, so a template issue moved to `bugs/random_folder/issue.md` is still opened by its key. A Markdown file with no `Key` heading is left alone. Two documents with the same key are an error. `gitissue rekey` moves template issues back onto the key template and leaves free-layout files where they are.

```shell
gitissue init
gitissue init --key-provider date
gitissue init --key-provider name
gitissue init --issue-type FEAT --key-template "{type}-{created:yyyyMMdd}-{index:D3}-{uid}-{slug}"
```

| Token | Meaning | Default format |
| --- | --- | --- |
| `{type}` | The issue's type, defaulting to the configured `IssueType`. `{type}s` appends `s`, so `bug` becomes the directory `bugs` | `Issue` |
| `{created}` | Issue creation time. Use .NET date format specifiers, for example `{created:yyyy}`, `{created:MM}`, `{created:dd}`, or `{created:yyyy-MM-dd}` | `yyyy-MM-dd` |
| `{index}` | Sequential integer assigned in this repository. Use `{index:D3}` for `001`. Unique across clones only if `{uid}` is also present | unpadded |
| `{uid}` | Eight-character hexadecimal id | n/a |
| `{name}` or `{title}` | Sanitized issue title, with spaces | n/a |
| `{slug}` | Lowercase hyphenated title | n/a |

| Provider | Default template | Key example | Directory layout |
| --- | --- | --- | --- |
| default | `{type}s/{created:yyyy}/{created:MM}/{created:dd}/{uid}` | `Issues-2026-11-09-A1B2C3D4` | Markdown: `Issues/2026/11/09/Issues-2026-11-09-A1B2C3D4.md`. JSON: `Issues/2026/11/09/A1B2C3D4/` |
| `DateIssueKeyProvider` | `{created:yyyy}/{created:MM}/{created:dd}/{uid}` | `2026-11-09-A1B2C3D4` | Markdown: `2026/11/09/2026-11-09-A1B2C3D4.md`. JSON: `2026/11/09/A1B2C3D4/` |
| `NameIssueKeyProvider` | `{uid} - {name}` | `A1B2C3D4 - Add login page` | `A1B2C3D4 - Add login page/` |
| `TemplateIssueKeyProvider` | the configured `KeyTemplate` | depends on the template | depends on `/` in the template |

If `KeyTemplate` is set, it overrides the provider's default. Existing repositories that still name `FileIssueKeyProvider` keep date-based behavior until you change the template.

After you edit `KeyTemplate`, regenerate on-disk names:

```shell
gitissue rekey --dry-run
gitissue rekey
```

`rekey` locates issues by their stored `.json` or `.md` files, so they can be renamed even when the previous key no longer matches the new template. It rebuilds each key from the current template using the issue title, created time, type, and the unique id already present in the existing key. Quote keys that contain spaces, or pass the eight-character unique id to commands that resolve keys.

`{index}` is convenient, but two clones can mint the same index before they push. Keep `{uid}` in the template when the repository is distributed.

## Editing `.gitissue.config`

The configuration defines the key provider, key template, issue type, and each field's field/value types, metadata, and required status. Make a backup before editing it manually: changing a field's type or name can make existing issue data unreadable or prevent commands from working. Use the `fields` command afterward to confirm that the repository still loads and reports the expected schema.

`FieldType` describes the shape of the field. `Value` is one value and `Array` is a list. `ValueType` is the kind of value, such as `String`, `Label`, or `DateTime`. These names are the same for Markdown and JSON issue files.

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
