namespace DotNetMinimalAPI.Models.Dtos;

// 💡【教學：什麼是 DTO (Data Transfer Object)？】
// 為什麼我們不直接回傳 OracleDemoItem (Entity)？
// 1. 避免暴露出資料庫的內部結構 (例如密碼、內部狀態、或未來新增的敏感欄位)。
// 2. 避免 EF Core 的關聯導覽屬性 (Navigation Property) 造成 JSON 序列化的無窮迴圈 (例如 Item 包含 Category，Category 又包含 Items...)。
// 3. 不同的 API 端點 (Create, Update, Get) 需要的欄位往往不同，使用獨立的 DTO 能明確定義 API 契約。

// 💡【教學：為什麼要用 record？】
// 這裡使用 C# 9+ 引入的 `record` (紀錄型別) 而不是 class。
// `record` 提供簡潔的語法，並且預設是「不可變的」(Immutable) 及具備「值相等性」(Value Equality)。
// 這非常適合用來作為 Minimal API 接收和回傳資料的純資料容器 (DTO)。

/// <summary>
/// 用於查詢 API 回傳結果的 DTO
/// </summary>
public record OracleDemoItemResponse(int Id, string Name, string? Description, DateTime CreatedAt, int? CategoryId);

/// <summary>
/// 用於建立 API (POST) 接收參數的 DTO (注意：不需要 Id 和 CreatedAt，因為這是資料庫自動產生的)
/// </summary>
public record CreateOracleDemoItemRequest(string Name, string? Description, int? CategoryId);

/// <summary>
/// 用於更新 API (PUT) 接收參數的 DTO
/// </summary>
public record UpdateOracleDemoItemRequest(string Name, string? Description, int? CategoryId);
