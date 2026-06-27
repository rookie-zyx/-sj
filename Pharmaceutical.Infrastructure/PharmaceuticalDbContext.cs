using Microsoft.EntityFrameworkCore;
using Pharmaceutical.Core;

namespace Pharmaceutical.Infrastructure
{
    public class PharmaceuticalDbContext : DbContext
    {
        public PharmaceuticalDbContext(DbContextOptions<PharmaceuticalDbContext> options)
            : base(options)
        {
        }

        // 定义对应的数据库表映射
        public DbSet<DrugCatalogEntity> Drugs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. 配置主键和表名
            modelBuilder.Entity<DrugCatalogEntity>(entity =>
            {
                entity.ToTable("drugs");
                entity.HasKey(e => e.DrugId);

            });
        }
    }
}