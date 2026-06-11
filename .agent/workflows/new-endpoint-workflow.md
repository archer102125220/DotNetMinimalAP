# New Endpoint Implementation Workflow

This is the standard workflow to follow when implementing a new API endpoint in the .NET Minimal API project.

## Step 1: Data Transfer Objects (DTOs)
- [ ] Create Request DTO (`record` or `record struct`) if there is a request body.
- [ ] Create Response DTO (`record` or `record struct`) for the response payload.
- [ ] Ensure DTOs do not expose sensitive internal data and never use Entity classes directly.

## Step 2: AOT JSON Serialization Context
- [ ] **CRITICAL**: Add `[JsonSerializable(typeof(YourNewDto))]` to `AppJsonSerializerContext`.
- [ ] If returning a collection, add `[JsonSerializable(typeof(List<YourNewDto>))]` as well.
- [ ] Run `dotnet build` to ensure the source generator picks up the new types successfully.

## Step 3: Route Registration
- [ ] Locate the appropriate `RouteGroupBuilder` extension method (e.g., `MapTodoRoutes`).
- [ ] Map the endpoint using the correct HTTP verb (`MapGet`, `MapPost`, `MapPut`, etc.).
- [ ] Extract the handler logic into a separate static method if it is more than a few lines.

## Step 4: Endpoint Implementation & Metadata
- [ ] Implement the handler using `async` and inject services (e.g., `AppDbContext`) via method parameters.
- [ ] For database reads, ensure `.AsNoTracking()` is used.
- [ ] Return strongly-typed results using `TypedResults` (e.g., `TypedResults.Ok(dto)`).
- [ ] Chain `.WithName("UniqueEndpointName")` to the route mapping.
- [ ] Chain `.WithSummary("...")` and `.WithDescription("...")` for OpenAPI documentation.
- [ ] If returning different status codes, chain `.Produces<T>()` or `.ProducesProblem()` accordingly.

## Step 5: Verification
- [ ] Run `dotnet build`.
- [ ] Check `/scalar/v1` or `/openapi/v1.json` (in Development) to ensure the endpoint appears correctly with all schemas.
- [ ] Verify AOT compatibility by ensuring no reflection-related warnings appeared during build.
