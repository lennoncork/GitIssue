# GitIssue

An 'in-source' issue management system built on GIT. Inspired by the great work of the [git-issue](https://github.com/dspinellis/git-issue) project. This project implements a similar concept in a .NET core application with a flexible issue schema. 

# Compatibility
This project runs on .NET Core 3.1 and is compatible with the following,
* Windows 
* Linux

## Usage
You use _git issue_ with the following sub-commands.

## Configuration

### Editor

## Help

### Initialization
* `git issue init`: Create a new issues repository in the current directory.

### Work with an issue
* `git issue create`: Create a new open issue
* `git issue track`: Tracks an existing issue, simplifies subsequent commands
* `git issue delete`: Create a specified or tracked issue
* `git issue show`: Showa a specified (or tracked) issue
* `git issue edit`: Edits a specified (or tracked) issue
* `git issue commit`: Commits an issue to the repository

### Help and debug
* `git issue help`: Display help information about git issue.
* `git issue fields`: Display the fields available in the issue repository
* `git issue export`: Dump the whole database in json format to stdout.
* `git issue version`: Displays the version


## Contributing
This project is in it's early phases of development and contributions  are very welcome. 

