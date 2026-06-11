# C# Language and Type Safety Rules

## 1. Nullable Reference Types
- The project runs with `<Nullable>enable</Nullable>`.
- **Rule**: Properly handle all possible null cases. Use `string.IsNullOrEmpty(str)` or `string.IsNullOrWhiteSpace(str)` for string checks. Use `if (obj is not null)` or the null-coalescing operator (`??`) for objects.
- **Rule**: Use `ArgumentNullException.ThrowIfNull(param)` at the start of methods as a guard clause.

## 2. Strict Typing
- **Rule**: NEVER use `dynamic` or `object` unless absolutely necessary.
- **Rule**: Prefer strongly typed generic collections like `List<T>` over `ArrayList`.
- **Rule**: Avoid using `var` unless the right side of the assignment makes the type blatantly obvious (e.g., `var list = new List<string>();`).

## 3. Pattern Matching
- **Rule**: Prefer C# 8+ pattern matching for casting and type checking (e.g., `switch` expressions and `if (obj is MyType myObj)`) instead of the older `as` syntax.

## 4. DTO Preference
- **Rule**: Use `record` or `record struct` when defining Data Transfer Objects (DTOs) or data models. They provide immutability and concise syntax, which fits the Minimal API style perfectly.
