---
name: feature-slice-pattern
description: Applies a consistent end-to-end feature workflow across contracts, API, web client, UI, and tests
compatibility: opencode
metadata:
  workflow: feature-delivery
---
## What I do
- Define the order of changes for vertical feature slices.
- Keep API contracts and client expectations synchronized.
- Ensure each feature lands with at least minimal verification.

## Feature slice order
1. Update or add DTO contracts in `AspireChat.Common`.
2. Implement or update endpoint behavior in `AspireChat.Api`.
3. Update typed client methods in `AspireChat.Web/Clients`.
4. Implement UX updates in Razor pages/components.
5. Add or adjust tests in `AspireChat.Tests`.

## Quality checks
- Confirm auth requirements match endpoint behavior.
- Confirm response shape matches consuming UI.
- Run targeted build/tests before finalizing.

## Working examples from this repo
- Contracts first: chat DTOs live in `AspireChat/AspireChat.Common/Chats/GetAll.cs` and `AspireChat/AspireChat.Common/Chats/Send.cs`.
- API implementation: `AspireChat/AspireChat.Api/Chats/GetAllEndpoint.cs` and `AspireChat/AspireChat.Api/Chats/SendEndpoint.cs`.
- Client integration: `AspireChat/AspireChat.Web/Clients/ChatClient.cs` consumes `/chats/{groupId}` endpoints.
- UI integration: `AspireChat/AspireChat.Web/Components/Pages/Chat.razor` renders history and sends new messages.
- Feature verification: `dotnet build AspireChat/AspireChat.sln` and `dotnet test AspireChat/AspireChat.Tests`.
