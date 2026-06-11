# RESTful API Design (Minimal API)

## Route Naming and Grouping
- **Prefix**: All API routes MUST start with `api/`, mapped using `MapGroup("/api/...")`.
- **Nouns**: Routes should represent resources using nouns (e.g., `/api/users`), NOT verbs.
- **Casing**: Use lowercase paths.
- **No Controllers**: Route definition should be handled in `Program.cs` or extension methods mapping on `RouteGroupBuilder`.

## HTTP Verbs and Handlers
- `GET` (`MapGet`): Retrieve resource(s). Returns `200 OK`.
- `POST` (`MapPost`): Create a new resource. Returns `201 Created` with Location header.
- `PUT` (`MapPut`): Completely replace resource. Returns `204 No Content`.
- `PATCH` (`MapPatch`): Partially update resource. Returns `204 No Content`.
- `DELETE` (`MapDelete`): Remove a resource. Returns `204 No Content`.

## TypedResults Constraint
You **MUST** use `TypedResults` instead of `Results` or `IResult`. This is critical for generating accurate OpenAPI documentation and preserving type safety.

```csharp
// ❌ FORBIDDEN (loses type info, bad for OpenAPI)
app.MapGet("/{id}", IResult (int id) => Results.Ok(data));

// ✅ REQUIRED
app.MapGet("/{id}", Results<Ok<Todo>, NotFound> (int id) =>
    todo is not null ? TypedResults.Ok(todo) : TypedResults.NotFound());
```

## HTTP Status Codes
- `200 OK`: Successful query with data.
- `201 Created`: Successful creation. Must use `TypedResults.Created(uri, payload)`.
- `204 No Content`: Successful operation with no return data.
- `400 Bad Request`: Client error.
- `401 Unauthorized` / `403 Forbidden`: Auth errors.
- `404 Not Found`: Resource does not exist.
- `500 Internal Server Error`: Do not expose raw exception messages to the client.
