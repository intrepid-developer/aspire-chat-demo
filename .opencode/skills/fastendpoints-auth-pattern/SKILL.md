---
name: fastendpoints-auth-pattern
description: Guides secure FastEndpoints patterns for route config, claim extraction, and response handling
compatibility: opencode
metadata:
  domain: api-security
---
## What I do
- Standardize auth-aware endpoint implementation details.
- Encourage predictable status codes and error responses.
- Reduce claim parsing and authorization inconsistencies.

## Endpoint checklist
1. Configure route and explicit anonymous/auth requirements.
2. Read claims defensively and validate parse outcomes.
3. Return precise failure responses (400/401/404/500 as appropriate).
4. Keep request/response contracts in `AspireChat.Common` aligned.
5. Avoid leaking sensitive details in error messages.

## Apply especially to
- Login/register/profile/update flows.
- Any endpoint that depends on `ClaimTypes.Sid` or user identity context.

## Working examples from this repo
- Anonymous auth entrypoint: `AspireChat/AspireChat.Api/Users/LoginEndpoint.cs` sets `AllowAnonymous()` and returns JWT claims including `ClaimTypes.Sid`.
- Claim extraction target: `AspireChat/AspireChat.Api/Users/GetProfileEndpoint.cs` reads `User.FindFirst(ClaimTypes.Sid)?.Value` and loads user profile.
- Update flow target: `AspireChat/AspireChat.Api/Users/UpdateEndpoint.cs` validates user identity from claims before mutating EF entities.
- Global auth setup reference: `AspireChat/AspireChat.Api/Program.cs` uses `.AddAuthenticationJwtBearer(...)`, `.AddAuthorization()`, and `app.UseAuthentication(); app.UseAuthorization();`.
