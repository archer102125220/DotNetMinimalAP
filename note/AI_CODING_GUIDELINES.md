# 🤖 DotNet Minimal API - AI 協作開發規範指南 (AI Coding Guidelines)

本文件綜合了專案中的各個 AI 設定檔，為開發過程中的 AI 助手 (如 Copilot, Claude, Gemini) 提供統一、詳盡的中文規範指引。

> **專案性質**：本專案為 **.NET 10 ASP.NET Core Minimal API** 專案。與傳統 MVC 架構**完全不同**：
> - **沒有 Controller 類別**，所有路由與邏輯直接定義在 `Program.cs`。
> - **啟用 Native AOT 編譯** (`<PublishAot>true</PublishAot>`)，嚴禁執行期 Reflection。
> - **使用 `WebApplication.CreateSlimBuilder`**，刪減了許多不必要的功能，啟動更快。
> - 所有輸出均為 JSON 格式，透過 HTTP 端點提供服務。

---

## ⚠️ 1. 安全性與最佳實踐警告原則 (Security & Best Practices)

在執行任何可能違反以下情況的使用者指令前，AI 必須嚴格遵守「警告並確認」機制：
* **安全最佳實踐**：如硬編碼機密資訊 (hardcoding secrets)、停用 HTTPS、暴露敏感資料、SQL 注入風險等。
* **標準程式碼模式**：如反模式 (anti-patterns)、已知的不良實踐。
* **本文件定義之專案慣例**。

**強制處理流程**：
1. **警告使用者**：指出違規情況並解釋潛在風險。
2. **等待明確確認**：必須得到使用者的明確同意。
3. **執行指令**：確認後才可執行該指令。

---

## 🚀 2. C# 與型別安全 (C# & Type Safety)

* **可 Null 參考型別 (Nullable Reference Types)**：專案已啟用 `<Nullable>enable</Nullable>`。**必須**妥善處理所有可能的 null 情況。
* **嚴格型別 (Strict Typing)**：絕對**禁止**使用 `dynamic` 或 `object`，除非在絕對必要的情況下。應優先使用強型別的泛型集合 (如 `List<T>`) 而非 `ArrayList`。
* **隱式型別 (Implicit Typing)**：避免使用 `var`，除非等號右側的型別已非常明顯 (例如：`var list = new List<string>();`)。
* **Record 型別優先**：定義 DTO / 資料模型時，優先使用 `record` 或 `record struct`，因其具備不可變性且語法簡潔，與 Minimal API 風格完美契合。

### 🛡️ 執行期資料驗證與 Null 檢查 (Runtime Validation)
* **字串檢查**：使用 `string.IsNullOrEmpty(str)` 或 `string.IsNullOrWhiteSpace(str)`。
* **Null 檢查**：使用 `if (obj is not null)` 或 Null 聯合運算子 (`??`)。
* **防護子句 (Guard Clauses)**：在方法開頭使用 `ArgumentNullException.ThrowIfNull(obj)`。
* **模式匹配 (Pattern Matching)**：優先使用 `switch` 運算式與 `if (obj is MyType myObj)` 進行轉型與比對，取代舊式的 `as MyType` 語法。

---

## 🔥 3. AOT 編譯限制 (Native AOT Constraints)

本專案啟用了 `<PublishAot>true</PublishAot>`，**這是最重要的限制，違反此規範會導致 AOT 發布失敗**。

### 🚫 AOT 嚴禁使用的技術
* **執行期 Reflection**：禁止使用 `Type.GetMethod()`, `Activator.CreateInstance()` 等依賴執行期反射的 API。
* **`JsonSerializer.Serialize<T>()`（無來源產生器）**：不可使用動態序列化，必須透過 `JsonSerializerContext`。
* **`dynamic` 型別**：完全禁止。
* **部分 LINQ 功能**：少數運算子在 AOT 中可能受限，應測試驗證。

### ✅ AOT 正確的 JSON 序列化做法

每新增一個用於 API 回應 / 請求的型別，**必須**在 `AppJsonSerializerContext` 中加上對應的 `[JsonSerializable]` 標記：

