# Project Structure & Routing Architecture

This project uses the **Minimal API Architecture**, which differs entirely from traditional MVC.

## Recommended File Structure

```text
DotNetMinimalAPI/
├── Program.cs              # Application entry point: DI registration, Middlewares, Route definitions
├── Routes/                 # (Optional) Route Handlers grouped by resource
│   ├── TodoRoutes.cs       # Static class using Extension Methods for routing
│   └── UserRoutes.cs
├── Models/                 # EF Core Entity classes
│   └── Todo.cs
├── Models/Dtos/            # DTO classes (Requests / Responses)
│   ├── TodoResponse.cs
│   └── CreateTodoRequest.cs
├── Data/                   # DbContext and DB configurations
│   └── AppDbContext.cs
├── Services/               # (Optional) Business logic layer
│   └── TodoService.cs
└── note/                   # Developer notes
```

## Route Handler Extraction (Extension Method Pattern)

To keep `Program.cs` clean, extract routes into extension methods on `RouteGroupBuilder`:

```csharp
// Routes/TodoRoutes.cs
public static class TodoRoutes
{
    public static RouteGroupBuilder MapTodoRoutes(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetAllTodos).WithName("GetTodos").WithSummary("Get all todos");
        group.MapPost("/", CreateTodo).WithName("CreateTodo").WithSummary("Create a todo");
        return group;
    }

    private static async Task<Ok<List<TodoResponse>>> GetAllTodos(AppDbContext db) =>
        TypedResults.Ok(await db.Todos.AsNoTracking().Select(t => new TodoResponse(t.Id, t.Title)).ToListAsync());
}

// In Program.cs
var todosApi = app.MapGroup("/api/todos");
todosApi.MapTodoRoutes();
```

## 🛑 Prohibited Actions
- **NEVER** create classes inheriting from `ControllerBase` or `Controller`.
- **NEVER** use MVC specific attributes like `[ApiController]`, `[Route]`, `[HttpGet]`.
- This project **does not use** Views, Razor Pages, HTMX, `wwwroot`, static files, Session, or Cookie-based authentication.
