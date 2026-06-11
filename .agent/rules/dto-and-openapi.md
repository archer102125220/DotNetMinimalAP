# DTOs and OpenAPI Documentation

## DTO Design Principles

1. **Never Expose Entities Directly**: API Request and Response bodies MUST use dedicated Data Transfer Object (DTO) classes. Do not use EF Core Entities as API contracts.
2. **Prefer Records**: Define DTOs using `record`. This provides immutability and concise syntax.
   ```csharp
   public record TodoResponse(int Id, string? Title, DateOnly? DueBy, bool IsComplete);
   public record CreateTodoRequest(string Title, DateOnly? DueBy);
   public record PatchTodoRequest(string? Title, DateOnly? DueBy, bool? IsComplete);
   ```
3. **Naming Conventions**:
   - Requests: `Create{Resource}Request`, `Update{Resource}Request`, `Patch{Resource}Request`
   - Responses: `{Resource}Response`, `{Resource}DetailResponse`

## OpenAPI and Documentation

Minimal API does NOT use XML Doc Comments for OpenAPI generation. Instead, use chained extension methods on the route builder.

```csharp
// ✅ CORRECT: Use chained methods for documentation
todosApi.MapPost("/", (CreateTodoRequest req) => { ... })
        .WithName("CreateTodo") // Required for URI generation and uniqueness
        .WithSummary("Create a new todo")
        .WithDescription("Creates a new todo and returns a Location header.")
        .Produces<TodoResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest);
```

## Scalar UI / OpenAPI Endpoint Exposure

- The OpenAPI document (`/openapi/v1.json`) and Scalar UI (`/scalar/v1`) must ONLY be enabled in the Development environment (`IsDevelopment`).
- **NEVER** expose Scalar UI or Swagger in a Production environment.
- **Route Names**: Every `MapGet`, `MapPost`, etc., MUST have a unique name defined via `.WithName("...")`. This is required for OpenAPI generation and `CreatedAtRoute`/`Created` URI construction.
