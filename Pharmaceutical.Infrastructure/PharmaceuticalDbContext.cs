using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Pharmaceutical.Core;

namespace Pharmaceutical.Infrastructure;

public class PharmaceuticalDbContext : IdentityDbContext<AppUser>
{
    public PharmaceuticalDbContext(DbContextOptions<PharmaceuticalDbContext> options)
        : base(options)
    {
    }

    public DbSet<DrugCatalogEntity> Drugs { get; set; }
    public DbSet<SupplierEntity> Suppliers { get; set; }
    public DbSet<StockTransactionEntity> StockTransactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<DrugCatalogEntity>(entity =>
        {
            entity.ToTable("drugs");
            entity.HasKey(e => e.DrugId);
            entity.Property(e => e.DrugId).HasMaxLength(50);
            entity.Property(e => e.DrugName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.TradeName).HasMaxLength(200);
            entity.Property(e => e.Specification).HasMaxLength(200);
            entity.Property(e => e.DosageForm).HasMaxLength(100);
            entity.Property(e => e.ApprovalNum).HasMaxLength(100);
            entity.Property(e => e.StorageCond).HasMaxLength(200);
            entity.Property(e => e.PurchasePrice).HasPrecision(10, 2);
            entity.Property(e => e.RetailPrice).HasPrecision(10, 2);
        });

        modelBuilder.Entity<SupplierEntity>(entity =>
        {
            entity.ToTable("suppliers");
            entity.HasKey(e => e.SupplierId);
            entity.Property(e => e.SupplierId).ValueGeneratedOnAdd();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.ContactPerson).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.Address).HasMaxLength(500);
        });

        modelBuilder.Entity<StockTransactionEntity>(entity =>
        {
            entity.ToTable("stock_transactions");
            entity.HasKey(e => e.TransactionId);
            entity.Property(e => e.TransactionId).ValueGeneratedOnAdd();
            entity.HasOne(e => e.Drug)
                .WithMany()
                .HasForeignKey(e => e.DrugId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.TransactionType).HasMaxLength(10).IsRequired();
            entity.Property(e => e.Operator).HasMaxLength(100);
            entity.Property(e => e.Remark).HasMaxLength(500);
        });
    }
}
