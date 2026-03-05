---
name: distributed-test-playbook
description: Establishes reliable Aspire distributed integration test patterns and diagnostics
compatibility: opencode
metadata:
  domain: testing
---
## What I do
- Standardize integration tests built on `DistributedApplicationTestingBuilder`.
- Improve reliability of startup, health waiting, and HTTP assertions.
- Encourage clear diagnostics when distributed tests fail.

## Test pattern
1. Build app host with explicit cancellation and timeout.
2. Start application and wait for target resource healthy state.
3. Create named client and execute focused request assertions.
4. Keep assertions specific and actionable.
5. Log enough context to debug failures quickly.

## Working examples from this repo
- End-to-end harness: `AspireChat/AspireChat.Tests/WebTests.cs` creates app host via `DistributedApplicationTestingBuilder.CreateAsync<Projects.AspireChat_AppHost>(...)`.
- Timeout discipline: `AspireChat/AspireChat.Tests/WebTests.cs` applies `DefaultTimeout = TimeSpan.FromSeconds(30)` with `WaitAsync(...)` for build/start/health wait.
- Named client usage: `AspireChat/AspireChat.Tests/WebTests.cs` calls `app.CreateHttpClient("web")` and validates `GET /` returns `200 OK`.
- Execution command: `dotnet test AspireChat/AspireChat.Tests`.
