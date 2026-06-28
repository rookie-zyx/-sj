using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Pharmaceutical.Core.DTOs;
using Pharmaceutical.Core.Interfaces;

namespace Pharmaceutical.Services;

public class StockService : IStockService
{
    private readonly IStockRepository _repository;
    private readonly IDistributedCache _cache;
    private readonly ILogger<StockService> _logger;
    private const string StockCacheKey = "StockTransactions";
    private const string DrugCacheKey = "AllDrugs";

    public StockService(IStockRepository repository, IDistributedCache cache, ILogger<StockService> logger)
    {
        _repository = repository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<List<StockTransactionDto>> GetAllTransactionsAsync()
    {
        var transactions = await _repository.GetAllAsync();
        return transactions.Select(t => new StockTransactionDto
        {
            TransactionId = t.TransactionId,
            DrugId = t.DrugId,
            DrugName = t.Drug?.DrugName ?? string.Empty,
            TransactionType = t.TransactionType,
            Quantity = t.Quantity,
            Operator = t.Operator,
            CreatedAt = t.CreatedAt,
            Remark = t.Remark
        }).ToList();
    }

    public async Task<bool> StockInAsync(StockInDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.DrugId) || dto.Quantity <= 0) return false;

        try
        {
            var result = await _repository.ProcessTransactionAsync(
                dto.DrugId, "IN", dto.Quantity, dto.Operator ?? "系统", dto.Remark);
            if (result) await InvalidateCacheAsync();
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "入库失败: {DrugId}", dto.DrugId);
            return false;
        }
    }

    public async Task<bool> StockOutAsync(StockOutDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.DrugId) || dto.Quantity <= 0) return false;

        try
        {
            var result = await _repository.ProcessTransactionAsync(
                dto.DrugId, "OUT", dto.Quantity, dto.Operator ?? "系统", dto.Remark);
            if (result) await InvalidateCacheAsync();
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "出库失败: {DrugId}", dto.DrugId);
            return false;
        }
    }

    private async Task InvalidateCacheAsync()
    {
        try
        {
            await _cache.RemoveAsync(StockCacheKey);
            await _cache.RemoveAsync(DrugCacheKey);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis 缓存清理失败");
        }
    }
}
