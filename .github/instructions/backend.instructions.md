---
applyTo: "**/*.cs"
---

# Backend & ORM Rules

## Entity Framework Core (EF Core) Best Practices

- **Async First**: ALWAYS use async/await methods for database operations (`ToListAsync()`, `FirstOrDefaultAsync()`). Synchronous DB calls are strictly forbidden.
- **No Tracking**: For read-only queries where you do not need to update the entities, always append `.AsNoTracking()` to improve performance.
- **Deep Check Policy**: AI must perform deep checks to catch N+1 queries in loops, missing `await` keywords, and unreleased `IDisposable` resources.

## Database Schema Modifications (⚠️ CRITICAL)

**Before ANY database schema change** (e.g., running migrations, modifying EF models), you MUST:

1. **Ask the human developer**: "Is this project deployed to production?"
2. **Wait for their answer** and act accordingly:
   - **Not deployed**: You may drop the last unapplied migration, modify the existing migration, or delete and recreate the DB.
   - **Deployed**: NEVER modify existing executed migrations. You must always create NEW migration files (`dotnet ef migrations add AddNewColumn`).

## Minimal API Conventions

- **No Controllers**: This project uses Minimal API. Do not create classes inheriting from `Controller` or `ControllerBase`. Do not use `[ApiController]` or `[Route]`.
- **Routing**: Use `app.MapGroup` for grouping and `MapGet`, `MapPost`, etc. for defining endpoints. Use small Extension Methods to split routes when `Program.cs` gets too large.
- **TypedResults**: ALWAYS use `TypedResults` (e.g., `TypedResults.Ok()`, `TypedResults.NotFound()`) instead of `Results`. Declare the explicit return type like `Results<Ok<T>, NotFound>`.
- **OpenAPI**: Chain methods such as `.WithName()`, `.WithSummary()`, and `.Produces<T>()` to explicitly configure the OpenAPI spec.
- **DTOs**: Never return EF Core entities directly. Always map to `record` DTOs.
- **AOT Serialization**: Every request/response DTO MUST be annotated in `AppJsonSerializerContext` with `[JsonSerializable(typeof(DTO))]`.
