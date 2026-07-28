using Handwerker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Handwerker.Infrastructure.Data;

public class HandwerkerDbContext(DbContextOptions<HandwerkerDbContext> options) : 
    DbContext(options)
{
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<Offer> Offers => Set<Offer>();
    public DbSet<Recipient> Recipients => Set<Recipient>();
    public DbSet<Provider> Providers => Set<Provider>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Bank> Banks => Set<Bank>();
    public DbSet<AppSettings> AppSettings => Set<AppSettings>();
    public DbSet<UserSettings> UserSettings => Set<UserSettings>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<ServiceItem> ServiceItems => Set<ServiceItem>();
    public DbSet<WageType> WageTypes => Set<WageType>();

    // Neue Entities für Artikelstamm und Lager
    public DbSet<Article> Articles => Set<Article>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<ArticleWarehouse> ArticleWarehouses => Set<ArticleWarehouse>();
    public DbSet<InventoryMovement> InventoryMovements => Set<InventoryMovement>();

    // Neue Entities für Auftragsverwaltung
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<WorkTimeEntry> WorkTimeEntries => Set<WorkTimeEntry>();
    public DbSet<OrderMaterial> OrderMaterials => Set<OrderMaterial>();
    public DbSet<OrderOffer> OrderOffers => Set<OrderOffer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Unique Index auf UserId für UserSettings
        modelBuilder.Entity<UserSettings>()
            .HasIndex(us => us.UserId)
            .IsUnique();

        // Index für Notifications (Performance bei Abfragen nach UserId und IsRead)
        modelBuilder.Entity<Notification>()
            .HasIndex(n => new { n.UserId, n.IsRead });

        // Index für Sortierung nach CreatedAt
        modelBuilder.Entity<Notification>()
            .HasIndex(n => n.CreatedAt);

        // Artikel-Konfiguration
        modelBuilder.Entity<Article>()
            .HasIndex(a => a.ArticleNumber)
            .IsUnique();

        modelBuilder.Entity<Article>()
            .HasIndex(a => a.Name);

        modelBuilder.Entity<Article>()
            .HasIndex(a => a.IsActive);

        // Warehouse-Konfiguration
        modelBuilder.Entity<Warehouse>()
            .HasIndex(w => w.IsActive);

        // Leistungen-Konfiguration
        modelBuilder.Entity<ServiceItem>()
            .HasIndex(s => s.ServiceNumber)
            .IsUnique();

        modelBuilder.Entity<ServiceItem>()
            .HasIndex(s => s.Name);

        modelBuilder.Entity<ServiceItem>()
            .HasIndex(s => s.IsActive);

        // Lohnarten-Konfiguration
        modelBuilder.Entity<WageType>()
            .HasIndex(w => w.WageNumber)
            .IsUnique();

        modelBuilder.Entity<WageType>()
            .HasIndex(w => w.Name);

        modelBuilder.Entity<WageType>()
            .HasIndex(w => w.IsActive);

        // ArticleWarehouse (Many-to-Many mit Payload) Konfiguration
        modelBuilder.Entity<ArticleWarehouse>()
            .HasKey(aw => new { aw.ArticleId, aw.WarehouseId });

        modelBuilder.Entity<ArticleWarehouse>()
            .HasIndex(aw => aw.StockQuantity);

        modelBuilder.Entity<ArticleWarehouse>()
            .HasOne(aw => aw.Article)
            .WithMany(a => a.ArticleWarehouses)
            .HasForeignKey(aw => aw.ArticleId);

        modelBuilder.Entity<ArticleWarehouse>()
            .HasOne(aw => aw.Warehouse)
            .WithMany(w => w.ArticleWarehouses)
            .HasForeignKey(aw => aw.WarehouseId);

        // InventoryMovement Konfiguration
        modelBuilder.Entity<InventoryMovement>()
            .HasIndex(im => im.ArticleId);

        modelBuilder.Entity<InventoryMovement>()
            .HasIndex(im => im.WarehouseId);

        modelBuilder.Entity<InventoryMovement>()
            .HasIndex(im => im.Type);

        modelBuilder.Entity<InventoryMovement>()
            .HasIndex(im => im.CreatedAt);

        modelBuilder.Entity<InventoryMovement>()
            .HasIndex(im => new { im.ReferenceType, im.ReferenceId });

        modelBuilder.Entity<InventoryMovement>()
            .HasOne(im => im.Article)
            .WithMany(a => a.InventoryMovements)
            .HasForeignKey(im => im.ArticleId);

        modelBuilder.Entity<InventoryMovement>()
            .HasOne(im => im.Warehouse)
            .WithMany()
            .HasForeignKey(im => im.WarehouseId);

        // Order Konfiguration
        modelBuilder.Entity<Order>()
            .HasIndex(o => o.OrderNumber)
            .IsUnique();

        modelBuilder.Entity<Order>()
            .HasIndex(o => o.Status);

        modelBuilder.Entity<Order>()
            .HasIndex(o => o.Priority);

        modelBuilder.Entity<Order>()
            .HasIndex(o => o.OrderDate);

        modelBuilder.Entity<Order>()
            .HasIndex(o => o.CustomerNumber);

        modelBuilder.Entity<Order>()
            .HasIndex(o => o.InvoiceId);

        // WorkTimeEntry Konfiguration
        modelBuilder.Entity<WorkTimeEntry>()
            .HasIndex(w => w.OrderId);

        modelBuilder.Entity<WorkTimeEntry>()
            .HasIndex(w => w.UserId);

        modelBuilder.Entity<WorkTimeEntry>()
            .HasIndex(w => w.Date);

        modelBuilder.Entity<WorkTimeEntry>()
            .HasOne(w => w.Order)
            .WithMany(o => o.WorkTimeEntries)
            .HasForeignKey(w => w.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // OrderMaterial Konfiguration
        modelBuilder.Entity<OrderMaterial>()
            .HasIndex(om => om.OrderId);

        modelBuilder.Entity<OrderMaterial>()
            .HasIndex(om => om.ArticleId);

        modelBuilder.Entity<OrderMaterial>()
            .HasIndex(om => om.WarehouseId);

        modelBuilder.Entity<OrderMaterial>()
            .HasOne(om => om.Order)
            .WithMany(o => o.Materials)
            .HasForeignKey(om => om.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrderMaterial>()
            .HasOne(om => om.Article)
            .WithMany()
            .HasForeignKey(om => om.ArticleId);

        modelBuilder.Entity<OrderMaterial>()
            .HasOne(om => om.Warehouse)
            .WithMany()
            .HasForeignKey(om => om.WarehouseId);

        // OrderOffer Konfiguration (Many-to-Many)
        modelBuilder.Entity<OrderOffer>()
            .HasKey(oo => new { oo.OrderId, oo.OfferId });

        modelBuilder.Entity<OrderOffer>()
            .HasOne(oo => oo.Order)
            .WithMany(o => o.SourceOffers)
            .HasForeignKey(oo => oo.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrderOffer>()
            .HasOne(oo => oo.Offer)
            .WithMany()
            .HasForeignKey(oo => oo.OfferId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
