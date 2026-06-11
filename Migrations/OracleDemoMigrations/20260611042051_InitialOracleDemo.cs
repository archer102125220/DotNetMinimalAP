using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

// 💡【教學：什麼是 Migrations (資料庫移轉)？】
// Entity Framework Core 使用 Migrations 機制來「記錄並追蹤資料庫 Schema (結構) 的變更」。
// 這個檔案是透過指令 `dotnet ef migrations add InitialOracleDemo` 自動產生的。
// 
// - 當我們修改了 Models 資料夾裡的類別 (例如新增欄位、改資料表名稱) 時，
//   我們不需要手動去資料庫下 SQL 語法修改 Schema。
// - 只要執行 Add Migration 指令，EF Core 就會自動比對現有的模型與上一次的 Migration，
//   然後產生出像這個檔案一樣的 C# 程式碼。
// - 接著執行 `dotnet ef database update` 時，EF Core 會自動把這裡的 C# 程式碼翻譯成 
//   對應資料庫 (例如 Oracle, SQL Server, Postgres) 的專屬 SQL (例如 CREATE TABLE, ALTER TABLE) 去執行。
// 
// 【檔案結構說明】
// Up() 方法：代表套用這個 Migration 時要執行的動作 (例如：建立資料表)。
// Down() 方法：代表復原 (Rollback) 這個 Migration 時要執行的動作 (例如：刪除資料表)。

namespace DotNetMinimalAPI.Migrations.OracleDemoMigrations
{
    /// <inheritdoc />
    public partial class InitialOracleDemo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OracleDemoCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Name = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OracleDemoCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OracleDemoItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Name = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Description = table.Column<string>(type: "NVARCHAR2(2000)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    CategoryId = table.Column<int>(type: "NUMBER(10)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OracleDemoItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OracleDemoItems_OracleDemoCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "OracleDemoCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OracleDemoItems_CategoryId",
                table: "OracleDemoItems",
                column: "CategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OracleDemoItems");

            migrationBuilder.DropTable(
                name: "OracleDemoCategories");
        }
    }
}
