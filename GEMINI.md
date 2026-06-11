# Project Instructions for Gemini (DotNet Minimal API)

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
- **Strict Typing**: NEVER use `dynamic` or `object` unless absolutely necessary. Prefer strongly typed generic collections (e.g., `List<T>`).
- **Implicit Typing**: Avoid `var` unless the right side makes the type blatantly obvious (e.g., `var list = new List<string>()`).
- **Record Preference**: Use `record` or `record struct` for DTOs and Data Models due to immutability and concise syntax.

### Runtime Data Validation & Null Checking
- **Strings**: Use `string.IsNullOrEmpty(str)` or `string.IsNullOrWhiteSpace(str)`.
- **Null Checking**: Use `if (obj is not null)` or the null-coalescing operator `??`.
- **Guard Clauses**: Use `ArgumentNullException.ThrowIfNull(obj)` at the start of methods.
- **Pattern Matching**: Prefer `switch` expressions and pattern matching `if (obj is MyType myObj)` over older casting methods (`as MyType`).

### Native AOT Constraints (⚠️ CRITICAL)
This project uses `<PublishAot>true</PublishAot>`. Violating these rules will cause AOT build failures:
- **No Runtime Reflection**: Do not use `Type.GetMethod()`, `Activator.CreateInstance()`, etc.
- **No Dynamic JSON Serialization**: You MUST NOT use `JsonSerializer.Serialize<T>()` without a source generator.
- **JSON Serialization Context**: Every request/response type MUST be registered in `AppJsonSerializerContext` with `[JsonSerializable(typeof(T))]`.
- **No `dynamic`**: Completely forbidden.

### RESTful API & Minimal API Routing
- **No Controllers**: All routes are defined via `app.Map*` and grouped using `MapGroup`.
- **Naming & Case**: All route paths must be lowercase. Use plural nouns for resources (e.g., `/api/todos`), NO verbs.
- **Unified Prefix**: API routes MUST start with `/api/`.

### TypedResults (⚠️ CRITICAL)
- **Strongly Typed Responses**: You MUST use the `TypedResults` static class instead of `Results` or `IResult`.
- **OpenAPI Type Inference**: Explicitly declare return types using union types (e.g., `Results<Ok<Todo>, NotFound>`) to allow OpenAPI to infer the schema.

### DTOs & Data Models
- **Never Expose Entities**: Do not use EF Core Entities as API Request/Response bodies. ALWAYS use DTOs (e.g., `TodoResponse`, `CreateTodoRequest`).
- **Record Types**: Use `record` for DTOs.
- **OpenAPI Metadata**: Use chained methods like `.WithName()`, `.WithSummary()`, and `.WithDescription()` instead of XML Doc Comments.

### OpenAPI & Scalar UI
- **Development Only**: OpenAPI endpoint and Scalar UI (`/scalar/v1`) are only enabled in Development. NEVER expose them in Production.
- **Unique Route Names**: Every route MUST have a unique name defined via `.WithName("...")` to properly generate OpenAPI specs and support `TypedResults.Created()`.

### Entity Framework Core (EF Core) Best Practices & Deep Check Policy (⚠️ CRITICAL)
- **Async First**: ALWAYS use async/await methods for database operations (`ToListAsync()`, `FirstOrDefaultAsync()`, `SaveChangesAsync()`). Synchronous DB calls are forbidden.
- **No Tracking**: For read-only queries, use `.AsNoTracking()` to improve performance.
- **Dependency Injection**: Always resolve `DbContext` via Route Handler parameters injection (e.g., `(AppDbContext db) => ...`).

When reviewing or refactoring backend code, you MUST perform TWO rounds of checks:

#### Round 1: Basic Check
- ✅ Standard syntax and proper `using` imports.
- ✅ Proper dependency injection.
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
- **AOT Publish Check**: Verify AOT compilation using `dotnet publish -r osx-arm64 -c Release` before production. Do not ignore AOT warnings.
- **EF Core CLI**: Use `dotnet ef` tools for migrations (e.g. `dotnet ef migrations add`, `dotnet ef database update`).
- **Environment**: Always check `appsettings.json` and `appsettings.Development.json` for proper configuration before running.

---

## Backend ORM & Schema Changes (MANDATORY)

### ⚠️ Database Modification Confirmation (CRITICAL)

**Before ANY database schema change** (migrations, model changes), you MUST:

1. **Ask the human developer**: "Is this project deployed to production?"
2. **Based on the answer**:
   - **Not deployed**: You may drop the last unapplied migration and modify the existing migration, or delete the DB and recreate (`dotnet ef database drop`, `dotnet ef database update`).
   - **Deployed**: NEVER modify existing executed migrations; always create NEW migration files (`dotnet ef migrations add AddNewColumn`).

### Migrations Workflow
- Use `dotnet ef migrations add <MigrationName>` to create a migration.
- Use `dotnet ef database update` to apply migrations.
- Always review the generated migration C# file before applying it to ensure EF Core scaffolded it correctly.

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

- **Program.cs**: Application entry point, DI registration, Middlewares, and simple routes.
- **Routes/**: Static classes containing route handlers as extension methods for `RouteGroupBuilder`.
- **Models/**: Entity classes (EF Core).
- **Models/Dtos/**: DTO classes (Request / Response).
- **Data/**: DbContext and database configurations.

**Forbidden Concepts in this Project**:
- NO `ControllerBase` or `Controller` classes.
- NO `[ApiController]`, `[Route]`, `[HttpGet]` MVC attributes.
- NO Views, Razor Pages, HTMX, wwwroot, static files, Session, or Cookie authentication.

For more detailed rules, you MUST review the specific files located in the `.agent/rules/` directory (if available in the project).
