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

## Coding standards

Follow the repository's standard .NET and C# conventions when editing code:

- Prefer idiomatic C# and modern language features that are already used in the codebase.
- Keep code simple, readable, and consistent with the surrounding implementation.
- Use nullable reference types and explicit types where they improve clarity.
- Favor expression-bodied members, pattern matching, and other language features when they make the code easier to read.
- Follow existing naming conventions, including PascalCase for types and methods, camelCase for locals and parameters, and UPPERCASE for constants where already used.
- Preserve existing architecture and dependency patterns rather than introducing new abstractions unless they are clearly justified.
- Keep public APIs consistent with the existing design and avoid unnecessary breaking changes.
- Use the repository's established testing style and keep changes covered by relevant tests.
- When converting or refactoring code, prefer straightforward .NET standard library approaches over custom abstractions unless the existing codebase already uses a specific pattern.
- Match the repository's formatting and style choices, and avoid introducing unrelated style churn.

## Testing standards

The repository's tests are written with NUnit and follow a consistent structure that should be preserved when adding or updating tests:

- Prefer NUnit attributes such as `[Test]`, `[TestFixture]`, `[SetUp]`, and `[OneTimeSetUp]` for test structure and lifecycle.
- Keep tests focused on a single behavior or scenario and use descriptive names that reflect the expected outcome.
- Reuse the existing helpers and base test infrastructure in the test project rather than introducing ad-hoc patterns.
- Use temporary directories and files through the shared helpers so tests remain isolated and deterministic.
- Avoid brittle assertions and prefer checks that validate observable behavior rather than implementation details.
- Keep tests fast, independent, and repeatable; do not rely on shared mutable state between tests.
- When a behavior changes, update or add the corresponding regression test rather than only changing production code.
- Follow the repository's existing test organization by placing new tests alongside related suites under the appropriate test folder.