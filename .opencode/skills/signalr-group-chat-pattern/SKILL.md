---
name: signalr-group-chat-pattern
description: Defines stable SignalR group chat patterns for hub methods, web connection lifecycle, and message flow
compatibility: opencode
metadata:
  domain: realtime
---
## What I do
- Standardize group join/leave and message broadcast behavior.
- Align API persistence + hub push + UI updates.
- Reduce reconnect and duplicate-subscription bugs.

## Core pattern
1. Persist chat message in API before broadcasting.
2. Broadcast to group using canonical event names.
3. In web client, connect once per active group and clean up handlers on dispose.
4. Recompute local user identity on reconnect when needed.

## Verification
- Validate message appears in sender and recipient sessions.
- Validate leaving a group stops incoming group messages.
- Validate chat history and live updates render consistently.

## Working examples from this repo
- Hub methods: `AspireChat/AspireChat.Api/Hubs/GroupChatHub.cs` (`JoinGroup`, `LeaveGroup`).
- Broadcast event: `AspireChat/AspireChat.Api/Chats/SendEndpoint.cs` sends `ReceiveMessage` to `Clients.Group(req.GroupId.ToString())`.
- Client subscription: `AspireChat/AspireChat.Web/Services/ChatHubService.cs` registers `_connection.On<GetAll.Dto>("ReceiveMessage", ...)` and joins/leaves groups.
- UI integration: `AspireChat/AspireChat.Web/Components/Pages/Chat.razor` computes `IsMe` and appends incoming messages in the SignalR callback.
