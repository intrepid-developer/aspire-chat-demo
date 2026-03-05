# OpenCode Project Configuration

This directory contains project-local OpenCode setup for Aspire Chat.

## Custom Commands

- `/commit` - Creates a commit with a sensible message and concise details.
- `/prc [base]` - Creates a pull request (default base is `main` if omitted in prompt logic).
- `/review [target]` - Runs a full review for a commit/range (defaults to `HEAD`).

Examples:

- `/commit`
- `/prc main`
- `/review HEAD~1..HEAD`

## Agent Strategy

### `@orchestrator` is the primary agent

This project treats `@orchestrator` as the default/primary agent for day-to-day work, including `/commit` and `/prc`.

Using `@orchestrator` by default keeps cross-project behavior consistent because it:

- coordinates work across Common, API, Web, AppHost, and tests,
- delegates to specialized subagents only when it helps,
- keeps command outputs aligned with the same final reporting expectations.

### Why `/review` uses `code-review-agent`

`/review` benefits from a focused reviewer profile with read-only edit tools and stricter command scope.

## Custom Agents

Agents live in `.opencode/agents/` and include:

- `orchestrator`
- `fastendpoints-api-engineer`
- `blazor-mudblazor-ui-engineer`
- `signalr-chat-specialist`
- `aspire-infra-deploy-agent`
- `dotnet-test-reliability-agent`
- `security-audit-agent`
- `docs-blog-sync-agent`
- `code-review-agent`

## Custom Skills

Skills live in `.opencode/skills/*/SKILL.md` and provide reusable workflows for architecture orientation, feature slicing, auth, SignalR, UI, EF migrations, observability, and distributed testing.
