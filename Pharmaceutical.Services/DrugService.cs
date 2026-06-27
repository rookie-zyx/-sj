using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Pharmaceutical.Core;
using Pharmaceutical.Infrastructure;

namespace Pharmaceutical.Services;

public class DrugService
{
    private readonly PharmaceuticalDbContext _context;
    private readonly IDistributedCache _cache;
    private readonly ILogger<DrugService> _logger;
    private const string CacheKey = "AllDrugs";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public DrugService(
        PharmaceuticalDbContext context,
        IDistributedCache cache,
        ILogger<DrugService> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    public async Task<List<DrugCatalogEntity>> GetAllDrugsAsync()
    {
        try
        {
            var cachedData = await _cache.GetStringAsync(CacheKey);
            if (!string.IsNullOrEmpty(cachedData))
            {
                return JsonSerializer.Deserialize<List<DrugCatalogEntity>>(cachedData)
                    ?? new List<DrugCatalogEntity>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis 读取失败，降级至 MySQL 查询");
        }

        var drugs = await _context.Drugs.AsNoTracking().ToListAsync();

        try
        {
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheDuration
            };
            await _cache.SetStringAsync(CacheKey, JsonSerializer.Serialize(drugs), cacheOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis 写入失败，仅返回数据库结果");
        }

        return drugs;
    }

    public async Task<List<DrugCatalogEntity>> GetLowStockDrugsAsync(int threshold)
    {
        var drugs = await GetAllDrugsAsync();
        return drugs.Where(d => d.StockQuantity < threshold).ToList();
    }

    public async Task<DrugOperationResult> AddDrugAsync(DrugCatalogEntity drug)
    {
        if (drug == null
            || string.IsNullOrWhiteSpace(drug.DrugId)
            || string.IsNullOrWhiteSpace(drug.DrugName))
        {
            return DrugOperationResult.Fail("药品编号和名称不能为空", DrugOperationError.ValidationFailed);
        }

        if (drug.PurchasePrice < 0 || drug.RetailPrice < 0 || drug.StockQuantity < 0)
        {
            return DrugOperationResult.Fail("价格或库存不能为负数", DrugOperationError.ValidationFailed);
        }

        if (drug.SupplierId <= 0)
        {
            return DrugOperationResult.Fail("供应商 ID 必须大于 0", DrugOperationError.ValidationFailed);
        }

        try
        {
            var exists = await _context.Drugs.AnyAsync(x => x.DrugId == drug.DrugId);
            if (exists)
            {
                return DrugOperationResult.Fail($"药品编号 {drug.DrugId} 已存在", DrugOperationError.DuplicateKey);
            }

            await _context.Drugs.AddAsync(drug);
            await _context.SaveChangesAsync();
            await InvalidateCacheAsync();

            return DrugOperationResult.Ok("药品录入成功");
        }
        catch (DbUpdateException ex) when (IsDuplicateKeyException(ex))
        {
            _logger.LogWarning(ex, "药品编号重复: {DrugId}", drug.DrugId);
            return DrugOperationResult.Fail($"药品编号 {drug.DrugId} 已存在", DrugOperationError.DuplicateKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "新增药品失败: {DrugId}", drug.DrugId);
            return DrugOperationResult.Fail("数据库写入失败", DrugOperationError.DatabaseError);
        }
    }

    public async Task<DrugOperationResult> DeleteDrugAsync(string drugId)
    {
        if (string.IsNullOrWhiteSpace(drugId))
        {
            return DrugOperationResult.Fail("药品编号不能为空", DrugOperationError.ValidationFailed);
        }

        try
        {
            var drug = await _context.Drugs.FirstOrDefaultAsync(x => x.DrugId == drugId);
            if (drug == null)
            {
                return DrugOperationResult.Fail($"未找到编号为 {drugId} 的药品", DrugOperationError.NotFound);
            }

            _context.Drugs.Remove(drug);
            await _context.SaveChangesAsync();
            await InvalidateCacheAsync();

            return DrugOperationResult.Ok("药品下架成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除药品失败: {DrugId}", drugId);
            return DrugOperationResult.Fail("删除操作失败", DrugOperationError.DatabaseError);
        }
    }

    private async Task InvalidateCacheAsync()
    {
        try
        {
            await _cache.RemoveAsync(CacheKey);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis 缓存清理失败");
        }
    }

    private static bool IsDuplicateKeyException(DbUpdateException ex)
    {
        var message = ex.InnerException?.Message ?? ex.Message;
        return message.Contains("Duplicate", StringComparison.OrdinalIgnoreCase);
    }
}
