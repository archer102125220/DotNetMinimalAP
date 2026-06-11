---
applyTo: "**/*.cs"
---

# C# Rules

## Core Rules & Type Safety

- **Nullable Reference Types**: `<Nullable>enable</Nullable>` is enabled. ALWAYS handle nulls properly.
- **Strict Typing**: NEVER use `dynamic` or `object`. Code must be strictly typed.
- **Collections**: Prefer generic collections (e.g., `List<T>`) over untyped ones.
- **Implicit Typing**: Avoid `var` unless the right side makes the type blatantly obvious (e.g., `var list = new List<string>()`).
- **Record Types**: Use `record` or `record struct` for DTOs and Data Models to leverage immutability and concise syntax.

## Runtime Data Validation & Null Checking

- **Strings**: Use `string.IsNullOrEmpty(str)` or `string.IsNullOrWhiteSpace(str)`.
- **Null Checking**: Use `if (obj is not null)` or the null-coalescing operator `??`.
- **Guard Clauses**: Use `ArgumentNullException.ThrowIfNull(obj)` at the start of methods.
- **Pattern Matching**: Prefer `switch` expressions and pattern matching `if (obj is MyType myObj)` over older casting methods (`as MyType`).

## Native AOT Constraints (⚠️ CRITICAL)

- **No Runtime Reflection**: Do not use `Type.GetMethod()`, `Activator.CreateInstance()`, or APIs relying on dynamic code generation.
- **JSON Serialization**: Use `JsonSerializerContext`. All serialized/deserialized types MUST be registered in `AppJsonSerializerContext`.

## Lint Disable Comments (⚠️ CRITICAL)

- **NEVER** add `#pragma warning disable` or suppress C# compiler warnings without **explicit user instruction**.
- When encountering compiler warnings or errors:
  1. Report the warning to the user.
  2. Wait for the user's explicit instruction to add a suppression pragma.
  3. Only then add the disable comment with proper justification.
