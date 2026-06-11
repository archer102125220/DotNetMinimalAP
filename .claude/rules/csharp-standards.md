# C# Language and Type Safety Rules

## 1. Nullable Reference Types
- The project runs with `<Nullable>enable</Nullable>`.
- **Rule**: Always handle nulls appropriately. Avoid using `!` (null-forgiving operator) unless you are absolutely certain.
- **Rule**: Use `ArgumentNullException.ThrowIfNull(param)` at the start of methods.

## 2. Strong Typing & Native AOT
- **Rule**: NEVER use `dynamic` or `object` types. Native AOT forbids runtime dynamic dispatch.
- **Rule**: When using `var`, it should only be used when the type is blatantly obvious from the right side.
- **Rule**: Prefer generic collections `List<T>` over untyped arrays or `ArrayList`.

## 3. DTOs and Immutable Types
- **Rule**: Always prefer `record` or `record struct` for Data Transfer Objects (DTOs) and Models when appropriate, as they provide value equality and immutability.

## 4. Pattern Matching
- **Rule**: Prefer C# pattern matching (e.g., `if (obj is MyType myObj)`) instead of casting (`var myObj = obj as MyType;`).
- **Rule**: Prefer `switch` expressions over `switch` statements for returning values.

## Examples

```csharp
// ❌ FORBIDDEN
dynamic data = GetData();
var myObj = obj as MyType;

// ✅ REQUIRED
MyDataClass data = GetData();
if (obj is MyType myObj) { ... }
public record CreateTodoRequest(string Title, DateOnly? DueBy);
```
