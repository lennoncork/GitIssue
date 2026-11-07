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

The tool can also be installed locally using a .NET tool manifest. See the [.NET tool documentation](https://learn.microsoft.com/en-us/dotnet/core/tools/global-tools#install-a-local-tool) for local tool setup.

### Initialize GitIssue

From the root directory of the Git repository where you want to keep issues, run:

```shell
dotnet gitissue init
```

This creates an `.issues` directory and the default issue configuration. To use a non-default directory, pass `--name` and use the same value in later commands:

```shell
dotnet gitissue init --name work-items
dotnet gitissue fields --name work-items
```

### Create and view an issue

Create an issue by providing its title and description:

```shell
dotnet gitissue create "Add onboarding guide" "Document installation and first-use workflows."
```

GitIssue prints the new issue key. A newly created issue is tracked automatically if no other issue is being tracked. View the current tracked issue with:

```shell
dotnet gitissue show
```

You can also view all issues with `dotnet gitissue show all`. For the available commands and their options, run:

```shell
dotnet gitissue help
dotnet gitissue create --help
```

## Everyday workflows

Commands that accept an issue key can use the key printed when the issue was created. For commands that support tracking, `.` (or `T` / `Tracked`) means the currently tracked issue.

```shell
# Select an issue to work on
dotnet gitissue track ISSUE_KEY

# View the tracked issue
dotnet gitissue show

# Edit the title or another field
dotnet gitissue edit . Title "Write the getting-started guide"

# Add and remove values from an array field such as Labels
dotnet gitissue add . Labels documentation
dotnet gitissue remove . Labels documentation

# Add a comment
dotnet gitissue comment . "The first draft is ready for review."

# Find issues using an expression over issue objects
dotnet gitissue find 'i => i.Title.Contains("guide")'

# Review recorded changes and commit the issues to Git
dotnet gitissue changes
dotnet gitissue commit
```

The `edit` command opens the editor when no field and value are supplied. The `create` command opens the editor when no title is supplied. The default editor is `joe`.

`dotnet gitissue commit` commits changes to issue data in the Git repository. It does not replace your normal Git workflow for other repository files.

## Where GitIssue stores data

By default, GitIssue creates `.issues` in the current Git repository. It contains `config.json` and the issue data. The tracked issue is stored separately in `tracking.json`; the change log is stored in `changes.json`.

The `.issues` directory is part of your repository. Review and commit it according to your project's normal practices. Back it up before making manual changes to its files.

## More help

- [Command reference](commands.md) lists the available verbs and common options.
- [Configuration](configuration.md) describes the default fields and repository settings.
- Use `dotnet gitissue <command> --help` for the tool's current option details.
- Report unclear or missing user documentation through the repository's normal issue or contribution process.
