# AGENTS

## Introduction

This repository is configured to work well with automated agents (such as GitHub Copilot agents) that read, modify, and validate the code. This document gives those agents (and human contributors) a single place to learn how to build and test the solution in a predictable way.

Agents should:
- Treat the solution as a standard .NET solution rooted at this repository directory.
- Use the commands in the sections below when validating changes.
- Prefer running tests frequently to ensure changes remain safe.

## Build and Test skills

Build and test instructions are maintained as reusable skills in the repository. See the skill files for canonical steps and guidance:

- [.github/skills/build/SKILL.md](.github/skills/build/SKILL.md)
- [.github/skills/test/SKILL.md](.github/skills/test/SKILL.md)

Quick commands (from the repository root):

```bash
dotnet tool restore
dotnet restore src/GitIssue.sln --source https://api.nuget.org/v3/index.json
dotnet cake --target=Build
dotnet cake --target=Test
```

Agents should prefer the `dotnet cake` targets for build and test to mirror CI behavior.