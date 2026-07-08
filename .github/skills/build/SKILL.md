---
name: build
description: Canonical build instructions for the repository.
scope: repository
version: 1.0
---

# Build Skill

## Description

This skill contains the canonical build instructions for the repository. Agents should follow these steps when validating or modifying code.

## Prerequisites
- Install a compatible .NET SDK (see `dotnet --info`).
- Restore local dotnet tools with `dotnet tool restore`.

## Commands
From the repository root (the directory containing `build.cake`):

```bash
dotnet tool restore
dotnet restore src/GitIssue.sln --source https://api.nuget.org/v3/index.json
dotnet cake --target=Build
```

## Guidance
- Prefer the `dotnet cake` `Build` target to mirror CI.
- Use explicit Cake targets such as `Build` and `Push` rather than calling `dotnet build` directly, except for one-off debugging.
- If package restore fails against internal feeds, add `https://api.nuget.org/v3/index.json` as a fallback source.
