# GitIssue

An 'in-source' issue management system built on GIT. Inspired by the great work of the [git-issue](https://github.com/dspinellis/git-issue) project. This project implements a similar concept in a .NET core application with a flexible issue schema. 

![build](https://github.com/lennoncork/GitIssue/workflows/build/badge.svg?branch=master)

# Quick Start
To get started, clone this repository, build and configure an alias in GIT. 
1) Clone the repository
```
git clone git@github.com:lennoncork/GitIssue.git
```
2) Build the GitIssue Tool
```
cd GitIssue
dotnet publish --framework netcoreapp3.1 --runtime win10-x64 --configuration release src/GitIssue.Util/GitIssue.Util.csproj
```
3) Configure the git alias
```
./scripts/config-alias.sh
```
4) Try it out
```
git issue help
```

# Compatibility
This project runs on .NET Core 3.1 and is compatible with both Windows and Linux. 

## Configuration

The issue configuration is contained in the config.json file checked into source. This config file describes the set of fields, their types, and expected values. The config file is created automatically using the `init` command. 

## Help

### Initialization
* `git issue init`: Create a new issues repository in the current directory.

### Work with an issue
* `git issue create`: Create a new open issue.
* `git issue track`: Tracks an existing issue, simplifies subsequent commands.
* `git issue delete`: Create a specified or tracked issue.
* `git issue show`: Showa a specified (or tracked) issue.
* `git issue edit`: Edits a specified (or tracked) issue.
* `git issue commit`: Commits an issue to the repository.

### Help and debug
* `git issue help`: Display help information about git issue.
* `git issue fields`: Display the fields available in the issue repository.
* `git issue export`: Dump the whole database in json format to stdout.
* `git issue version`: Displays the version of the tool.

## Contributing
This project is in it's early phases of development and contributions are very welcome. 

