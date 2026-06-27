using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pharmaceutical.Core
{
    // ✅ 强制将实体映射到数据库里真实的表名 'drugs'
    [Table("drugs")]
    public class DrugCatalogEntity
    {
        // ✅ 主键对齐数据库中的 drug_id
        [Key]
        [Column("drug_id")]
        public string DrugId { get; set; } = null!; 

        [Required]
        [Column("drug_name")]
        public string DrugName { get; set; } = null!; 

        [Column("trade_name")]
        public string TradeName { get; set; } = null!; 

        [Column("specification")]
        public string Specification { get; set; } = null!; 

        [Column("dosage_form")]
        public string DosageForm { get; set; } = null!; 

        [Column("approval_num")]
        public string ApprovalNum { get; set; } = null!; 

        [Column("storage_cond")]
        public string StorageCond { get; set; } = null!; 

        [Column("purchase_price")]
        public decimal PurchasePrice { get; set; } 

        [Column("retail_price")]
        public decimal RetailPrice { get; set; }

        [Column("stock_quantity")]
        public int StockQuantity { get; set; } 

        [Column("supplier_id")]
        public int SupplierId { get; set; }
    }
}