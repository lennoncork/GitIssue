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

`KeyProvider` in `config.json` selects how new issue keys are generated and how those keys map to directories under the issue-data folder. The default is `DateIssueKeyProvider`. Choose a provider when initializing:

```shell
gitissue init
gitissue init --key-provider date
gitissue init --key-provider name
```

| Provider | Key example | Directory layout | Best for |
| --- | --- | --- | --- |
| `DateIssueKeyProvider` | `2026-11-09-A1B2C3D4` | `.issues/2026/11/09/A1B2C3D4/` | Bugs and other time-ordered work. The date groups issues on disk; the trailing unique id avoids collisions. |
| `NameIssueKeyProvider` | `A1B2C3D4 - Add login page` | `.issues/A1B2C3D4 - Add login page/` | Features and other named work. The unique id stays unique in a distributed repository; the title makes the key readable. Quote keys that contain spaces, or pass the eight-character unique id to commands that resolve keys. |

Existing repositories that still name `FileIssueKeyProvider` keep that date-based behavior. New repositories should use `DateIssueKeyProvider`.

The date format is a poor fit for feature work because the calendar path says when the issue was created, not what it is. Named keys reverse that: the title is visible in the key, and uniqueness still comes from a short id rather than a shared counter.

Other schemes that are not built in, but are reasonable later providers:

- **Slug plus unique id**, such as `A1B2C3D4-add-login-page`. Same uniqueness as named keys, without spaces, so command-line use is easier.
- **Type-prefixed keys**, such as `BUG-2026-11-09-A1B2C3D4` or `FEAT-A1B2C3D4`. Useful when one repository tracks more than one issue kind.
- **Content-addressed hashes**, as in git-bug: the id is a hash of the first change, shown as a short prefix. Opaque to read, but collision-free and git-like.
- **Random UUID or ULID**. Fossil uses a 40-character hex ticket id. ULIDs stay unique and sort by time.
- **Sequential ids**, such as `ISS-1`. Familiar from Jira and GitHub, but two clones can mint the same number before they push.

Sequential numbers are a poor default here because GitIssue is distributed: keys must stay unique without a central allocator.

## Editing `config.json`

The configuration defines the key provider and each field's field/value types, metadata, and required status. Make a backup before editing it manually: changing a field's type or name can make existing issue data unreadable or prevent commands from working. Use the `fields` command afterward to confirm that the repository still loads and reports the expected schema.

Interactive commands use `joe` as the default editor. Although some commands expose editor-related options, editor selection is not consistently applied across commands in the current implementation.
