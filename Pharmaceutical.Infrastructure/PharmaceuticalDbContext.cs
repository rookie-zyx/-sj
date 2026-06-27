using Microsoft.EntityFrameworkCore;
using Pharmaceutical.Core;

namespace Pharmaceutical.Infrastructure;

public class PharmaceuticalDbContext : DbContext
{
    public PharmaceuticalDbContext(DbContextOptions<PharmaceuticalDbContext> options)
        : base(options)
    {
    }

    public DbSet<DrugCatalogEntity> Drugs { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<DrugCatalogEntity>(entity =>
        {
            entity.ToTable("drugs");
            entity.HasKey(e => e.DrugId);

            entity.Property(e => e.DrugId).HasMaxLength(50).IsRequired();
            entity.Property(e => e.DrugName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.TradeName).HasMaxLength(200);
            entity.Property(e => e.Specification).HasMaxLength(200);
            entity.Property(e => e.DosageForm).HasMaxLength(100);
            entity.Property(e => e.ApprovalNum).HasMaxLength(100);
            entity.Property(e => e.StorageCond).HasMaxLength(200);
            entity.Property(e => e.PurchasePrice).HasPrecision(10, 2);
            entity.Property(e => e.RetailPrice).HasPrecision(10, 2);
        });
    }
}
