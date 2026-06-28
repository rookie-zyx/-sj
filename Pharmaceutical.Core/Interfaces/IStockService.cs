using Pharmaceutical.Core.DTOs;

namespace Pharmaceutical.Core.Interfaces;

public interface IStockService
{
    Task<List<StockTransactionDto>> GetAllTransactionsAsync();
    Task<bool> StockInAsync(StockInDto dto);
    Task<bool> StockOutAsync(StockOutDto dto);
}
