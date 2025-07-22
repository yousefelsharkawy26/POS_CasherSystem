using POS_ModernUI.Models;
using Microsoft.EntityFrameworkCore;

namespace POS_ModernUI.Database.Context;
public class AppDbContext: DbContext
{
    public AppDbContext() { 
        if (!Database.CanConnect())
        {
            Database.Migrate(); // Ensure database is created and migrations are applied
        }
    }
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public virtual DbSet<Notification> Notifications { get; set; }
    public virtual DbSet<Product> Products { get; set; }
    public virtual DbSet<Supplier> Suppliers { get; set; }
    public virtual DbSet<SalesOrder> SalesOrders { get; set; }
    public virtual DbSet<SalesOrderDetail> SalesOrderDetails { get; set; }
    public virtual DbSet<PurchaseOrder> PurchaseOrders { get; set; }
    public virtual DbSet<PurchaseOrderDetail> PurchaseOrderDetails { get; set; }
    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<ImageCleaner> ImagePaths { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        //var config = new ConfigurationBuilder()
        //                .SetBasePath(Directory.GetCurrentDirectory())
        //                .AddJsonFile("AppSettings.json", optional: true, reloadOnChange: true)
        //                .Build();

        // Use SQL Server with a connection string
        var conn = "Server=.;Database=CasherSystem_Db_2;Trusted_Connection=True;TrustServerCertificate=True;";
        optionsBuilder.UseSqlServer(conn);

        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Add Seeding Data
        modelBuilder.Entity<User>().HasData([
            new() {Id = 1, Name = "Yousef", Password = "Admin"},
            new() {Id = 2, Name = "User", Password = "user123"}
            ]);
    }
}
