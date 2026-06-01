# 🤖 DotNet Mvc API - AI 協作開發規範指南 (AI Coding Guidelines)

本文件綜合了專案中的各個 AI 設定檔，為開發過程中的 AI 助手 (如 Copilot, Claude, Gemini) 提供統一、詳盡的中文規範指引。

> **專案性質**：本專案為純 **ASP.NET Core Web API** 專案，**不包含** Razor Views、HTMX、前端樣式 (CSS/SCSS) 或 wwwroot 靜態資源。所有輸出均為 JSON 格式，透過 HTTP 端點提供服務。

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
* **嚴格型別 (Strict Typing)**：絕對**禁止**使用 `dynamic` 或 `object`，除非在反射 (Reflection) 或處理無型別 JSON 等絕對必要的情況下。應優先使用強型別的泛型集合 (如 `List<T>`) 而非 `ArrayList`。
* **隱式型別 (Implicit Typing)**：避免使用 `var`，除非等號右側的型別已非常明顯 (例如：`var list = new List<string>();`)。

### 🛡️ 執行期資料驗證與 Null 檢查 (Runtime Validation)
* **字串檢查**：使用 `string.IsNullOrEmpty(str)` 或 `string.IsNullOrWhiteSpace(str)`。
* **Null 檢查**：使用 `if (obj is not null)` 或 Null 聯合運算子 (`??`)。
* **防護子句 (Guard Clauses)**：在方法開頭使用 `ArgumentNullException.ThrowIfNull(obj)`。
* **模式匹配 (Pattern Matching)**：優先使用 `switch` 運算式與 `if (obj is MyType myObj)` 進行轉型與比對，取代舊式的 `as MyType` 語法。

---

## 🌐 3. RESTful API 設計規範

### 📌 HTTP 動詞使用慣例
| 動詞     | 用途                               | 成功回應碼       |
|----------|------------------------------------|-----------------|
| `GET`    | 取得資源（集合或單筆）             | `200 OK`        |
| `POST`   | 新增資源                           | `201 Created`   |
| `PUT`    | 完整替換資源                       | `204 No Content`|
| `PATCH`  | 部分更新資源                       | `204 No Content`|
| `DELETE` | 刪除資源                           | `204 No Content`|

### 📌 路由設計規範
* **統一前綴**：所有 API 路由必須以 `api/` 開頭，例如 `[Route("api/[controller]")]`。
* **小寫命名**：路由路徑一律使用小寫（ASP.NET Core 預設已處理）。
* **資源導向**：路由應以**名詞**（資源名稱）命名，禁止使用動詞，例如 `/api/users` 而非 `/api/getUsers`。
* **巢狀資源**：若有關聯關係，可使用巢狀路由，例如 `GET /api/users/{userId}/orders`，但巢狀深度不應超過兩層。

### 📌 HTTP 狀態碼使用規範
* `200 OK`：查詢成功並有回傳資料。
* `201 Created`：新增成功，**必須**在 Response Header 附上 `Location` (使用 `CreatedAtAction`)。
* `204 No Content`：操作成功但無回傳資料（PUT, PATCH, DELETE）。
* `400 Bad Request`：用戶端請求格式錯誤或驗證失敗。
* `401 Unauthorized`：未經身份驗證。
* `403 Forbidden`：已驗證但無權限。
* `404 Not Found`：指定資源不存在。
* `409 Conflict`：資源衝突（如重複建立）。
* `500 Internal Server Error`：伺服器內部錯誤，**不得**將原始例外訊息 (Exception) 直接暴露給客戶端。

---

## 📦 4. DTO 與回應格式規範

### 📌 DTO 設計原則
* **禁止直接暴露 Entity**：API 的 Request Body 與 Response Body 必須使用獨立的 **DTO (Data Transfer Object)** 類別，絕對不可直接將 EF Core Entity 作為 API 回傳型別。
* **命名慣例**：
  * 請求用：`CreateUserRequest`, `UpdateUserRequest`, `PatchUserRequest`
  * 回應用：`UserResponse`, `UserDetailResponse`
