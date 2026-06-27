using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
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
        private readonly IDistributedCache _cache; // 注入分布式缓存接口
        private const string CacheKey = "AllDrugs";

        public DrugService(PharmaceuticalDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
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
        /// 新增药品（带缓存双写一致性策略）
        /// </summary>
        public async Task<bool> AddDrugAsync(DrugCatalogEntity drug)
        {
            if (string.IsNullOrWhiteSpace(drug.DrugName)) return false;

            await _context.Drugs.AddAsync(drug);
            var result = await _context.SaveChangesAsync() > 0;

            if (result)
            {
                // 核心安全策略：数据库变更了，直接删除旧缓存（清空旧缓存，下次查询自动更新）
                await _cache.RemoveAsync(CacheKey);
            }

            return result;
        }
    }
}