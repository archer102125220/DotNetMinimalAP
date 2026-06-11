# RESTful API Design (Minimal API Style)

## Route Definition
Do not use Controllers. All routes MUST be defined using `app.Map*` methods, leveraging `MapGroup` for grouping routes.

```csharp
// ✅ CORRECT: Use MapGroup and chained OpenAPI metadata methods
var todosApi = app.MapGroup("/api/todos");

todosApi.MapGet("/", () => TypedResults.Ok(list))
        .WithName("GetTodos")
        .WithSummary("Get all todos")
        .WithDescription("Returns a list of all existing todos.");
```

## Route Naming Conventions
- **Prefix**: All API routes MUST begin with `api/` via `MapGroup("/api/...")`.
- **Lowercase**: Route paths must be strictly lowercase.
- **Resource-Oriented (Nouns)**: Routes should be named after resources (nouns), not verbs. Example: `/api/users` instead of `/api/getUsers`.
- **Nesting**: Nested routes are allowed for relations (e.g., `GET /api/users/{userId}/orders`), but do not exceed two levels of nesting.

## HTTP Verbs and Methods
- `GET` -> `MapGet`: Retrieve resource(s). Returns `200 OK`.
- `POST` -> `MapPost`: Create new resource. Returns `201 Created`.
- `PUT` -> `MapPut`: Completely replace resource. Returns `204 No Content`.
- `PATCH` -> `MapPatch`: Partially update resource. Returns `204 No Content`.
- `DELETE` -> `MapDelete`: Delete resource. Returns `204 No Content`.

## HTTP Status Codes
- `200 OK`: Successful query with data.
- `201 Created`: Successful creation. MUST include the `Location` header via `TypedResults.Created(uri, payload)`.
- `204 No Content`: Successful operation with no return data.
- `400 Bad Request`: Client error or validation failure.
- `401 Unauthorized`: Authentication required.
- `403 Forbidden`: Authenticated but lacks permissions.
- `404 Not Found`: Target resource does not exist.
- `409 Conflict`: Resource conflict (e.g., duplicate).
- `500 Internal Server Error`: Server error. NEVER expose raw exception messages to the client.
