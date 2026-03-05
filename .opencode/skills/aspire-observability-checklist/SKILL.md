---
name: aspire-observability-checklist
description: Verifies OpenTelemetry, health endpoints, and service discovery defaults in Aspire-hosted services
compatibility: opencode
metadata:
  domain: observability
---
## What I do
- Validate baseline service defaults for telemetry and health.
- Check instrumentation assumptions after API/Web changes.
- Provide a quick sanity checklist for Aspire dashboard visibility.

## Checklist
1. Confirm `AddServiceDefaults()` is used in service startup.
2. Confirm default health endpoints are mapped in development.
3. Confirm HTTP client service discovery is preserved.
4. Confirm traces/metrics/logs are still emitted.
5. Confirm no change accidentally suppresses critical diagnostics.

## Working examples from this repo
- Service defaults in API: `AspireChat/AspireChat.Api/Program.cs` calls `builder.AddServiceDefaults()` and `app.MapDefaultEndpoints()`.
- Service defaults in Web: `AspireChat/AspireChat.Web/Program.cs` calls `builder.AddServiceDefaults()` and `app.MapDefaultEndpoints()`.
- Service discovery HTTP clients: `AspireChat/AspireChat.Web/Program.cs` sets typed client `BaseAddress = new("https://api")`.
- Test-time health wait: `AspireChat/AspireChat.Tests/WebTests.cs` waits for `WaitForResourceHealthyAsync("web", ...)` before assertions.
