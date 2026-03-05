---
description: Create a pull request with a clear summary of what changed
agent: orchestrator
---
Create a pull request for the current branch.

Base branch: `$1` (default to `main` when not provided).

Requirements:

1. Inspect branch state, status, and commit history against the base branch.
2. Ensure the branch is pushed (set upstream if needed).
3. Draft a concise PR title and body covering:
   - Summary of changes
   - Why the changes were made
   - Testing/verification performed
   - Risks or follow-ups
4. Create the PR with `gh pr create`.
5. Return the PR URL and the final title/body used.

If PR creation is blocked (auth, remote, no diff), report the exact blocker and next action.
