using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pharmaceutical.Core
{
    // ✅ 1. 强制将实体映射到数据库里真实的表名 'drugs'，而不是默认的 't_drug_catalog'
    [Table("drugs")]
    public class DrugCatalogEntity
    {
        // ✅ 2. 主键是字符串类型的药品编号 (如 'H20203021')，对齐数据库中的 drug_id
        [Key]
        [Column("drug_id")]
        public string DrugId { get; set; }

        [Required]
        [Column("drug_name")]
        public string DrugName { get; set; }

        [Column("trade_name")]
        public string TradeName { get; set; } // 商品名

        [Column("specification")]
        public string Specification { get; set; } // 规格

        [Column("dosage_form")]
        public string DosageForm { get; set; } // 剂型

        [Column("approval_num")]
        public string ApprovalNum { get; set; } // 批准文号

        [Column("storage_cond")]
        public string StorageCond { get; set; } // 储存条件

        [Column("purchase_price")]
        public decimal PurchasePrice { get; set; } // 采购价

        [Column("retail_price")]
        public decimal RetailPrice { get; set; } // 零售价

        [Column("stock_quantity")]
        public int StockQuantity { get; set; } // 当前库存数量

        [Column("supplier_id")]
        public int SupplierId { get; set; } // 供应商ID
    }
}