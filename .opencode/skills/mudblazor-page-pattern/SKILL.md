---
name: mudblazor-page-pattern
description: Applies consistent MudBlazor page patterns for forms, dialogs, tables, and feedback states
compatibility: opencode
metadata:
  domain: ui
---
## What I do
- Promote consistent page composition in Blazor/MudBlazor.
- Ensure loading, success, and failure states are handled clearly.
- Keep UX behavior aligned across login, groups, chat, and profile flows.

## UI checklist
1. Use `MudForm` validation for user inputs.
2. Provide disabled/loading states for async actions.
3. Show success/error feedback with alerts or snackbars.
4. Keep navigation outcomes explicit after actions.
5. Maintain readability and responsive layouts.

## Working examples from this repo
- Form pattern: `AspireChat/AspireChat.Web/Components/Pages/Login.razor` uses `MudForm` + `MudTextField` with required validation and `_loading`-gated submit button.
- Error feedback pattern: `AspireChat/AspireChat.Web/Components/Pages/Login.razor` displays `MudAlert Severity="Error"` for login/register failures.
- Data table pattern: `AspireChat/AspireChat.Web/Components/Pages/Groups.razor` uses `MudTable` with row actions and a dialog-driven create flow.
- Chat composer pattern: `AspireChat/AspireChat.Web/Components/Pages/Chat.razor` uses `MudTextField` with adornment send action and enter-key handling.
