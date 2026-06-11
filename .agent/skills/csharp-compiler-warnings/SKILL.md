---
name: csharp-compiler-warnings
description: Workflow for handling C# compiler warnings and suppression policy.
---

# csharp-compiler-warnings

When encountering a C# compiler warning during build (`dotnet run`, `dotnet build`), follow this workflow:

## 1. Do Not Ignore
Compiler warnings (especially nullable warnings `CS8600`, `CS8602` etc.) are critical in a project with `<Nullable>enable</Nullable>`. Never ignore them.

## 2. Fix the Root Cause
Always attempt to fix the root cause of the warning first by:
- Adding null checks (`if (obj != null)`).
- Using null-coalescing operators (`??`).
- Correcting types or adding `?` where a null is expected.

## 3. Suppression Policy (Warning)
- You are strictly FORBIDDEN to use `#pragma warning disable` without explicit permission from the human developer.
- If a warning cannot be reasonably fixed and must be suppressed:
  1. Ask the user for permission to suppress it.
  2. Explain WHY it needs to be suppressed.
  3. Once approved, add the suppression along with a comment explaining the justification.
