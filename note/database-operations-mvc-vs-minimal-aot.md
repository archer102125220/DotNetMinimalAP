# 資料庫連線操作：DotNetMvcWeb 與 DotNetMinimalAPI 的差異比較

在從傳統 MVC (`DotNetMvcWeb`) 轉換到現代化 Minimal API (`DotNetMinimalAPI`) 的過程中，資料庫的連線與操作方式有著根本上的典範轉移。特別是為了配合 **Native AOT (提前編譯)**，我們的實作方式產生了巨大的差異。

---

## 1. 架構與依賴注入 (Dependency Injection) 的差異

### 🏛️ 傳統 MVC (DotNetMvcWeb)
- **連線取得方式**：依賴 `Controller` 的建構子 (Constructor) 進行注入。
- **寫法特徵**：需要宣告 private readonly 欄位，並在建構子中指派。
- **生命週期**：`DbContext` 的生命週期綁定在整個 Controller 的 Request 範圍。

```csharp
// MVC Controller 寫法
public class OracleDemoController : Controller
{
    private readonly AppDbContext _db;

    public OracleDemoController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var items = await _db.OracleDemoItems.ToListAsync();
        return View(items);
    }
}
```

### 🚀 Minimal API (DotNetMinimalAPI)
- **連線取得方式**：直接在 Lambda 函式的 **參數 (Parameter)** 宣告 `AppDbContext db`，框架會自動進行方法層級的依賴注入。
- **寫法特徵**：不需要建構子、不需要 private 欄位，寫法極度精簡，就像呼叫一般函式一樣。
- **記憶體優勢**：因為沒有 Controller 實體化 (Instantiation) 的過程，記憶體分配更少，執行速度更快。

```csharp
// Minimal API 寫法
app.MapGet("/api/oracle-demo", async (AppDbContext db) => 
{
    var items = await db.OracleDemoItems.ToListAsync();
    return TypedResults.Ok(items);
});
```

---

## 2. 查詢設計的致命差異：JIT vs Native AOT

這是兩個專案差異 **最大且最容易踩坑** 的地方。

### 🏛️ 傳統 MVC (依賴 JIT 即時編譯)
在 `DotNetMvcWeb` 專案中，我們大量使用了**動態 LINQ (Dynamic LINQ)** 或 `FromSqlInterpolated` 來組裝查詢條件：
```csharp
// MVC 中常見的動態查詢 (依賴 JIT 編譯)
var query = _db.OracleDemoItems.AsQueryable();

if (!string.IsNullOrEmpty(keyword))
{
    // 在執行期動態將 Expression Tree 轉譯成 SQL
    query = query.Where(i => i.Name.Contains(keyword));
}

var results = await query.ToListAsync();
```
這種寫法在傳統環境下運作完美，因為 .NET 有 JIT (Just-In-Time) 編譯器，能在執行期動態編譯這些 LINQ 樹狀結構。

### 🚀 Minimal API (配合 Native AOT)
`DotNetMinimalAPI` 啟用了 `<PublishAot>true</PublishAot>`。在 Native AOT 環境下，**沒有 JIT 編譯器**！
任何依賴執行期動態生成 SQL 的寫法（包含上面 MVC 的動態 LINQ），都會在執行時直接拋出 `System.InvalidOperationException: Dynamic LINQ queries are not supported when precompiling queries.` 錯誤導致程式崩潰。

**💡 解決方案：強制使用預先編譯查詢 (`EF.CompileAsyncQuery`)**
在 Minimal API 專案中，我們必須把所有查詢獨立抽出來，宣告為 **靜態唯讀委派 (Static Readonly Func)**。這樣 .NET 編譯器在建置期就能確定所有 SQL 的形狀：

```csharp
// 必須宣告為靜態 CompileAsyncQuery
private static readonly Func<AppDbContext, string, IAsyncEnumerable<OracleDemoItem>> SearchItemsQuery =
    EF.CompileAsyncQuery<AppDbContext, string, OracleDemoItem>(
        (AppDbContext db, string pattern) => db.OracleDemoItems.Where(i => EF.Functions.Like(i.Name, pattern)).AsNoTracking()
    );

// 在 API 中直接呼叫
app.MapGet("/", async (string keyword, AppDbContext db) => 
{
    var searchPattern = $"%{keyword}%";
    var results = new List<OracleDemoItem>();
    
    // 不再使用 ToListAsync，而是用 await foreach 展開編譯好的查詢
    await foreach (var item in SearchItemsQuery(db, searchPattern))
    {
        results.Add(item);
    }
    
    return TypedResults.Ok(results);
});
```

---

## 3. 資料庫模型的解析方式

### 🏛️ 傳統 MVC (動態反射)
- EF Core 在啟動 `DbContext` 時，會使用 **反射 (Reflection)** 動態掃描所有的 Model 類別，建立記憶體中的模型結構 (IModel)。
- 缺點是啟動較慢，且消耗較多記憶體。

### 🚀 Minimal API (靜態編譯模型 Compiled Models)
- 由於 Native AOT 禁用反射，我們必須在開發期使用指令 `dotnet ef dbcontext optimize -c AppDbContext -o CompiledModels --nativeaot` 產生靜態模型。
- 在 `Program.cs` 註冊時，強制指定使用靜態模型，完全跳過反射掃描：
```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseOracle(connectionString);
    // 💡 關鍵差異：掛載 Compiled Models 靜態快照
    options.UseModel(DotNetMinimalAPI.CompiledModels.AppDbContextModel.Instance);
});
```
這使得 Minimal API 的冷啟動速度達到毫秒級別，非常適合 Serverless 或雲端微服務架構。

---

## 結論總結

| 比較項目 | DotNetMvcWeb (傳統 MVC) | DotNetMinimalAPI (Native AOT) |
| :--- | :--- | :--- |
| **依賴注入 (DI)** | Controller 建構子注入 | Lambda 方法參數注入 |
| **LINQ 查詢方式** | 支援執行期動態組合 (`IQueryable`) | **必須使用** `EF.CompileAsyncQuery` 靜態定義 |
| **Model 解析** | 啟動時透過反射 (Reflection) 動態建立 | 使用 Compiled Models 靜態掛載 |
| **資料回傳** | 傳遞 Entity 到 Razor View (`return View()`) | 回傳 DTO 並使用 `TypedResults.Ok()` 提供強型別支援 |
| **效能與體積** | 體積大、啟動慢、記憶體吃重 | 體積極小、毫秒啟動、極低記憶體消耗 |
