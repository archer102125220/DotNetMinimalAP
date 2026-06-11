# TypedResults Usage Guidelines

This project MUST use the `TypedResults` static class instead of `Results` or `IResult` to ensure strongly typed responses. This is essential for OpenAPI documentation to automatically infer response types.

## ✅ Correct Approach

You must explicitly declare the return type (e.g., `Results<Ok<T>, NotFound>`) and return using `TypedResults`.

```csharp
// ✅ CORRECT: Explicit return type + TypedResults
todosApi.MapGet("/{id}", Results<Ok<Todo>, NotFound> (int id) =>
    sampleTodos.FirstOrDefault(t => t.Id == id) is { } todo
        ? TypedResults.Ok(todo)
        : TypedResults.NotFound())
    .WithName("GetTodoById")
    .WithSummary("Get a specific todo");
```

## 🚫 Incorrect Approaches

Do not use `IResult` or the untyped `Results` class.

```csharp
// ❌ INCORRECT: IResult loses type metadata for OpenAPI
todosApi.MapGet("/{id}", IResult (int id) => { ... });

// ❌ INCORRECT: Non-typed Results class
return Results.Ok(todo);  // MUST be TypedResults.Ok(todo)
```
