# Refactor Code

Refactor existing C# code to improve quality, maintainability, and follow DotNet Minimal API project standards.

## Usage

Use this command when you need to:
- Extract route handlers from a bloated `Program.cs` into extension methods in `Routes/`.
- Apply C# Best Practices (Pattern matching, Records, TypedResults).
- Optimize Entity Framework Core queries.
- Ensure Native AOT compatibility.

## Template

Please refactor the following code:

**Target**: [Specify file, class, or method]

**Goals**:
- [ ] Extract routes into extension methods
- [ ] Convert DTOs to `record` types
- [ ] Upgrade responses to use `TypedResults`
- [ ] Apply C# Best Practices (proper Nullable handling)
- [ ] Optimize Entity Framework Core queries
- [ ] Add explicit OpenAPI `.WithSummary()` / `.Produces()` metadata

**Constraints**:
- ✅ Maintain existing functionality.
- ✅ Follow Minimal API project coding standards.
- ❌ Do NOT use Controllers or MVC patterns.
- ❌ Do NOT use automated bash scripts (`sed`, `awk`) for C# refactoring.
- ❌ Do NOT add `#pragma warning disable` without permission.

**Context**:
[Provide any additional context about the code]

## Example

```
Please refactor the following code:

**Target**: Program.cs (Users endpoints)

**Goals**:
- [x] Extract routing to `Routes/UserRoutes.cs`
- [x] Use `TypedResults`
- [x] Apply proper async/await patterns for EF Core
```

## Related Skills
- [No Scripts for Refactoring](../rules/no-scripts.md)
- [C# Standards](../rules/csharp-standards.md)
- [File Organization](../rules/file-organization.md)
