# GitIssue Design

## Vision

GitIssue is an in-source issue management system: it keeps a project's issues
as files in that project's Git repository instead of requiring a separate
hosted issue tracker or database. The issue data is therefore versioned and
shared alongside the code, and contributors can review and commit issue changes
using familiar Git workflows.

The project provides both a reusable .NET library for issue data and repository
operations and a .NET command-line tool for everyday issue workflows. Its
configurable schema allows repositories to describe issues with fields beyond
the defaults while keeping the data in a portable, human-readable format.

## Requirements

For the project to fulfill that vision, it must:

- Store issue records and their configuration within a Git working tree so
  they can be versioned and shared with that repository.
- Support creating, reading, updating, deleting, finding, tracking, importing,
  and exporting issues through the library and/or CLI.
- Represent issue fields through a configurable schema, with typed scalar and
  array values and support for custom issue keys and value types.
- Provide a command-line interface with help and useful success or failure
  results for its supported workflows.
- Integrate issue changes with the repository's Git history while keeping
  issue management separate from the user's normal source-code commits.
- Run on the supported Windows and Linux environments using .NET 10.
- Include automated tests and repeatable build, formatting, analysis, and
  coverage checks so changes can be validated consistently.
- Keep user and contributor documentation aligned with the behavior implemented
  by the tool and library.

## Architecture

### Solution structure

The solution is organized into four projects under `src/`:

- `GitIssue` is the core library. It defines issue and field contracts,
  configuration, serialization, repository discovery, issue management,
  change tracking, and import/sync abstractions.
- `GitIssue.Tool` is the executable .NET tool, exposed as `dotnet gitissue`.
  It parses command options, wires dependencies, invokes library operations,
  and formats terminal output.
- `GitIssue.Tests` tests the library, including issue persistence, fields,
  configuration, Git operations, and import/sync behavior.
- `GitIssue.Tool.Tests` tests CLI command parsing and end-to-end command
  workflows against temporary repositories.

### Core issue model and schema

`IIssueManager` is the main application-facing service. `IssueManager` creates,
loads, deletes, searches, and tracks issues, using injected key, configuration,
repository, change-log, and persistence services. Autofac registrations in
`GitIssueModule` provide the default implementations and allow selected
services to be overridden.

An issue is modeled by `IIssue` and the abstract `Issue` base class. Fields are
identified by `FieldKey` and described by `FieldInfo`, which specifies the
value type, field representation, required status, and metadata. The default
`IssueConfiguration` defines key, title, description, author, created and
updated timestamps, labels, and comments. Field factories and readers create
typed scalar or array fields from this schema; the value types and type
converters provide serialization and parsing for the supported domain values.

### Repository data and persistence

`RepositoryRoot` identifies the working tree and the issue-data directory
(`.issues` by default, or a name supplied by the user). The directory contains
the JSON configuration and issue data. The default file-backed issue
implementation (`FileIssue`/`JsonIssue`) reads and writes configured fields
using JSON serialization. The library also contains file-backed field
implementations, allowing field storage to be selected by the configured field
type.

The tracked issue and the issue change log are persisted separately from the
issue fields. The tracked-issue file supports the CLI's current-issue shortcut;
the change log records issue operations for display and commit messaging.
`IssueManager.CommitAsync` stages issue-data changes and creates a Git commit
through LibGit2Sharp. It refuses the commit when a merge is in progress or when
index changes already exist under the issues directory. Staged adds, edits,
deletions, and renames outside that directory stay in the index and are left
out of the issue commit. Ignored files under the issues directory are not
added. The tracking file and change log are excluded from the commit, and the
cleared change log is written before the method returns.

### Command-line layer

`Program` uses CommandLineParser to map options to command classes, and Autofac
to construct each command and its dependencies. Each command derives from
`Command<T>` and delegates issue operations to `IIssueManager`. The command
families cover initialization, create/show/edit/delete, field updates and
comments, find/track, Git changes and commits, and data import/export/sync.
Interactive editing and terminal formatting are implemented in the tool
project.

Import and sync code sits in the core library's `Syncs` namespace, with CLI
commands adapting supported input formats to that layer. The `find` command
also evaluates user-supplied C# expressions against issue objects; expressions
should therefore only be run when trusted.

## Validation

The repository uses Cake targets defined in `build.cake`, with shared .NET
build settings in `src/Directory.Build.props`. The principal local commands
from the repository root are:

```shell
dotnet tool restore
dotnet restore src/GitIssue.slnx --source https://api.nuget.org/v3/index.json
dotnet cake --target=Build
dotnet cake --target=Format
dotnet cake --target=Analyzers
dotnet cake --target=Test
dotnet cake --target=Report
```

`Build` builds the solution, `Format` checks formatting without changing
files, `Analyzers` runs the analyzer check, and `Test` runs both NUnit test
projects. `Coverage` collects test coverage, and `Report` produces the
Markdown coverage summary. The Cake `Default` target combines these checks
with packaging and publishing work.

The GitHub Actions build workflow runs on Windows and Ubuntu with .NET 10 and
invokes Cake targets for build, formatting, analyzers, validation, tests, and
coverage reporting. The workflow currently names a `Validate` target, while
`build.cake` does not define one; treat the declared Cake targets as the
authoritative local commands until that workflow/target discrepancy is
resolved.

Tests use NUnit and temporary Git repositories to exercise the library and
CLI behavior. Prefer focused tests in the relevant test project, then use the
Cake targets to validate wider changes.

## Documentation

Documentation is maintained as Markdown in the repository:

- `README.md` is the concise project introduction, quick start, and build
  entry point.
- `docs/README.md` is the end-user guide and links to the detailed references.
- `docs/commands.md` describes CLI commands and options.
- `docs/configuration.md` explains issue fields and repository configuration.
- `AGENTS.md` provides build, test, coding, and documentation guidance for
  agents and contributors.
- `DESIGN.md` describes the system's vision, requirements, architecture,
  validation approach, and documentation structure.

Keep examples and descriptions tied to supported behavior. When a change
affects user workflows or configuration, update the relevant guide and command
or configuration reference alongside the implementation. Keep this design
document focused on enduring project structure and approach rather than
duplicating the detailed user guide.
