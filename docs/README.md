# GitIssue User Guide

GitIssue stores issue data in your Git repository, so issues can be versioned and shared with the repository. This guide covers installing the command-line tool, starting an issue repository, and the most common workflows.

## Contents

- [Getting started](#getting-started)
- [Everyday workflows](#everyday-workflows)
- [Where GitIssue stores data](#where-gitissue-stores-data)
- [More help](#more-help)
- [Command reference](commands.md)
- [Configuration](configuration.md)

## Getting started

### Requirements

- Git
- .NET 10
- A working directory inside a Git repository

### Install the tool

Install the latest published tool:

```shell
dotnet tool install --global GitIssue.Tool
```

A global install runs as `gitissue`. The tool can also be installed locally using a .NET tool manifest, in which case the command is `dotnet gitissue`. See the [.NET tool documentation](https://learn.microsoft.com/en-us/dotnet/core/tools/global-tools#install-a-local-tool) for local tool setup.

### Initialize GitIssue

From the root directory of the Git repository where you want to keep issues, run:

```shell
gitissue init
```

This creates an `.issues` directory and the default issue configuration. To use a non-default directory, pass `--name` and use the same value in later commands:

```shell
gitissue init --name work-items
gitissue fields --name work-items
```

### Create and view an issue

Create an issue by providing its title and description:

```shell
gitissue create "Add onboarding guide" "Document installation and first-use workflows."
```

GitIssue prints the new issue key. A newly created issue is tracked automatically if no other issue is being tracked. View the current tracked issue with:

```shell
gitissue show
```

You can also view all issues with `gitissue show all`. For the available commands and their options, run:

```shell
gitissue help
gitissue create --help
```

### Use GitIssue from Copilot

From the repository root, install the MCP server into Visual Studio Code:

```shell
gitissue mcp install
```

Reload the window and trust the `gitissue` server when Visual Studio Code asks. Copilot can then list and update issues in the working tree. The server does not commit. See the [command reference](commands.md#mcp-server).

### Open the local website

From the repository root:

```shell
gitissue web serve
```

The site listens on `http://127.0.0.1:5080`. Add `--open` to launch a browser. This is separate from `gitissue mcp serve`.

## Everyday workflows

Commands that accept an issue key can use the key printed when the issue was created. For commands that support tracking, `.` (or `T` / `Tracked`) means the currently tracked issue.

```shell
# Select an issue to work on
gitissue track ISSUE_KEY

# View the tracked issue
gitissue show

# Edit the title or another field
gitissue edit . Title "Write the getting-started guide"

# Add and remove values from an array field such as Labels
gitissue add . Labels documentation
gitissue remove . Labels documentation

# Add a comment
gitissue comment . "The first draft is ready for review."

# Find issues using an expression over issue objects
gitissue find 'i => i.Title.ToString().Contains("guide")'

# Review recorded changes and commit the issues to Git
gitissue changes
gitissue commit
```

The `edit` command opens the editor when no field and value are supplied. The `create` command opens the editor when no title is supplied. The default editor is `joe`.

`gitissue commit` commits changes to issue data in the Git repository. It does not replace your normal Git workflow for other repository files.

## Where GitIssue stores data

By default, GitIssue creates `.issues` in the working directory. That directory contains `config.json`, the issue files, `tracking.json`, and `changes.json`. Each issue is a Markdown document. `gitissue init --storage json` selects one JSON document per issue. `gitissue validate` reports fields that do not match the configured schema.

The `.issues` directory is part of your repository. Review and commit it according to your project's normal practices. Back it up before making manual changes to its files.

## More help

- [Command reference](commands.md) lists the available verbs and common options.
- [Configuration](configuration.md) describes the default fields and repository settings.
- Use `gitissue <command> --help` for the tool's current option details.
- Report unclear or missing user documentation through the repository's normal issue or contribution process.
