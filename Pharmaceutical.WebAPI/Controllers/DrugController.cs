using Microsoft.VisualBasic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using Pharmaceutical.Core;
using Pharmaceutical.Services;

namespace Pharmaceutical.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // 💡 契约路由：api/Drug
    public class DrugController : ControllerBase
    {
        private readonly DrugService _drugService;
        private readonly IDistributedCache _cache;
        private const string CacheKey = "AllDrugs"; // Redis 缓存键名

        // 构造函数注入服务和 Redis 缓存
        public DrugController(DrugService drugService, IDistributedCache cache)
        {
            _drugService = drugService;
            _cache = cache;
        }

        /// <summary>
        /// 1. 查询全量药品 (带 Redis 缓存保护)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetDrugs()
        {
            // 这个方法你之前已经跑通了，它会去调用 DrugService
            var drugs = await _drugService.GetAllDrugsAsync();
            return Ok(drugs);
        }

        /// <summary>
        /// 2. 录入新药品 (POST api/Drug)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateDrug([FromBody] DrugCatalogEntity model)
        {
            if (model == null) return BadRequest("药品数据不能为空");

            // 调用服务层写入 MySQL
            var result = await _drugService.AddDrugAsync(model);
            if (!result) return StatusCode(500, "数据库写入失败");

            // 🔥 PM 强力要求：数据变动了，必须立刻斩断旧缓存，确保 Blazor 前端刷新时看到最新数据！
            try
            {
                await _cache.RemoveAsync(CacheKey);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Redis 缓存清理异常]: {ex.Message}");
            }

            return Ok(new { success = true, message = "药品录入成功，缓存已同步刷新！" });
        }

        /// <summary>
        /// 3. 下架/删除药品 (DELETE api/Drug/{id})
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDrug(string id)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest("药品编号不能为空");

            // 调用服务层从 MySQL 删除
            var result = await _drugService.DeleteDrugAsync(id);
            if (!result) return NotFound($"未找到编号为 {id} 的药品或删除失败");

            // 🔥 同步强刷 Redis 缓存
            try
            {
                await _cache.RemoveAsync(CacheKey);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Redis 缓存清理异常]: {ex.Message}");
            }

            return Ok(new { success = true, message = "药品下架成功，缓存已同步刷新！" });
        }
    }
}