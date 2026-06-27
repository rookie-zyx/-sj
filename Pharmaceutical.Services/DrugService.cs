using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Pharmaceutical.Core;
using Pharmaceutical.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace Pharmaceutical.Services
{
    public class DrugService
    {
        private readonly PharmaceuticalDbContext _context;
        private readonly IDistributedCache _cache;
        private readonly ILogger<DrugService> _logger;
        private const string CacheKey = "AllDrugs";

        public DrugService(PharmaceuticalDbContext context, IDistributedCache cache, ILogger<DrugService> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// 获取所有药品列表（Redis 缓存防线版）
        /// </summary>
        public async Task<List<DrugCatalogEntity>> GetAllDrugsAsync()
        {
            // 1. 尝试从 Redis 缓存中读取数据
            var cachedData = await _cache.GetStringAsync(CacheKey);
            if (!string.IsNullOrEmpty(cachedData))
            {
                // 缓存命中：直接反序列化返回，不查数据库！
                return JsonSerializer.Deserialize<List<DrugCatalogEntity>>(cachedData) ?? new List<DrugCatalogEntity>();
            }

            // 2. 缓存未命中：穿透到 MySQL 数据库查询
            var drugs = await _context.Drugs.ToListAsync();

            // 3. 将查询结果写入 Redis，并设置 5 分钟的过期时间（防止数据永久积压）
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };

            var serializedData = JsonSerializer.Serialize(drugs);
            await _cache.SetStringAsync(CacheKey, serializedData, cacheOptions);

            return drugs;
        }

        /// <summary>
        /// 新增药品（带 MySQL 异常捕获与 Redis 缓存双写一致性策略）
        /// </summary>
        public async Task<bool> AddDrugAsync(DrugCatalogEntity drug)
        {
            // 1. 基础防错校验
            if (drug == null || string.IsNullOrWhiteSpace(drug.DrugName)) return false;

            try
            {
                // 2. 写入 MySQL 数据库
                await _context.Drugs.AddAsync(drug);
                var result = await _context.SaveChangesAsync() > 0;

                // 3. 如果数据库写入成功，立刻斩断 Redis 旧缓存
                if (result)
                {
                    try
                    {
                        // 核心安全策略：清空旧缓存，下次前端查询时会自动穿透并更新
                        await _cache.RemoveAsync("AllDrugs");
                    }
                    catch (Exception cacheEx)
                    {
                        // Redis 如果报错，记录日志但不阻止整个业务（高可用降级）
                        _logger.LogWarning(cacheEx, "Redis 缓存清理异常");
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                // 4. 捕获 MySQL 写入异常（如主键冲突、字段超长等）
                _logger.LogError(ex, "MySQL 写入异常");
                return false;
            }
        }

        /// <summary>
        /// 从 MySQL 删除指定药品
        /// </summary>
        public async Task<bool> DeleteDrugAsync(string drugId)
        {
            try
            {
                var drug = await _context.Drugs.FirstOrDefaultAsync(x => x.DrugId == drugId);
                if (drug == null) return false;

                _context.Drugs.Remove(drug);
                var rows = await _context.SaveChangesAsync();

                if (rows > 0)
                {
                    try
                    {
                        await _cache.RemoveAsync(CacheKey);
                    }
                    catch (Exception cacheEx)
                    {
                        _logger.LogWarning(cacheEx, "Redis 缓存清理异常");
                    }
                }

                return rows > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MySQL 删除异常");
                return false;
            }
        }
    }
}