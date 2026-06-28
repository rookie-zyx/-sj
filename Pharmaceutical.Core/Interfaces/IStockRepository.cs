namespace Pharmaceutical.Core.Interfaces;

public interface IStockRepository
{
    Task<List<StockTransactionEntity>> GetAllAsync();
    Task<bool> ProcessTransactionAsync(string drugId, string transactionType, int quantity, string operatorName, string? remark);
}
