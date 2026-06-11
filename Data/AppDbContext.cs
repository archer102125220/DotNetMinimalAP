using DotNetMinimalAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace DotNetMinimalAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<OracleDemoItem> OracleDemoItems { get; set; }
    public DbSet<OracleDemoCategory> OracleDemoCategories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Ensure names are capitalized/quoted correctly if needed for Oracle
        modelBuilder.Entity<OracleDemoItem>()
            .HasOne(i => i.Category)
            .WithMany(c => c.Items)
            .HasForeignKey(i => i.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
