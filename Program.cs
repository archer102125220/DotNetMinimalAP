using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.HttpResults;
using Scalar.AspNetCore;

var builder = WebApplication.CreateSlimBuilder(args);

// 配置 JSON 序列化（為了支援 AOT 編譯）
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

// 加入 OpenAPI 文件產生服務
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // 開啟 OpenAPI 路由
    app.MapOpenApi();
    
    // 開啟 Scalar UI 來呈現 API 文件 (可以取代 Swagger)
    // 啟動後在瀏覽器訪問 /scalar 即可看到漂亮的 API 測試畫面
    app.MapScalarApiReference();
}

// 建立一個在記憶體中的 List 當作暫時的資料庫
var sampleTodos = new List<Todo>
{
    new(1, "Walk the dog"),
    new(2, "Do the dishes", DateOnly.FromDateTime(DateTime.Now)),
    new(3, "Do the laundry", DateOnly.FromDateTime(DateTime.Now.AddDays(1))),
    new(4, "Clean the bathroom"),
    new(5, "Clean the car", DateOnly.FromDateTime(DateTime.Now.AddDays(2)))
};

var todosApi = app.MapGroup("/todos");

// 1. GET: 取得所有 Todo
todosApi.MapGet("/", () => sampleTodos)
        .WithName("GetTodos")
        .WithSummary("取得所有代辦事項");

// 2. GET: 根據 ID 取得單一 Todo
todosApi.MapGet("/{id}", Results<Ok<Todo>, NotFound> (int id) =>
    sampleTodos.FirstOrDefault(a => a.Id == id) is { } todo
        ? TypedResults.Ok(todo)
        : TypedResults.NotFound())
    .WithName("GetTodoById")
    .WithSummary("取得特定代辦事項");

// 3. POST: 建立新的 Todo
todosApi.MapPost("/", (Todo todo) =>
{
    // 自動產生新的 ID
    var newId = sampleTodos.Count > 0 ? sampleTodos.Max(t => t.Id) + 1 : 1;
    var newTodo = todo with { Id = newId }; // 替換掉傳入的 ID
    
    sampleTodos.Add(newTodo);
    return TypedResults.Created($"/todos/{newId}", newTodo);
})
.WithName("CreateTodo")
.WithSummary("新增代辦事項");

// 4. PUT: 完整更新特定 ID 的 Todo
todosApi.MapPut("/{id}", Results<NoContent, NotFound> (int id, Todo updatedTodo) =>
{
    var index = sampleTodos.FindIndex(t => t.Id == id);
    if (index == -1) return TypedResults.NotFound();

    // 替換為新的資料，但保留原本的 ID
    sampleTodos[index] = updatedTodo with { Id = id }; 
    return TypedResults.NoContent();
})
.WithName("UpdateTodo")
.WithSummary("完整更新代辦事項 (PUT)");

// 5. PATCH: 部分更新特定 ID 的 Todo
todosApi.MapPatch("/{id}", Results<NoContent, NotFound> (int id, TodoPatch patchTodo) =>
{
    var index = sampleTodos.FindIndex(t => t.Id == id);
    if (index == -1) return TypedResults.NotFound();

    var existing = sampleTodos[index];
    
    // 如果 patchTodo 中有值，就更新；否則保留舊值
    var updated = existing with 
    { 
        Title = patchTodo.Title ?? existing.Title,
        DueBy = patchTodo.DueBy ?? existing.DueBy,
        IsComplete = patchTodo.IsComplete ?? existing.IsComplete
    };

    sampleTodos[index] = updated;
    return TypedResults.NoContent();
})
.WithName("PatchTodo")
.WithSummary("部分更新代辦事項 (PATCH)");

// 6. DELETE: 刪除特定 ID 的 Todo
todosApi.MapDelete("/{id}", Results<NoContent, NotFound> (int id) =>
{
    var removedCount = sampleTodos.RemoveAll(t => t.Id == id);
    return removedCount > 0 ? TypedResults.NoContent() : TypedResults.NotFound();
})
.WithName("DeleteTodo")
.WithSummary("刪除代辦事項");

app.Run();

// 定義資料模型 (Models)
public record Todo(int Id, string? Title, DateOnly? DueBy = null, bool IsComplete = false);

// 定義給 PATCH 使用的資料模型 (所有欄位都是 Optional)
public record TodoPatch(string? Title, DateOnly? DueBy, bool? IsComplete);

// 設定 Json 序列化器支援的型別
[JsonSerializable(typeof(List<Todo>))]
[JsonSerializable(typeof(Todo))]
[JsonSerializable(typeof(TodoPatch))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}
