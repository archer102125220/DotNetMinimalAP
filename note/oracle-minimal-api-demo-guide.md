# Oracle Database Minimal API 實作導讀指南

這份文件是用來協助快速理解本專案中如何將 .NET Minimal API 與 Oracle 資料庫結合。我們使用 Entity Framework Core (EF Core) 並透過極簡的路由寫法，提供了一組純 JSON 格式的 RESTful API，方便前端框架 (Vue, React, Nuxt) 或是 Postman 進行串接與測試。

> 💡 **相關閱讀**：如果想深入了解背後的資料庫設定、EF Core 常用指令及踩坑紀錄，請參考 [Entity Framework Core (EF Core) 實戰教學指南](./ef-core-orm-guide.md)。
> 💡 **相關閱讀**：想了解 Minimal API 的路由宣告方式與回傳機制，請參考 [ASP.NET Core Minimal API 路由與端點設計教學](./minimal-api-routing-guide.md)。

---

## 1. 核心架構概覽

在 Minimal API 架構下，我們去除了傳統的 Controller 與 View，架構變得非常平坦且直接：

### 📦 Model & Data (資料模型與連線)
* **`Models/OracleDemoItem.cs`**：定義了資料庫中 `OracleDemoItems` 資料表的結構（包含 `Id`, `Name`, `Description`, `CreatedAt`）。
* **`Data/AppDbContext.cs`**：負責定義與 Oracle 資料庫的連線上下文。

### 🎮 Endpoints (API 端點)
* **`Program.cs` (或抽離的 Route Extensions)**：我們直接在這裡使用 `app.MapGroup("/api/oracle-demo")` 來宣告所有的 CRUD 操作。
* 大量使用了 `async / await` 以及 EF Core 的 `.AsNoTracking()` 來最佳化唯讀查詢的效能。

---

## 🛠️ API 端點列表 (Endpoints)

以下是我們實作的標準 RESTful API 結構，回傳格式皆為 `application/json`。

### 1. 取得所有資料 (支援搜尋)
* **方法**: `GET`
* **路徑**: `/api/oracle-demo`
* **Query 參數**: `?keyword={字串}` (選填，若提供則會使用原生的 `FromSqlInterpolated` 進行模糊搜尋)
* **回應範例 (200 OK)**:
  ```json
  [
    {
      "id": 1,
      "name": "測試資料 1",
      "description": "這是一段測試",
      "createdAt": "2026-06-02T12:00:00Z"
    }
  ]
  ```

### 2. 取得單筆資料
* **方法**: `GET`
* **路徑**: `/api/oracle-demo/{id}`
* **回應範例 (200 OK)**: (回傳單一物件)
* **回應 (404 Not Found)**: 若找不到對應 ID 的資料。

### 3. 建立新資料
* **方法**: `POST`
* **路徑**: `/api/oracle-demo`
* **Request Body (JSON)**: `id` 與 `createdAt` 不需提供，資料庫與 EF Core 會自動生成。
  ```json
  {
    "name": "我是新建立的資料",
    "description": "透過 POST 建立"
  }
  ```
* **回應 (201 Created)**: 回傳剛建立好的完整物件資料，並在 Header 附上 Location。

### 4. 更新資料
* **方法**: `PUT`
* **路徑**: `/api/oracle-demo/{id}`
* **Request Body (JSON)**: 必須傳送完整的物件資訊。
  ```json
  {
    "name": "更新後的名稱",
    "description": "更新後的描述"
  }
  ```
* **回應 (204 No Content)**: 更新成功不回傳內容。若找不到資料回傳 `404 Not Found`。

### 5. 刪除資料
* **方法**: `DELETE`
* **路徑**: `/api/oracle-demo/{id}`
* **回應 (204 No Content)**: 刪除成功不回傳內容。

---

## 3. 核心實作亮點

### 後端實作：安全的 原生 SQL (Raw SQL)
在列表搜尋 API 中，我們示範了如何透過 `FromSqlInterpolated` 來執行原生的 Oracle SQL 查詢：
```csharp
app.MapGet("/", async (string? keyword, AppDbContext db) => 
{
    if (string.IsNullOrEmpty(keyword))
    {
        return TypedResults.Ok(await db.OracleDemoItems.AsNoTracking().ToListAsync());
    }

    var searchPattern = $"%{keyword}%";
    var results = await db.OracleDemoItems
        .FromSqlInterpolated($"SELECT * FROM \"OracleDemoItems\" WHERE \"Name\" LIKE {searchPattern}")
        .AsNoTracking()
        .OrderByDescending(i => i.CreatedAt)
        .ToListAsync();
        
    return TypedResults.Ok(results);
});
```
**安全防護重點**：寫原生 SQL 時，**強烈建議使用 `FromSqlInterpolated`**！EF Core 會自動在底層將變數 (`{searchPattern}`) 轉換為參數化查詢 (Parameterized Query)，這能 100% 防止 SQL Injection (資料隱碼攻擊)。同時，別忘了搭配 `.AsNoTracking()` 提升唯讀查詢效能。

### 依賴注入的極簡化
在 Minimal API 中，我們不需要寫 Constructor，只要在 Lambda 函式的參數宣告 `AppDbContext db`，框架就會自動從 DI 容器中把連線倒給我們，寫起來就像一般函數一樣自然！

### JSON 序列化與 AOT 支援
在 `Program.cs` 的最上方，我們宣告了：
```csharp
[JsonSerializable(typeof(List<OracleDemoItem>))]
[JsonSerializable(typeof(OracleDemoItem))]
internal partial class AppJsonSerializerContext : JsonSerializerContext { }
```
並將其註冊到 `options.SerializerOptions.TypeInfoResolverChain`。這是因為 .NET Minimal API 強烈支援 **AOT (提前編譯)**，為了讓編譯器在打包時就知道哪些類別需要轉成 JSON，我們使用了 Source Generator 來產生序列化程式碼，這能大幅提升啟動速度並降低記憶體消耗。

---

## 4. 如何測試與執行

1. **確保 Oracle 資料庫運行中**：請確認已準備好 Oracle DB 環境或 Docker 容器。
2. **啟動專案**：在專案根目錄執行 `dotnet watch run` 或直接使用 IDE 執行。
3. **查看 API 文件**：開啟瀏覽器前往 `/scalar` (或 `/swagger`)，即可看到自動產生的漂亮 API 測試介面，您可以直接在網頁上對 Oracle 資料庫進行 CRUD 操作測試。