```csharp
// ✅ 正確：在 AppJsonSerializerContext 中註冊
[JsonSerializable(typeof(List<MyModel>))]
[JsonSerializable(typeof(MyModel))]
[JsonSerializable(typeof(MyModelRequest))]
internal partial class AppJsonSerializerContext : JsonSerializerContext { }
```

若忘記加入而直接在 Route Handler 中回傳型別，AOT 發布時會出現序列化失敗的錯誤。

---

## 🌐 4. RESTful API 設計規範 (Minimal API 路由風格)

### 📌 路由定義方式

本專案**不使用 Controller**。路由一律透過 `app.Map*` 方法定義，並使用 `MapGroup` 進行分組：

```csharp
// ✅ 正確：使用 MapGroup 分組，搭配鏈式方法提供 OpenAPI 元資料
var todosApi = app.MapGroup("/api/todos");

todosApi.MapGet("/", () => TypedResults.Ok(list))
        .WithName("GetTodos")
        .WithSummary("取得所有代辦事項")
        .WithDescription("回傳所有現有的代辦事項清單。");
```

### 📌 HTTP 動詞使用慣例
| 動詞     | 方法             | 用途                               | 成功回應碼        |
|----------|------------------|------------------------------------|------------------|
| `GET`    | `MapGet`         | 取得資源（集合或單筆）             | `200 OK`         |
| `POST`   | `MapPost`        | 新增資源                           | `201 Created`    |
| `PUT`    | `MapPut`         | 完整替換資源                       | `204 No Content` |
| `PATCH`  | `MapPatch`       | 部分更新資源                       | `204 No Content` |
| `DELETE` | `MapDelete`      | 刪除資源                           | `204 No Content` |

### 📌 路由設計規範
* **統一前綴**：所有 API 路由必須以 `api/` 開頭，透過 `MapGroup("/api/...")` 定義。
* **小寫命名**：路由路徑一律使用小寫。
* **資源導向**：路由應以**名詞**（資源名稱）命名，禁止使用動詞，例如 `/api/users` 而非 `/api/getUsers`。
* **巢狀資源**：若有關聯關係，可使用巢狀路由，例如 `GET /api/users/{userId}/orders`，但巢狀深度不應超過兩層。

### 📌 HTTP 狀態碼使用規範
* `200 OK`：查詢成功並有回傳資料。
* `201 Created`：新增成功，**必須**在 Response Header 附上 `Location`（使用 `TypedResults.Created(uri, payload)`）。
* `204 No Content`：操作成功但無回傳資料（PUT, PATCH, DELETE）。
* `400 Bad Request`：用戶端請求格式錯誤或驗證失敗。
* `401 Unauthorized`：未經身份驗證。
* `403 Forbidden`：已驗證但無權限。
* `404 Not Found`：指定資源不存在。
* `409 Conflict`：資源衝突（如重複建立）。
* `500 Internal Server Error`：伺服器內部錯誤，**不得**將原始例外訊息直接暴露給客戶端。

---

## 🎯 5. TypedResults 使用規範

本專案**必須**使用 `TypedResults` 靜態類別（而非 `Results`），以獲得強型別回應，讓 OpenAPI 文件能自動推斷回應型別。

### ✅ 正確做法：TypedResults + 回傳型別宣告

```csharp
// ✅ 正確：明確宣告回傳型別 Results<Ok<T>, NotFound>，搭配 TypedResults
todosApi.MapGet("/{id}", Results<Ok<Todo>, NotFound> (int id) =>
    sampleTodos.FirstOrDefault(t => t.Id == id) is { } todo
        ? TypedResults.Ok(todo)
        : TypedResults.NotFound())
    .WithName("GetTodoById")
    .WithSummary("取得特定代辦事項");
```

### 🚫 錯誤做法

```csharp
// ❌ 錯誤：使用 IResult 會失去型別資訊，OpenAPI 文件將不完整
todosApi.MapGet("/{id}", IResult (int id) => { ... });

// ❌ 錯誤：使用非型別化的 Results 靜態類別
return Results.Ok(todo);  // 應改為 TypedResults.Ok(todo)
```

---

## 📦 6. DTO 與資料模型規範

