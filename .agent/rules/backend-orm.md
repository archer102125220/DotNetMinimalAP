# Entity Framework Core (EF Core) Best Practices

When implementing database operations, prioritize EF Core patterns and ensure extreme performance and safety.

## 1. Async First
- ALL database operations MUST be asynchronous (e.g., `ToListAsync()`, `FirstOrDefaultAsync()`).
- Synchronous calls (`.ToList()`) are strictly FORBIDDEN.

## 2. No-Tracking for Read-Only Data
- If you are only querying data and not modifying it, you MUST append `.AsNoTracking()` to improve performance.

## 3. Dependency Injection (DI)
- Always inject the `AppDbContext` via parameters in Route Handlers. Do not instantiate it directly.
  ```csharp
  // ✅ Correct DbContext injection in Minimal API
  todosApi.MapGet("/", async (AppDbContext db) =>
      await db.Todos.AsNoTracking().ToListAsync());
  ```

## 4. EF Core Deep Check Policy [MANDATORY]
When reviewing or refactoring backend code, the AI must perform a two-round check:
1. **First Round (Surface)**: Syntax, using imports, DI correctly, naming, null checks.
2. **Second Round (Deep) [REQUIRED]**:
   - 🔴 Missing `await` or mishandled `Task`.
   - 🔴 **N+1 Query Problems** in loops. (Must use `.Include()`, `.Select()`, or batch fetches beforehand).
   - 🔴 Unreleased `IDisposable` resources (e.g., Streams, HttpClients). Must use `using (...)` or `using var`.
   - 🟡 Synchronous EF Core calls.
   - 🟡 Missing `.AsNoTracking()` on read-only queries.

*(Note: If only the first round is performed, the AI MUST explicitly declare: "⚠️ I have only performed basic checks. EF Core and Memory deep checks are still required.")*

## 5. AOT Compatibility Warning
EF Core's support for Native AOT is continually evolving. If you encounter Reflection-related warnings after enabling AOT, you MUST inform the developer. AOT might need to be disabled for certain functions or alternative approaches considered.