* **DTO 存放位置**：統一放置於 `Models/Dtos/` 子資料夾內。

### 📌 XML 文件註解 (XML Doc Comments)
* 本專案已啟用 `<GenerateDocumentationFile>true</GenerateDocumentationFile>`，所有公開的 Controller Action **必須**加上 XML 文件註解（`<summary>`, `<param>`, `<returns>`），以確保 OpenAPI 文件自動產生完整的說明。
* `<NoWarn>$(NoWarn);1591</NoWarn>` 已設定，但仍建議所有公開成員都加上註解。

---

## 📖 5. OpenAPI 文件與 Scalar UI

* **開發環境**：OpenAPI 文件 (`/openapi/v1.json`) 與 Scalar UI (`/scalar/v1`) **僅在開發環境 (`IsDevelopment`)** 下啟用。
* **生產環境禁止暴露**：絕對不可在生產環境下啟用 Scalar 或 Swagger 端點，以防止 API 結構外洩。
* **文件完整性**：每個 Action 應透過 XML 文件註解與 `[ProducesResponseType]` 屬性，明確宣告所有可能的 HTTP 回應型別，以產生最完整的 OpenAPI 規格。

---

## 🗄️ 6. Entity Framework Core 與資料庫操作

### 🔄 非同步與效能最佳化
* **非同步優先 (Async First)**：所有資料庫操作**必須**使用 async/await (如 `ToListAsync()`, `FirstOrDefaultAsync()`)，嚴禁使用同步呼叫 (如 `.ToList()`)。
* **無追蹤 (No Tracking)**：唯讀查詢必須加上 `.AsNoTracking()` 以提升效能。
* **依賴注入 (DI)**：永遠透過建構子注入 `DbContext`，絕對不要使用 `new AppDbContext()` 實例化。

### ⚠️ EF Core 與記憶體深度檢查 (Deep Check Policy)
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

## 🏗️ 7. 專案架構慣例

* **Controllers/**：所有 API 控制器必須：
  * 繼承自 `ControllerBase`（**不是** `Controller`，因為本專案無 Razor Views）。
  * 標記 `[ApiController]` 屬性（自動處理模型驗證與綁定來源推斷）。
  * 標記 `[Route("api/[controller]")]` 屬性。
  * 類別名稱必須以 `Controller` 結尾。
* **Models/**：包含 Entity 類別、DTOs（建議放 `Models/Dtos/` 子目錄）。
* **ASP.NET Core 穩定 API**：優先使用標準的 Web API 模式與內建依賴注入。除非舊有程式碼必需，否則使用 `System.Text.Json` 而非 `Newtonsoft.Json`。
* **🛑 禁止事項**：本專案**不使用** Views、Razor Pages、HTMX、wwwroot、靜態檔案、Session 或 Cookie 驗證。

---

## 🛠️ 8. 開發工具、設定與其他規範

* **開發環境 (dotnet CLI)**：
  * 使用 `dotnet run` 或 `dotnet watch` 進行熱重載開發。
  * 執行前務必確認 `appsettings.json` 與 `appsettings.Development.json` 設定正確。
  * API 測試可使用 `.http` 檔案 (`DotNetMvcAPI.http`) 或直接前往 Scalar UI (`/scalar/v1`)。
* **警告與 Lint 忽略政策**：
  * **絕對不可**在沒有使用者明確指示下加入 `#pragma warning disable`。
  * 若遇到編譯器警告：先向使用者回報 ➡️ 等待明確指示 ➡️ 加上停用註解與正當理由。
* **禁止腳本重構 (No Scripts for Refactoring)**：
  * **絕對禁止**使用 `sed`, `awk`, `powershell`, bash 腳本等自動化腳本來修改程式碼，因為腳本無法理解 C# 語意與 `using` 命名空間。
  * ✅ **允許作法**：使用 AI 工具 (如 `replace_file_content`) 進行精準修改，並在修改後務必驗證 `using` 宣告與建置狀態。