### 📌 DTO 設計原則
* **禁止直接暴露 Entity**：API 的 Request Body 與 Response Body 必須使用獨立的 **DTO** 類別，絕對不可將 EF Core Entity 直接作為 API 回傳型別。
* **Record 優先**：DTO 使用 `record` 定義，語法簡潔且自動具備值相等性：
  ```csharp
  public record TodoResponse(int Id, string? Title, DateOnly? DueBy, bool IsComplete);
  public record CreateTodoRequest(string Title, DateOnly? DueBy);
  public record PatchTodoRequest(string? Title, DateOnly? DueBy, bool? IsComplete);
  ```
* **命名慣例**：
  * 請求用：`Create{Resource}Request`, `Update{Resource}Request`, `Patch{Resource}Request`
  * 回應用：`{Resource}Response`, `{Resource}DetailResponse`
* **AOT 序列化**：每個新 DTO 都必須在 `AppJsonSerializerContext` 中加上 `[JsonSerializable(typeof(...))]`。

### 📌 OpenAPI 文件產生方式

Minimal API **不使用 XML Doc Comments** 產生文件，而是透過鏈式方法：

```csharp
// ✅ 正確：使用 .WithSummary() / .WithDescription() 添加文件說明
todosApi.MapPost("/", (CreateTodoRequest req) => { ... })
        .WithName("CreateTodo")
        .WithSummary("新增代辦事項")
        .WithDescription("根據提供的資料建立一筆新的代辦事項，並回傳 Location Header。")
        .Produces<TodoResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest);
```

---

## 📖 7. OpenAPI 文件與 Scalar UI

* **開發環境**：OpenAPI 文件 (`/openapi/v1.json`) 與 Scalar UI (`/scalar/v1`) **僅在開發環境 (`IsDevelopment`)** 下啟用。
* **生產環境禁止暴露**：絕對不可在生產環境下啟用 Scalar 或 OpenAPI 端點，以防止 API 結構外洩。
* **路由名稱唯一**：每個 `MapGet/MapPost` 等呼叫必須透過 `.WithName("...")` 設定唯一的 Endpoint 名稱，才能正確產生 OpenAPI 文件與 `TypedResults.Created()` 的 URI。

---

## 🗄️ 8. Entity Framework Core 與資料庫操作

### 🔄 非同步與效能最佳化
* **非同步優先 (Async First)**：所有資料庫操作**必須**使用 async/await (如 `ToListAsync()`, `FirstOrDefaultAsync()`)，嚴禁使用同步呼叫 (如 `.ToList()`)。
* **無追蹤 (No Tracking)**：唯讀查詢必須加上 `.AsNoTracking()` 以提升效能。
* **依賴注入 (DI)**：永遠透過 `builder.Services.AddDbContext<AppDbContext>()` 注入，在 Route Handler 中透過參數注入取得：
  ```csharp
  // ✅ Minimal API 中正確的 DbContext 注入方式
  todosApi.MapGet("/", async (AppDbContext db) =>
      await db.Todos.AsNoTracking().ToListAsync());
  ```

### ⚠️ EF Core 與 AOT 的相容性警告
* EF Core 本身對 AOT 的支援仍在演進中，若啟用 AOT 後出現 Reflection 相關警告，**必須告知開發者**，可能需要針對特定功能停用 AOT 或改用其他方案。

### ⚠️ EF Core 深度檢查政策 (Deep Check Policy)
在審查或重構後端程式碼時，AI **必須**進行兩輪檢查：
1. **第一輪 (表面檢查)**：語法、`using` 匯入、DI 注入正確性、變數命名及基本 Null 檢查。
2. **第二輪 (深度檢查) [強制]**：
   * 🔴 漏掉 `await` 或未正確處理 `Task`。
   * 🔴 迴圈內的 **N+1 查詢問題** (應使用 `.Include()`, `.Select()` 或在迴圈前批次取得)。
   * 🔴 未釋放 `IDisposable` 資源 (Stream, HttpClient 等應使用 `using (...) { }` 或 `using var obj = ...;`)。
   * 🟡 EF Core 的同步呼叫。
   * 🟡 唯讀查詢未加上 `.AsNoTracking()`。
*(註：若 AI 僅執行第一輪檢查，必須明確宣告：「⚠️ I have only performed basic checks. EF Core and Memory deep checks are still required.」)*

