# DTOs and OpenAPI

## Data Transfer Objects (DTOs)
- **Separation of Concerns**: Request and Response bodies MUST use dedicated DTO classes.
- **NEVER Expose Entities**: Do not use EF Core Entities directly as API parameters or return types.
- **Record Types First**: Use `record` or `record struct` for DTOs.
  ```csharp
  public record TodoResponse(int Id, string Title);
  public record CreateTodoRequest(string Title);
  ```

## Native AOT Serialization
- Every DTO or Collection type returned by or accepted by an endpoint **MUST** be registered in the `AppJsonSerializerContext` to support Native AOT compilation.
  ```csharp
  [JsonSerializable(typeof(List<TodoResponse>))]
  [JsonSerializable(typeof(CreateTodoRequest))]
  internal partial class AppJsonSerializerContext : JsonSerializerContext { }
  ```

## OpenAPI Documentation
- **Fluent Methods**: Minimal API uses extension methods for OpenAPI docs, NOT XML comments.
  ```csharp
  app.MapGet("/", ...)
     .WithName("GetTodos")
     .WithSummary("Gets all todos")
     .Produces<List<TodoResponse>>(StatusCodes.Status200OK);
  ```
- **Environment**: OpenAPI endpoints (`/openapi/v1.json`) and Scalar UI MUST ONLY be enabled in the Development environment (`app.Environment.IsDevelopment()`). Never in Production.
