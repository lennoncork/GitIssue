---
name: coverage
description: Run code coverage for the repository's unit tests using the Cake Coverage task.
scope: repository
version: 1.0
---

# Coverage Skill

## Description

This skill runs the repository's Coverage Cake task to execute the unit tests and include only the `GitIssue` and `GitIssue.Tool` assemblies as configured in `coverage.settings`. The Validate target merges those results to `./artifacts/combined.xml` and requires at least 80% line coverage for `GitIssue` and 60% line coverage for `GitIssue.Tool`. The Report target writes a Markdown summary in `./coverage/CoverageReport`.

## Prerequisites
- Install a compatible .NET SDK (see `dotnet --info`).
- Restore local dotnet tools with `dotnet tool restore`.

## Commands
From the repository root (the directory containing `build.cake`):

```bash
dotnet tool restore
dotnet restore src/GitIssue.slnx --source https://api.nuget.org/v3/index.json
dotnet cake --target=Coverage
```

## Guidance
- Prefer the `dotnet cake` `Coverage` target when you need coverage output for the test suite.
- Review `./artifacts/combined.xml` and the generated summary in `./coverage/CoverageReport` after the task completes.
- Use the test skill for standard test execution when coverage output is not needed.