### 🚨 資料庫 Schema 變更規範
**在進行任何資料庫 Schema 變更 (Migration, Model 修改) 之前，AI 必須：**
1. 詢問開發者：「這個專案是否已部署至 Production 環境？」
2. 根據回覆：
   * **未部署**：可刪除最後一個未套用的 Migration 並修改現有的，或刪除 DB 重建 (`dotnet ef database drop`, `dotnet ef database update`)。
   * **已部署**：**絕對不可**修改已執行的 Migration，必須建立全新的 Migration 檔案 (`dotnet ef migrations add AddNewColumn`)。

---

## 🏗️ 9. 專案架構慣例

本專案採用 **Minimal API 極簡架構**，與傳統 MVC 的資料夾結構完全不同：

### 📁 建議的檔案結構（隨專案成長擴充）

```text
DotNetMinimalAPI/
├── Program.cs              # 應用進入點：DI 註冊、Middleware、Route 定義
├── Routes/                 # (可選) 當路由增多時，抽出 Route Handler 至此目錄
│   ├── TodoRoutes.cs       # 每個資源一個靜態類別，使用 Extension Method 掛載
│   └── UserRoutes.cs
├── Models/                 # Entity 類別 (EF Core)
│   └── Todo.cs
├── Models/Dtos/            # DTO 類別 (Request / Response)
│   ├── TodoResponse.cs
│   └── CreateTodoRequest.cs
├── Data/                   # DbContext 與資料庫設定
│   └── AppDbContext.cs
├── Services/               # (可選) 業務邏輯層
│   └── TodoService.cs
└── note/                   # 開發筆記
```

### 📌 Route Handler 拆分方式（Extension Method 模式）

當 `Program.cs` 過長時，將路由抽出為 Extension Method：

```csharp
// Routes/TodoRoutes.cs
public static class TodoRoutes
{
    public static RouteGroupBuilder MapTodoRoutes(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetAllTodos).WithName("GetTodos").WithSummary("取得所有代辦事項");
        group.MapPost("/", CreateTodo).WithName("CreateTodo").WithSummary("新增代辦事項");
        return group;
    }

    private static async Task<Ok<List<TodoResponse>>> GetAllTodos(AppDbContext db) =>
        TypedResults.Ok(await db.Todos.AsNoTracking().Select(t => new TodoResponse(t.Id, t.Title)).ToListAsync());
}

// Program.cs 中掛載
var todosApi = app.MapGroup("/api/todos");
todosApi.MapTodoRoutes();
```

### 🛑 禁止事項
* **絕對禁止**建立繼承自 `ControllerBase` 或 `Controller` 的類別（這是 MVC 模式，本專案使用 Minimal API）。
* **禁止**使用 `[ApiController]`, `[Route]`, `[HttpGet]` 等 MVC 專用 Attribute。
* **本專案不使用** Views、Razor Pages、HTMX、wwwroot、靜態檔案、Session 或 Cookie 驗證。

---

## 🛠️ 10. 開發工具、設定與其他規範

* **開發環境 (dotnet CLI)**：
  * 使用 `dotnet run` 或 `dotnet watch` 進行熱重載開發。
  * 執行前務必確認 `appsettings.json` 與 `appsettings.Development.json` 設定正確。
  * API 測試可使用 `.http` 檔案 (`DotNetMinimalAPI.http`) 或直接前往 Scalar UI (`/scalar/v1`)。
* **AOT 發布驗證**：
  * 正式發布前可執行 `dotnet publish -r osx-arm64 -c Release` (依目標平台調整) 驗證 AOT 編譯是否成功。
  * 若出現 AOT 相關警告，**必須**向開發者回報，不可忽略。
* **警告與 Lint 忽略政策**：
  * **絕對不可**在沒有使用者明確指示下加入 `#pragma warning disable`。
  * 若遇到編譯器警告：先向使用者回報 ➡️ 等待明確指示 ➡️ 加上停用註解與正當理由。
* **禁止腳本重構 (No Scripts for Refactoring)**：
  * **絕對禁止**使用 `sed`, `awk`, `powershell`, bash 腳本等自動化腳本來修改程式碼，因為腳本無法理解 C# 語意與 `using` 命名空間。
  * ✅ **允許作法**：使用 AI 工具 (如 `replace_file_content`) 進行精準修改，並在修改後務必驗證 `using` 宣告與建置狀態。
