namespace Pharmaceutical.Core.DTOs;

public class StockTransactionDto
{
    public int TransactionId { get; set; }
    public string DrugId { get; set; } = null!;
    public string DrugName { get; set; } = string.Empty;
    public string TransactionType { get; set; } = null!;
    public int Quantity { get; set; }
    public string Operator { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? Remark { get; set; }
}

public class StockInDto
{
    public string DrugId { get; set; } = null!;
    public int Quantity { get; set; }
    public string? Operator { get; set; }
    public string? Remark { get; set; }
}

public class StockOutDto
{
    public string DrugId { get; set; } = null!;
    public int Quantity { get; set; }
    public string? Operator { get; set; }
    public string? Remark { get; set; }
}
