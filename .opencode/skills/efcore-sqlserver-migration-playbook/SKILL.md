---
name: efcore-sqlserver-migration-playbook
description: Provides safe EF Core SQL Server model and migration workflow guidance for API data changes
compatibility: opencode
metadata:
  domain: data
---
## What I do
- Guide entity and DbContext changes with migration discipline.
- Minimize breaking data changes and startup surprises.
- Keep API behavior consistent with schema updates.

## Migration playbook
1. Update entities and `AppDbContext` intentionally.
2. Generate migrations with meaningful names.
3. Review migration SQL impact before applying.
4. Verify app startup and endpoint behavior after migration.
5. Update tests impacted by schema changes.

## Watch-outs
- Required/nullable transitions.
- Unindexed lookup paths in chat/group queries.
- Data backfill requirements for new non-null fields.

## Working examples from this repo
- DbContext anchor: `AspireChat/AspireChat.Api/Entities/AppDbContext.cs` defines `Users`, `Groups`, and `Chats` DbSets.
- Existing migration baseline: `AspireChat/AspireChat.Api/Entities/Migrations/20250510094552_InitialMigration.cs`.
- Model snapshot reference: `AspireChat/AspireChat.Api/Entities/Migrations/AppDbContextModelSnapshot.cs`.
- Typical workflow command:
  - `dotnet ef migrations add AddSomeFeature --project AspireChat/AspireChat.Api --startup-project AspireChat/AspireChat.Api`
