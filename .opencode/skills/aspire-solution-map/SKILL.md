---
name: aspire-solution-map
description: Maps project structure, responsibilities, and baseline local development commands
compatibility: opencode
metadata:
  audience: contributors
  scope: repository
---
## What I do
- Identify each project role in the AspireChat solution.
- Provide a quick path map for API, Web, AppHost, Common, and Tests.
- Recommend baseline local commands for run, build, and test.

## When to use me
Use this before making changes if the task spans multiple projects or you need to orient quickly.

## Standard orientation checklist
1. Confirm solution and project layout.
2. Locate entry points (`Program.cs`) and core runtime paths.
3. Identify contract-sharing boundaries (`AspireChat.Common`).
4. Identify verification commands to run after changes.

## Working examples from this repo
- Solution file: `AspireChat/AspireChat.sln`.
- Entry points: `AspireChat/AspireChat.AppHost/Program.cs`, `AspireChat/AspireChat.Api/Program.cs`, `AspireChat/AspireChat.Web/Program.cs`.
- Contract boundary examples: `AspireChat/AspireChat.Common/Users/Login.cs`, `AspireChat/AspireChat.Common/Chats/GetAll.cs`.
- Baseline commands:
  - `dotnet run --project AspireChat/AspireChat.AppHost`
  - `dotnet build AspireChat/AspireChat.sln`
  - `dotnet test AspireChat/AspireChat.Tests`
