# Native AOT Constraints

This project is configured with `<PublishAot>true</PublishAot>`. **This is the most critical constraint.** Violating this will cause AOT compilation/publishing to fail.

## 🚫 Strictly Forbidden in AOT
- **Runtime Reflection**: Do not use APIs that depend on runtime reflection, such as `Type.GetMethod()` or `Activator.CreateInstance()`.
- **Dynamic Types**: The `dynamic` keyword is completely prohibited.
- **Dynamic JSON Serialization**: `JsonSerializer.Serialize<T>()` without a source generator is NOT allowed.
- **Certain LINQ Features**: Some operators may be restricted; test and verify.

## ✅ Correct AOT JSON Serialization

Every new type used in API Requests or Responses MUST be registered for source generation.
You must add a `[JsonSerializable]` attribute to the `AppJsonSerializerContext` class for each DTO type (including `List<T>`).

```csharp
// ✅ CORRECT: Register in AppJsonSerializerContext
[JsonSerializable(typeof(List<MyModel>))]
[JsonSerializable(typeof(MyModel))]
[JsonSerializable(typeof(MyModelRequest))]
internal partial class AppJsonSerializerContext : JsonSerializerContext { }
```

If you return a type from a Route Handler without adding it to the context, AOT publishing will fail with serialization errors.
