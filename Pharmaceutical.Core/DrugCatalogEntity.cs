using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pharmaceutical.Core;

[Table("drugs")]
public class DrugCatalogEntity
{
    [Key]
    [Required(ErrorMessage = "药品编号不能为空")]
    [MaxLength(50)]
    [Column("drug_id")]
    public string DrugId { get; set; } = null!;

    [Required(ErrorMessage = "药品名称不能为空")]
    [MaxLength(200)]
    [Column("drug_name")]
    public string DrugName { get; set; } = null!;

    [MaxLength(200)]
    [Column("trade_name")]
    public string? TradeName { get; set; }

    [MaxLength(200)]
    [Column("specification")]
    public string? Specification { get; set; }

    [MaxLength(100)]
    [Column("dosage_form")]
    public string? DosageForm { get; set; }

    [MaxLength(100)]
    [Column("approval_num")]
    public string? ApprovalNum { get; set; }

    [MaxLength(200)]
    [Column("storage_cond")]
    public string? StorageCond { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "采购价不能为负数")]
    [Column("purchase_price")]
    public decimal PurchasePrice { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "零售价不能为负数")]
    [Column("retail_price")]
    public decimal RetailPrice { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "库存数量不能为负数")]
    [Column("stock_quantity")]
    public int StockQuantity { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "供应商 ID 必须大于 0")]
    [Column("supplier_id")]
    public int SupplierId { get; set; }
}
