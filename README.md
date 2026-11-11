# GitIssue

GitIssue is an in-source issue management system built on Git. It stores issues in your repository and supports a flexible issue schema.

![build](https://github.com/lennoncork/GitIssue/actions/workflows/build.yml/badge.svg?branch=main)
[![NuGet](https://img.shields.io/nuget/v/GitIssue.Tool)](https://www.nuget.org/packages/GitIssue.Tool)
[![NuGet Downloads](https://img.shields.io/nuget/dt/GitIssue.Tool)](https://www.nuget.org/packages/GitIssue.Tool)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE.md)

## Quick start

Install the .NET tool and initialize issue data from the root of a Git repository:

```shell
dotnet tool install --global GitIssue.Tool
gitissue init
gitissue create "Example issue" "Describe the work to be done."
gitissue help
```

A global install runs as `gitissue`. A local tool install runs as `dotnet gitissue`.

To open the local website, or to expose issues to GitHub Copilot in Visual Studio Code:

```shell
gitissue web serve
gitissue mcp install
```

See the [GitIssue User Guide](docs/README.md) for installation, workflows, commands, and configuration.

## Building from source

To build from source, clone this repository, build, and configure the optional Git alias.

```shell
git clone git@github.com:lennoncork/GitIssue.git
dotnet tool restore
dotnet cake
./scripts/config-alias.sh
```

The alias runs `.publish/GitIssue.Tool.exe`, which the Cake publish target builds for `win-x64`.

## Compatibility

The tool targets .NET 10 and supports Windows and Linux.

## Contributing

Contributions are welcome. See the [user guide](docs/README.md) for project usage and the repository's contribution process.

## Thanks
Thank you to [JetBrains](https://www.jetbrains.com/) for providing developers with an Open Source License for [ReSharper](https://www.jetbrains.com/resharper/).
