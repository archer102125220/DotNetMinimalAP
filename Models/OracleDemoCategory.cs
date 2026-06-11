namespace DotNetMinimalAPI.Models;

// 💡【教學：什麼是 Entity (實體)？】
// 這個類別代表資料庫中的「一張表」(Table)。
// EF Core 會將這個類別映射到名為 "OracleDemoCategories" 的資料表。
// 注意：這個類別只負責和資料庫溝通，【絕對不應該】直接作為 API 的回傳結果，以免洩漏機密欄位。
public class OracleDemoCategory
{
    // 慣例上，命名為 Id 的欄位會自動被 EF Core 識別為主鍵 (Primary Key)

    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // 💡【教學：導覽屬性 Navigation Property】
    // 這裡定義了「一對多」的關係：一個分類 (Category) 可以包含多個項目 (Items)。
    // 使用 ICollection 介面是 EF Core 推薦的標準作法。
    public ICollection<OracleDemoItem> Items { get; set; } = new List<OracleDemoItem>();
}
