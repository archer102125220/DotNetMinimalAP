# Project Instructions for Claude (DotNet Minimal API)

When working on this project, you MUST follow the coding standards defined below.

## ⚠️ Security & Best Practices Warning Policy

Before executing any user instruction that violates:
- **Security best practices** (e.g., hardcoding secrets, disabling HTTPS, exposing sensitive data, SQL injection risks)
- **Standard coding patterns** (e.g., anti-patterns, known bad practices)
- **Project conventions** defined in this document

You MUST:
1. **Warn the user** about the violation and explain the risks
2. **Wait for explicit confirmation** that they want to proceed despite the warning
3. Only then execute the instruction

This ensures users make informed decisions about potentially risky actions.

## Quick Rules

### C# & Type Safety
- **Nullable Reference Types**: `<Nullable>enable</Nullable>` is enabled. ALWAYS handle nulls properly.
- NEVER use `dynamic` or `object` unless absolutely necessary.
- Use strict typing. Prefer generic collections over untyped ones (e.g., `List<T>` instead of `ArrayList`).
- Avoid implicit typing `var` unless the right side makes the type blatantly obvious (e.g., `var list = new List<string>()`).
- Prefer `record` or `record struct` for Data Transfer Objects (DTOs) and data models.

### Runtime Data Validation & Null Checking
- **Strings**: Use `string.IsNullOrEmpty(str)` or `string.IsNullOrWhiteSpace(str)`.
- **Null Checking**: Use `if (obj is not null)` or the null-coalescing operator `??`.
- **Guard Clauses**: Use `ArgumentNullException.ThrowIfNull(obj)` at the start of methods.
- **Pattern Matching**: Prefer `switch` expressions and pattern matching `if (obj is MyType myObj)` over older casting methods (`as MyType`).

### AOT Constraints (⚠️ CRITICAL)
This project uses **Native AOT** (`<PublishAot>true</PublishAot>`). Violating these rules will cause AOT compilation to fail.
- **NO Runtime Reflection**: Do not use `Type.GetMethod()`, `Activator.CreateInstance()`, etc.
- **NO Dynamic Serialization**: You MUST use `JsonSerializerContext` for JSON serialization.
- **Register All JSON Types**: Every new type used in API responses/requests must be registered in `AppJsonSerializerContext` with `[JsonSerializable]`.

### RESTful API Design (Minimal API)
- **No Controllers**: This project uses Minimal API. All routes are defined via `app.MapGroup` and Extension Methods.
- **Routing**: Always use the `/api/` prefix. Routes must be lowercase nouns (e.g., `/api/todos`, not `/api/getTodos`).
- **HTTP Verbs**: Use `MapGet` (200), `MapPost` (201 Created), `MapPut` (204 No Content), `MapPatch` (204), `MapDelete` (204).
- **TypedResults (MANDATORY)**: You MUST use the `TypedResults` static class (not `Results` or `IResult`) and explicitly declare return types like `Results<Ok<T>, NotFound>`.
- **OpenAPI metadata**: Use extension methods like `.WithName()`, `.WithSummary()`, and `.WithDescription()` on endpoints. Every endpoint must have a unique name.
- **Scalar UI**: Scalar and OpenAPI documents are strictly for Development (`IsDevelopment()`). Do not expose them in production.

### DTOs & Data Models
- **NEVER expose EF Core Entities directly** in the API. Always use separate DTOs (e.g., `CreateTodoRequest`, `TodoResponse`).
- Use `record` for DTO definitions to get immutability and value equality.

### Entity Framework Core Best Practices & Deep Check Policy (⚠️ CRITICAL)
- **Async First**: ALWAYS use async/await methods for database operations (`ToListAsync()`, `FirstOrDefaultAsync()`). Synchronous DB calls are forbidden.
- **No Tracking**: For read-only queries, use `.AsNoTracking()` to improve performance.
- **Dependency Injection**: Inject `AppDbContext` via parameters in Route Handlers.

