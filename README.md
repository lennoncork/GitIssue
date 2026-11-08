# GitIssue

GitIssue is an in-source issue management system built on Git. It stores issues in your repository and supports a flexible issue schema.

![build](https://github.com/lennoncork/GitIssue/actions/workflows/build.yml/badge.svg?branch=main)

## Quick start

Install the .NET tool and initialize issue data from the root of a Git repository:

```shell
dotnet tool install --global GitIssue.Tool
gitissue init
gitissue create "Example issue" "Describe the work to be done."
gitissue help
```

A global install runs as `gitissue`. A local tool install runs as `dotnet gitissue`.

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

## Releasing

After the build workflow passes on `main`, create and publish a GitHub Release from a stable `vMAJOR.MINOR.PATCH` tag, such as `v0.3.0`. Configure NuGet Trusted Publishing for the `lennoncork/GitIssue` repository and the `deploy.yaml` workflow before publishing. The deployment workflow uses GitHub OIDC to obtain a short-lived NuGet API key, builds, tests, and packages both NuGet projects, saves the packages as an artifact, and publishes them to NuGet.org. GitVersion derives package versions from the release tag.

## Thanks
Thank you to [JetBrains](https://www.jetbrains.com/) for providing developers with an Open Source License for [ReSharper](https://www.jetbrains.com/resharper/).
