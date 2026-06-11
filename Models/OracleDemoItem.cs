namespace DotNetMinimalAPI.Models;

// 💡【教學：Entity 與 Nullable Reference Types】
// 在專案啟用了 <Nullable>enable</Nullable> 的情況下，我們必須明確區分欄位是否可以為 null。
public class OracleDemoItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    // 💡【教學：可為 Null 的欄位與外鍵】
    // 使用 `int?` 和 `string?` 明確告訴 EF Core 這些在資料庫裡是 Nullable (可為空) 的欄位。
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // 定義外鍵 (Foreign Key)
    public int? CategoryId { get; set; }

    // 💡【教學：關聯導覽屬性】
    // 這個屬性讓你可以透過 EF Core 的 `.Include(x => x.Category)` 將關聯的資料一起撈出來。
    public OracleDemoCategory? Category { get; set; }
}
