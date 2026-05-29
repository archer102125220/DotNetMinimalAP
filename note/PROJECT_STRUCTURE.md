# DotNetMinimalAPI 專案結構與設定說明

這個文件主要說明 `DotNetMinimalAPI` 這個 .NET 10 Minimal API 專案的基本結構、重要的設定檔案，以及如何理解這個專案的架構。

## 📁 專案目錄結構

```text
DotNetMinimalAPI/
├── Program.cs                     # 應用程式的進入點與 API 路由定義
├── DotNetMinimalAPI.csproj        # 專案檔 (定義框架版本、套件依賴、AOT 設定)
├── DotNetMinimalAPI.http          # HTTP 請求測試檔 (可以直接在 IDE 中發送請求測試 API)
├── appsettings.json               # 全域的應用程式設定檔
├── appsettings.Development.json   # 僅在開發環境 (Development) 載入的設定檔
├── Properties/
│   └── launchSettings.json        # 啟動設定檔 (定義執行的 Port、環境變數等)
├── bin/                           # 編譯後輸出的執行檔目錄
├── obj/                           # 編譯過程中的暫存檔案目錄
└── note/                          # 放置開發筆記與說明的目錄
```

## 📄 核心檔案說明

### 1. `Program.cs`
在 Minimal API 專案中，`Program.cs` 是整個應用的心臟。這裡面包含了：
- **依賴注入 (DI)**：註冊應用程式所需的所有服務。
- **中介軟體 (Middleware)**：設定 HTTP 請求處理的管線，例如：CORS、驗證、授權、API 文件等。
- **API 路由與處理邏輯**：直接在 `app.MapGet`、`app.MapPost` 等方法內實作每個 API 的邏輯 (本專案以 `/todos` 路由作為範例，實作了完整的 CRUD 功能)。
- **資料模型定義**：在檔案底部定義了使用於 API 請求或回應的 Data Transfer Objects (例如 `Todo`, `TodoPatch`)。
- **JSON 序列化設定**：因為專案啟用了 AOT (Ahead-of-Time 編譯)，傳統的反序列化方式會無法運作，所以必須透過 `AppJsonSerializerContext` 產生針對各個類型的 `JsonSerializable` 標記，並傳遞給 `ConfigureHttpJsonOptions`。

### 2. `DotNetMinimalAPI.csproj`
這是 MSBuild 專案檔，定義了如何編譯此專案。本專案中的關鍵設定：
- `<TargetFramework>net10.0</TargetFramework>`: 宣告專案使用 .NET 10 框架。
- `<PublishAot>true</PublishAot>`: 啟用 Native AOT 編譯。這能大幅加快啟動時間和降低記憶體使用量，代價是無法在執行期使用 Reflection 等動態特性。
- **套件依賴** (`PackageReference`): 例如用來產生 OpenAPI 文件的 `Microsoft.AspNetCore.OpenApi`，以及用來顯示漂亮 API 測試畫面的 `Scalar.AspNetCore`。

### 3. `appsettings.json` 與 `appsettings.Development.json`
這兩個是專案的設定檔，用來存放連線字串、日誌等級、或其他自訂參數。
- `appsettings.json`：存放預設的所有環境通用的設定。
- `appsettings.Development.json`：存放開發環境的專屬設定。它會覆蓋 `appsettings.json` 裡面重複的設定，且**不應該**將生產環境的機密資料（例如真正的資料庫連線字串）存放在這裡。通常專案跑在本機時，環境變數 `ASPNETCORE_ENVIRONMENT` 為 `Development`，因此預設會吃這個檔案的設定。

### 4. `Properties/launchSettings.json` (若存在)
這個檔案控制了當你在開發環境執行應用程式（透過 Visual Studio / VS Code 或 `dotnet run`）時的啟動行為，包括：
- 應用的存取 URL (`applicationUrl` 像是 `http://localhost:5000`)
- 環境變數的注入 (`ASPNETCORE_ENVIRONMENT` 設為 `Development`)

### 5. `DotNetMinimalAPI.http`
這是一個用於快速測試 API 的文字檔。如果你使用支援 `.http` 檔案格式的編輯器（像是 Visual Studio、Rider 或裝有 REST Client 擴充套件的 VS Code），你可以直接點擊檔案內的「Send Request」按鈕，快速測試 API 端點，不用打開 Postman 等外部工具。

## 🛠 特色摘要 (AOT + Minimal API + Scalar)
- **極簡化**: 沒有傳統 MVC 架構中的 Controller，將路由和處理邏輯整合在 `Program.cs`，非常適合微服務或小型 API 開發。
- **AOT 編譯準備**: 透過 `WebApplication.CreateSlimBuilder` 以及 `JsonSerializerContext` 處理序列化，應用程式被設計為能編譯出體積極小且高效率的執行檔。
- **現代 API 文件**: 結合了 .NET 內建的 OpenAPI 以及第三方的 `Scalar.AspNetCore`，提供了比傳統 Swagger 更加現代、輕量的 API 互動介面 (透過瀏覽器造訪 `/scalar`)。
