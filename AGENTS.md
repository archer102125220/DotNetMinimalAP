# Project Instructions for AI Agents (DotNet Minimal API)

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
- **Strict Typing**: NEVER use `dynamic` or `object` unless absolutely necessary. Prefer generic collections over untyped ones (e.g., `List<T>` instead of `ArrayList`).
- **Implicit Typing**: Avoid `var` unless the right side makes the type blatantly obvious (e.g., `var list = new List<string>()`).
- **Record Types Priority**: When defining DTOs or data models, prefer `record` or `record struct` for immutability and concise syntax, which fits the Minimal API style perfectly.

### Runtime Data Validation & Null Checking
- **Strings**: Use `string.IsNullOrEmpty(str)` or `string.IsNullOrWhiteSpace(str)`.
- **Null Checking**: Use `if (obj is not null)` or the null-coalescing operator `??`.
- **Guard Clauses**: Use `ArgumentNullException.ThrowIfNull(obj)` at the start of methods.
- **Pattern Matching**: Prefer `switch` expressions and pattern matching `if (obj is MyType myObj)` over older casting methods (`as MyType`).

### Native AOT Constraints (⚠️ CRITICAL)
This project enables Native AOT (`<PublishAot>true</PublishAot>`). **Violating these constraints will cause AOT build failures.**
- **NO Runtime Reflection**: Forbidden to use `Type.GetMethod()`, `Activator.CreateInstance()`, etc.
- **NO Dynamic Serialization**: `JsonSerializer.Serialize<T>()` without source generators is forbidden.
- **NO `dynamic` keyword**: Completely forbidden.
- **JSON Serialization**: Every type used for API request/response MUST be registered in `AppJsonSerializerContext` using `[JsonSerializable(typeof(YourModel))]`.

### RESTful API Design & Routing
- **NO Controllers**: All routes are defined via `app.Map*` methods and grouped using `MapGroup`.
- **Uniform Prefix**: All API routes MUST start with `api/` via `MapGroup("/api/...")`.
- **Lowercase Naming**: Route paths MUST be lowercase.
- **Resource-Oriented**: Routes should use **nouns** (resource names), NOT verbs (e.g., `/api/users`, NOT `/api/getUsers`).
- **HTTP Verbs**: Use `MapGet` (200 OK), `MapPost` (201 Created), `MapPut` (204 No Content), `MapPatch` (204 No Content), `MapDelete` (204 No Content).

### TypedResults Usage (MANDATORY)
- You MUST use the `TypedResults` static class instead of `Results` to provide strong-typed responses for OpenAPI metadata inference.
- **Correct Example**: `return TypedResults.Ok(data);`
- Specify return types in the lambda signature, e.g., `Results<Ok<Todo>, NotFound> (int id) => ...`

### DTOs & Data Models
- **NEVER Expose Entities Directly**: Request and Response bodies MUST use dedicated DTO classes (preferably `record`). Never return EF Core Entities directly.
- **Naming Conventions**: `Create{Resource}Request`, `Update{Resource}Request`, `Patch{Resource}Request`, `{Resource}Response`, `{Resource}DetailResponse`.

### OpenAPI & Scalar UI
- OpenAPI metadata MUST be generated using chained methods like `.WithName()`, `.WithSummary()`, and `.WithDescription()`.
- Every `MapGet`/`MapPost` MUST have a unique `.WithName("...")` to correctly generate OpenAPI docs and `Location` headers.
- OpenAPI and Scalar UI are enabled ONLY in the Development environment (`IsDevelopment`). NEVER expose them in Production.

## EF Core & Database Operations

- **Async First**: ALWAYS use async/await for database operations (e.g., `ToListAsync()`). Synchronous calls (`.ToList()`) are FORBIDDEN.
- **No Tracking**: Read-only queries MUST append `.AsNoTracking()`.
- **Dependency Injection**: Inject `AppDbContext` via route handler parameters, not globally.
- **AOT Compatibility**: Be aware of EF Core AOT warnings and report them to the user.

### EF Core 深度檢查政策 (Deep Check Policy) (⚠️ CRITICAL)

When reviewing or refactoring backend code, you MUST perform TWO rounds of checks:

#### Round 1: Basic Check (表面檢查)
- ✅ Standard syntax and proper `using` imports.
- ✅ Proper dependency injection.
- ✅ Variable naming and basic Null checks.

#### Round 2: Deep Check (深度檢查) - ⚠️ MANDATORY
You MUST check for these common mistakes:

| Anti-Pattern | Correct Pattern | Priority |
|--------------|----------------|----------|
| Missing `await` / returning un-awaited Task improperly | Explicit `await` or proper Task handling | 🔴 High |
| N+1 Query Problem inside loops | Use `.Include()`, `.Select()`, or fetch data in bulk prior to loop | 🔴 High |
| Un-disposed `IDisposable` (Streams, HttpClients) | Wrap in `using (...) { }` or `using var obj = ...;` | 🔴 High |
| Synchronous EF Core DB calls (`.ToList()`) | `await .ToListAsync()` | 🟡 Medium |
| Tracking entities for Read-Only operations | Append `.AsNoTracking()` | 🟡 Medium |

**CRITICAL**: If you only perform Round 1 checks, you MUST explicitly state:
> "⚠️ I have only performed basic checks. EF Core and Memory deep checks are still required."

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

---

## Project Architecture Conventions

This project uses the **Minimal API** extreme lean architecture, which is entirely different from traditional MVC.

- **Program.cs**: Application entry point (DI registration, Middleware, Route definitions).
- **Route Handlers**: Use Extension Methods on `RouteGroupBuilder` to extract routes from `Program.cs` when it grows too large (e.g., `public static RouteGroupBuilder MapTodoRoutes(this RouteGroupBuilder group)`).
- **FORBIDDEN**: 
  - NO classes inheriting from `ControllerBase` or `Controller`.
  - NO MVC attributes like `[ApiController]`, `[Route]`, `[HttpGet]`.
  - NO Views, Razor Pages, HTMX, wwwroot, static files, session, or cookie authentication.

---

## Build & Dev Tooling

- **Run**: `dotnet run` or `dotnet watch` for hot reload.
- **AOT Publish Check**: `dotnet publish -r osx-arm64 -c Release` (or target OS) to verify AOT compilation.
- **Warnings / Lint Suppression Policy (⚠️ CRITICAL)**: NEVER add `#pragma warning disable` without explicit user instruction. Report warnings, wait for permission, and add proper justification.

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
