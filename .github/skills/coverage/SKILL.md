---
name: coverage
description: Run code coverage for the repository's unit tests using the Cake Coverage task.
scope: repository
version: 1.0
---

# Coverage Skill

## Description

This skill runs the repository's Coverage Cake task to execute the unit tests and produce a coverage report in the `./coverage` directory.

## Prerequisites
- Install a compatible .NET SDK (see `dotnet --info`).
- Restore local dotnet tools with `dotnet tool restore`.

## Commands
From the repository root (the directory containing `build.cake`):

```bash
dotnet tool restore
dotnet restore src/GitIssue.sln --source https://api.nuget.org/v3/index.json
dotnet cake --target=Coverage
```

## Guidance
- Prefer the `dotnet cake` `Coverage` target when you need coverage output for the test suite.
- Review the generated reports in `./coverage` after the task completes.
- Use the test skill for standard test execution when coverage output is not needed.
