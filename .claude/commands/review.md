# Code Review

Perform comprehensive C# and ASP.NET Core Minimal API code review following project standards.

## Usage

Use this command when you need to:
- Review C# pull requests
- Check code quality & performance
- Ensure Native AOT compatibility
- Ensure coding standards compliance (Minimal API patterns)

## Template

Please review the following code:

**Target**: [Specify file(s), PR, or commit]

**Review Focus**:
- [ ] Code quality and readability
- [ ] C# Strong Typing (avoid `dynamic`, proper `var` usage)
- [ ] Nullable Reference Types handling
- [ ] Performance (EF Core `.AsNoTracking()`, Async/Await)
- [ ] Native AOT Compliance (No reflection, JSON serialization contexts)
- [ ] Minimal API Compliance (TypedResults, Extension Methods for routes)
- [ ] Security vulnerabilities
- [ ] Exception handling (no empty catches)

**Project Standards Checklist**:
- [ ] EF Core: Async queries (`ToListAsync`), `.AsNoTracking()` for read-only.
- [ ] C#: Explicit types where `var` is ambiguous. `record` types used for DTOs.
- [ ] AOT: Types registered in `AppJsonSerializerContext`.
- [ ] Routing: Handled via `MapGroup` and `MapGet`/`MapPost`, no Controllers.
- [ ] Return Types: Using `TypedResults` statically.
- [ ] Lint: No `#pragma warning disable` without justification.

**Output Format**:
- List issues by severity (Critical, High, Medium, Low)
- Provide specific line numbers
- Suggest concrete C# improvements
- Include code examples for fixes

## Example

```
Please review the following code:

**Target**: Routes/TodoRoutes.cs

**Review Focus**:
- [x] EF Core Performance (Deep Check)
- [x] AOT compatibility
- [x] TypedResults usage
```

## Review Output Format

```markdown
## Code Review Summary

### Critical Issues (Must Fix)
1. **[Line X]** Issue description
   - **Problem**: Sync DB call `ToList()` blocking threads.
   - **Fix**: Use `await ...ToListAsync()`.

### High Priority
2. **[Line Y]** Issue description
   - **Problem**: Missing `[JsonSerializable]` attribute for DTO.
   - **Fix**: Register `TodoResponse` in `AppJsonSerializerContext` to avoid AOT failure.

### Medium Priority
3. **[Line Z]** Issue description
   - **Suggestion**: Use `TypedResults.Ok()` instead of `Results.Ok()`.

### Positive Observations
- Route correctly extracted using extension method.
- Proper use of `record` for DTO.
```

## Related Skills
- [Security Policy](../rules/security-policy.md)
- [C# Standards](../rules/csharp-standards.md)
- [Backend ORM](../rules/backend-orm.md)
- [REST API Design](../rules/rest-api-design.md)
