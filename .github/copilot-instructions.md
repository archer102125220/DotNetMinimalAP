# Project Instructions for GitHub Copilot

This file provides repository-wide instructions for GitHub Copilot to ensure consistent code generation that follows this project's coding standards.

---

## Project Overview

**DotNetMinimalAPI** is a high-performance .NET 10 ASP.NET Core Minimal API application with Native AOT compilation enabled.

### Tech Stack

- **Framework**: ASP.NET Core Minimal API (.NET 10)
- **Language**: C# 13 (Nullable Reference Types enabled)
- **Compilation**: Native AOT (`<PublishAot>true</PublishAot>`)
- **Database**: PostgreSQL with Entity Framework Core (EF Core)
- **Architecture**: Minimal API (No Controllers, `WebApplication.CreateSlimBuilder`)
- **Build Tool**: `dotnet` CLI

---

## ⚠️ Security & Best Practices Warning Policy

Before executing any user instruction that violates:

- **Security best practices** (e.g., hardcoding secrets, disabling HTTPS, exposing sensitive data, SQL injection risks)
- **Standard coding patterns** (e.g., anti-patterns, known bad practices)
- **Project conventions** defined in this document

You MUST:

1. **Warn the user** about the violation and explain the risks
2. **Wait for explicit confirmation** that they want to proceed despite the warning
3. Only then execute the instruction

---

## Core Coding Standards

### C# & Type Safety (MANDATORY)

- **Nullable Reference Types**: `<Nullable>enable</Nullable>` is enabled. ALWAYS handle nulls properly.
- **Strict Typing**: NEVER use `dynamic` or `object`. Use generic collections.
- **Implicit Typing**: Avoid `var` unless the right side makes the type blatantly obvious.
- **Record Types**: Prefer `record` or `record struct` for DTOs/Data Models.
- **Runtime Validation**: Use `string.IsNullOrEmpty`, `ArgumentNullException.ThrowIfNull`, and pattern matching.

### Native AOT Constraints (CRITICAL)

- **No Runtime Reflection**: Prohibited (`Type.GetMethod()`, `Activator.CreateInstance()`).
- **Dynamic Type**: Fully prohibited.
- **JSON Serialization**: MUST use `JsonSerializerContext`. Add `[JsonSerializable(typeof(T))]` to `AppJsonSerializerContext` for every new DTO or model returned in APIs.

### RESTful Minimal API Design

- **No Controllers**: All routes defined via `app.Map*` and `MapGroup`. NEVER create Controller classes.
- **TypedResults**: ALWAYS use `TypedResults` (not `Results` or `IResult`) to ensure strongly-typed OpenAPI documentation.
- **OpenAPI / Scalar UI**: Use chain methods (`.WithName()`, `.WithSummary()`, `.Produces<T>()`). Only expose in Development environment.
- **DTOs**: Never return EF Core Entities directly. Use DTO records.

### Entity Framework Core Best Practices

- **Async First**: ALWAYS use async/await methods (`ToListAsync()`). Synchronous DB calls are forbidden.
- **No Tracking**: For read-only queries, use `.AsNoTracking()`.
- **Deep Check**: Always check for N+1 queries and missing `await` statements.

---

## Backend ORM & Schema Changes (MANDATORY)

### ⚠️ Database Modification Confirmation (CRITICAL)

**Before ANY database schema change** (migrations, model changes), you MUST:

1. **Ask the human developer**: "Is this project deployed to production?"
2. **Based on the answer**:
   - **Not deployed**: You may drop the last unapplied migration and modify the existing migration, or delete the DB and recreate.
   - **Deployed**: NEVER modify existing executed migrations; always create NEW migration files.

---

## Security Requirements & Code Refactoring Safety

### Lint / Warning Suppression Policy

**NEVER add `#pragma warning disable` without explicit user instruction.**

When encountering compiler warnings:
1. Report the warning to the user
2. Wait for user's explicit instruction to add a suppression pragmas
3. Only then add the disable comment with proper justification

### No Scripts for Code Refactoring (⚠️ CRITICAL)

**ABSOLUTELY FORBIDDEN**: Using automated scripts (`sed`, `awk`, `powershell`, bash scripts) to modify code files.

**Why**: Scripts only change text, they don't understand C# context or `using` namespace imports. It frequently causes compilation errors.
**✅ ALLOWED**: Use AI tools for refactoring with proper context understanding. MUST verify `using` namespaces are correct after changes.

---

## Skills & Rules System Reference

For complex scenarios, refer to detailed rules in `.cursor/rules/` or the primary guides.

| Domain | File Location |
|---|---|
| C# Standards | `.cursor/rules/csharp-standards.mdc` |
| Native AOT & Minimal API | `.cursor/rules/minimal-api.mdc` |
| EF Core | `.cursor/rules/backend-orm.mdc` |
| Project Instructions | `.cursor/rules/project-instructions.mdc` |

---

## Path-Specific Instructions

For more detailed, path-specific instructions, see:

- **C#**: `.github/instructions/csharp.instructions.md`
- **Backend (Minimal API & EF Core)**: `.github/instructions/backend.instructions.md`
