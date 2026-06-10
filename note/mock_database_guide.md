# .NET Minimal API 學習導讀：無真實資料庫 (Mock Database) 範例專案

歡迎來到這個 .NET Minimal API 學習專案！這份文件將帶領你了解如何在「**不連接真實資料庫**」的情況下，運用記憶體 (In-Memory) 來模擬資料存取，以此學習 ASP.NET Core Minimal API 的核心概念。

這種設計模式非常適合用來學習：
1. **依賴注入 (Dependency Injection, DI)**：如何將服務注入到 Minimal API 的端點 (Endpoints) 中。
2. **介面抽象 (Interface Abstraction)**：如何透過介面解耦 API 邏輯與具體的資料庫實作。

---

## 1. 核心觀念：資料模型與介面 (Models)

在 `Models/` 資料夾下，我們定義了資料結構與操作資料的合約（介面）。

### A. 實體模型 (`Product.cs`)
這是一個單純的 C# 類別 (POCO) 或 Record，代表了我們要操作的資料結構。
- 屬性如 `Id`, `Name`, `Price`, `Description`。
- 在未來的真實專案中，這個模型會對應到資料庫中的一張資料表 (Table)。

### B. 存取介面 (`IProductRepository.cs`)
我們不希望端點邏輯綁死在某一種特定的資料庫（例如 SQL Server），因此我們宣告了 `IProductRepository` 介面，裡面定義了 CRUD (新增、讀取、更新、刪除) 的方法。
- 方法皆回傳 `Task` 或 `Task<T>`，這是因為**真實的資料庫操作都應該是非同步的 (Asynchronous)**，我們在模擬階段就先遵循這個標準，未來切換成真實 DB 時端點邏輯就不用改寫。

---

## 2. 模擬資料庫實作 (`ProductRepository.cs`)

這是本專案的精華所在：用記憶體來假裝我們有一個資料庫。

```csharp
public class ProductRepository : IProductRepository
{
    private readonly List<Product> _products = new() { ... };
    private readonly object _lock = new();

    // ...實作 CRUD 方法
}
```

### 為什麼要有 `lock (_lock)`？ (執行緒安全)
在 Web 應用程式中，可能會有好幾百個使用者**同時**發送 Request 來到伺服器。
因為這個 Repository 在整個應用程式生命週期中只有一個實例（Singleton，稍後說明），所有人都會存取同一個 `_products` List。如果兩個人同時對 List 進行寫入 (Add/Remove)，會導致程式崩潰或資料錯亂。
因此，我們加上 `lock`，確保同一個時間只有一個 Request 能夠修改這份名單。

### 為什麼要用 `Task.FromResult`？
由於我們的 List 是在記憶體中，讀取速度極快，本身是同步 (Synchronous) 的操作。但因為介面 `IProductRepository` 規定要回傳非同步的 `Task`，所以我們用 `Task.FromResult()` 與 `Task.CompletedTask` 將同步的結果包裝成假裝是非同步的回傳值。

---

## 3. 將模擬資料庫註冊到系統中 (`Program.cs`)

在 ASP.NET Core 中，我們統一在 `Program.cs` 註冊系統需要使用的服務：

```csharp
builder.Services.AddSingleton<IProductRepository, ProductRepository>();
```

**什麼是 `AddSingleton`？**
- 這代表**依賴注入 (DI)** 容器在啟動時，只會建立「一個」`ProductRepository` 實例。
- 整個網站從開啟到關閉，所有的 API 端點都會拿到這同一個實例，所以我們剛才說的 `_products` List 能夠跨 Request 保持資料狀態（你新增了一筆資料，重新呼叫 API 查詢時還在）。

> **💡 未來連接真實資料庫 (EF Core) 時的差異：**
> 真實的資料庫連線不應該從頭到尾共用一條。屆時我們會將其改為 `AddScoped` (每個 Request 建立一次新的連線)。

---

## 4. API 端點如何使用資料 (Endpoints)

在 Minimal API 中，我們透過 `app.Map*` 方法定義端點，並透過參數自動進行依賴注入：

```csharp
var api = app.MapGroup("/api/products");

// 自動透過參數注入 IProductRepository
api.MapGet("/", async (IProductRepository repository) =>
{
    var products = await repository.GetAllAsync();
    return TypedResults.Ok(products);
});

api.MapPost("/", async (Product product, IProductRepository repository) =>
{
    await repository.AddAsync(product);
    return TypedResults.Created($"/api/products/{product.Id}", product);
});
```

- **參數注入**：我們不需要寫建構子，直接在 Lambda 參數中要求系統給一個 `IProductRepository` 即可。這就是 Minimal API 優雅的依賴注入體現。
- **Action 邏輯**：內部呼叫 `await repository.GetAllAsync()` 來取得資料，然後回傳 `TypedResults.Ok(products)` (HTTP 200) 或是 `TypedResults.NotFound()` (HTTP 404)。

---

## 總結與下一步

透過這套模擬架構，你可以在完全不需要安裝 SQL Server 或 Oracle、也不需設定連線字串的情況下，專心學習 **Minimal API 的路由宣告** 與 **依賴注入**。

當你熟悉了這些運作原理後，下一步就可以開始學習 **Entity Framework Core (EF Core)**。
屆時，你只需要：
1. 寫一個新的 `EfProductRepository` 實作 `IProductRepository` (透過 DbContext 去讀寫資料庫)。
2. 到 `Program.cs` 把 `AddSingleton<IProductRepository, ProductRepository>()` 換成新的註冊方式。
3. 你的 Endpoints (API 路由) 完全不需要動任何一行程式碼，整個系統就能無縫切換到真實的資料庫了！這就是**介面抽象 (Interface Abstraction)** 最強大的威力。
