# ASP.NET Core Minimal API 路由與端點設計教學

這份教學筆記專門用來解釋在 .NET Minimal API 專案中，**前端的操作（網址）是如何精準對應到後端程式碼，以及後端又是如何決定要回傳什麼資料的。**

相較於傳統的 MVC (使用 Controller 與 Action)，Minimal API 提供了一種極致輕量、直覺且效能更高的寫法（特別是結合 AOT 編譯時）。

## 1. 路由註冊：網址如何找到程式碼？

在 Minimal API 中，我們沒有 Controller 類別。所有的路由與業務邏輯，都可以直接在 `Program.cs` 裡面宣告。

### 💡 基本的方法映射 (Method Mapping)
利用 `app.MapGet`, `app.MapPost`, `app.MapPut`, `app.MapDelete` 等擴充方法，我們能直接把一個網址跟一個 C# 函式 (Lambda 或區域函式) 綁定在一起。

```csharp
// 當收到 GET /hello 時，執行這個 Lambda 函式並回傳字串
app.MapGet("/hello", () => "Hello World!");
```

這就是 Minimal API 名字的由來：「用最少的程式碼，完成 API 的建立」。

---

## 2. 路由群組與管理 (Route Groups)

如果所有的 API 都直接掛在 `app` 下，`Program.cs` 會變得非常雜亂。因此實務上我們會使用 **Route Groups (路由群組)** 來管理相似的端點，這有點類似 Controller 的概念。

```csharp
// 建立一個基礎路由群組 /todos
var todosApi = app.MapGroup("/todos");

// 以下的路由都會自動加上 /todos 前綴
todosApi.MapGet("/", () => "回傳所有待辦事項");
todosApi.MapGet("/{id}", (int id) => $"回傳待辦事項 ID: {id}");
todosApi.MapPost("/", (Todo newTodo) => "建立新的待辦事項");
```

### 💡 為什麼要用 MapGroup？
1. **節省程式碼**：不用在每個 endpoint 前面重複寫 `/todos`。
2. **共用設定**：你可以對整個 Group 一起套用中介軟體 (Middleware) 或授權驗證 (Authorization)，例如 `todosApi.RequireAuthorization()`。
3. **OpenAPI (Swagger) 分類**：方便在產生 API 文件時，將相關的端點歸類在同一個標籤 (Tag) 下。

---

## 3. 依賴注入 (DI) 與參數綁定 (Model Binding)

在 Minimal API 的 Lambda 函式中，框架非常聰明，會自動幫我們判斷參數的來源，我們稱之為**模型綁定 (Model Binding)**。

```csharp
app.MapPost("/users/{id}", async (
    int id,                 // 1. 自動從 URL 路徑取得 (因為路由寫了 {id})
    [FromQuery] string q,   // 2. 自動從 Query String 取得 (?q=search)
    UserDto user,           // 3. 自動從 HTTP Body (JSON) 轉換為物件
    AppDbContext db         // 4. 自動從依賴注入 (DI) 容器中取得資料庫連線
) => 
{
    // ...
});
```

相較於 MVC Controller 需要把 DI 寫在建構子 (Constructor)，Minimal API 可以直接將需要的服務 (如 `AppDbContext`) 寫在參數上，這讓程式碼更精簡。

---

## 4. 回傳結果：TypedResults

在傳統 MVC，我們會 `return Ok()`, `return NotFound()`。在 Minimal API 中，雖然可以直接回傳物件讓框架自動轉成 JSON (例如 `return todo;`)，但為了更標準的 HTTP 狀態碼以及支援 **AOT (Ahead-of-Time) 編譯** 與 **OpenAPI 文件生成**，我們強烈建議使用 `TypedResults`。

```csharp
todosApi.MapGet("/{id}", Results<Ok<Todo>, NotFound> (int id) =>
{
    var todo = sampleTodos.FirstOrDefault(t => t.Id == id);
    
    if (todo is not null)
        return TypedResults.Ok(todo); // 回傳 HTTP 200 OK 與 JSON 資料
    else
        return TypedResults.NotFound(); // 回傳 HTTP 404 Not Found
});
```

### 💡 為什麼要宣告 `Results<Ok<Todo>, NotFound>`？
這是 .NET 7 之後引入的強型別回傳結果。
1. **明確性**：任何人一看方法簽章，就知道這支 API 只會回傳 `200 OK (並帶有 Todo 物件)` 或 `404 NotFound`。
2. **API 文件 (OpenAPI/Swagger)**：框架能自動解析這個宣告，幫你在 API 文件上畫出漂亮的回傳狀態碼說明，完全不需要額外寫 XML 註解。
3. **編譯期檢查**：如果你不小心 `return TypedResults.BadRequest()`，編譯器會立刻報錯，因為你沒有宣告它！

---

## 5. OpenAPI 與擴充資訊

Minimal API 內建了強大的 OpenAPI 整合。我們可以使用流暢介面 (Fluent API) 為端點加上說明，這會在 Swagger 或 Scalar UI 上完美呈現。

```csharp
todosApi.MapGet("/", () => sampleTodos)
        .WithName("GetTodos")          // 給這個端點一個唯一名稱 (可用於產生 URL)
        .WithSummary("取得所有代辦事項")   // OpenAPI 的簡短摘要
        .WithDescription("這是詳細說明..."); // OpenAPI 的詳細說明
```

## 總結流程

1. 應用程式啟動，執行 `app.Map...` 註冊所有路由表。
2. 使用者發出 `GET /todos/5` 請求。
3. Minimal API 路由找到對應的 `MapGet("/{id}")` 邏輯。
4. 框架自動解析 URL，把 `5` 轉成整數 `id`。
5. Lambda 執行完畢，回傳 `TypedResults.Ok(todo)`。
6. 框架將 `todo` 物件序列化為 JSON，並帶上 `200 OK` 狀態碼回傳給客戶端！