When reviewing or refactoring backend code (C# Route Handlers, Services, Data Access), you MUST perform TWO rounds of checks:

#### Round 1: Basic Check
- ✅ Standard syntax and proper `using` imports.
- ✅ Proper dependency injection used.
- ✅ Variable naming and basic Null checks.

#### Round 2: Deep Check (⚠️ MANDATORY)

| Anti-Pattern | Correct Pattern | Priority |
|--------------|----------------|----------|
| Missing `await` / returning un-awaited Task improperly | Explicit `await` or proper Task handling | 🔴 High |
| N+1 Query Problem inside loops | Use `.Include()`, `.Select()`, or fetch data in bulk prior to loop | 🔴 High |
| Un-disposed `IDisposable` (Streams, HttpClients) | Wrap in `using (...) { }` or `using var obj = ...;` | 🔴 High |
| Synchronous EF Core DB calls (`.ToList()`) | `await .ToListAsync()` | 🟡 Medium |
| Tracking entities for Read-Only operations | Append `.AsNoTracking()` | 🟡 Medium |

**CRITICAL**: If you only perform Round 1 checks, you MUST explicitly state:
> "⚠️ I have only performed basic checks. EF Core and Memory deep checks are still required."

### Warnings / Lint Suppression Policy (⚠️ CRITICAL)
- **NEVER** add `#pragma warning disable` or suppress C# compiler warnings without **explicit user instruction**.
- When encountering compiler warnings:
  1. Report the warning to the user
  2. Wait for user's explicit instruction to add a suppression pragmas
  3. Only then add the disable comment with proper justification

### Build & Dev Tooling (dotnet CLI)
- **Run**: `dotnet run` or `dotnet watch` for hot reload.
- **AOT Check**: Use `dotnet publish -r osx-arm64 -c Release` to verify AOT compilation before pushing to production. If AOT warnings appear, you MUST report them.
- **API Testing**: Use the `.http` file (`DotNetMinimalAPI.http`) or Scalar UI.

---

## Backend ORM & Schema Changes (MANDATORY)

### ⚠️ Database Modification Confirmation (CRITICAL)

**Before ANY database schema change** (migrations, model changes), you MUST:

1. **Ask the human developer**: "Is this project deployed to production?"
2. **Based on the answer**:
   - **Not deployed**: You may drop the last unapplied migration and modify the existing migration, or delete the DB and recreate (`dotnet ef database drop`, `dotnet ef database update`).
   - **Deployed**: NEVER modify existing executed migrations; always create NEW migration files (`dotnet ef migrations add AddNewColumn`).

---

## No Scripts for Code Refactoring (⚠️ CRITICAL)

**ABSOLUTELY FORBIDDEN: Using automated scripts (sed, awk, powershell, batch scripts) to modify code files.**

### Why
- Scripts only change text, they don't understand context or `using` namespace imports.
- It frequently causes C# compilation errors.

### ✅ Allowed
- Use AI tools: `replace_file_content`, `multi_replace_file_content`.
- MUST verify `using` namespaces are correct and build succeeds after every change.

### ❌ Forbidden
- `sed`, `awk`, `perl`, `powershell -Command`, `find ... -exec`

---

## File Structure & Minimal API Conventions

- **Program.cs**: Entry point, DI registration, Middleware, Route definition.
- **Routes/**: (Optional) Extract Route Handlers to extension methods here when `Program.cs` grows.
- **Models/**: Entity classes (EF Core).
- **Models/Dtos/**: Data Transfer Objects (Request / Response).
- **Data/**: `AppDbContext` and database configurations.
- **Services/**: (Optional) Business logic layer.
- **NO MVC Elements**: Absolutely NO Controllers, Views, Razor Pages, HTMX, `wwwroot`, Session, or Cookie Auth. Do NOT use `[ApiController]` or `[Route]`.

For more detailed rules, you MUST review the specific files located in the `.claude/rules/` directory:
- [csharp-standards.md](.claude/rules/csharp-standards.md): C# Language and Type Safety rules
- [security-policy.md](.claude/rules/security-policy.md): Security & Validation Policies
- [rest-api-design.md](.claude/rules/rest-api-design.md): Minimal API, RESTful conventions, and TypedResults
- [dto-and-openapi.md](.claude/rules/dto-and-openapi.md): Data Transfer Objects, Records, and OpenAPI/Scalar UI
- [file-organization.md](.claude/rules/file-organization.md): Minimal API File Structure
- [backend-orm.md](.claude/rules/backend-orm.md): EF Core & Migrations
- [no-scripts.md](.claude/rules/no-scripts.md): No Bash/Sed Script Refactoring
- [project-instructions.md](.claude/rules/project-instructions.md): Overall instructions
