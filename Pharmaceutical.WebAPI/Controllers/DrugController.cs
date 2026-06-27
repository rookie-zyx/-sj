using Microsoft.AspNetCore.Mvc;
using Pharmaceutical.Core;
using Pharmaceutical.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pharmaceutical.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // 访问路径会被解析为: api/drug
    public class DrugController : ControllerBase
    {
        private readonly DrugService _drugService;

        // 构造函数注入：系统会自动把我们在 Program.cs 里注册的 DrugService 传进来
        public DrugController(DrugService drugService)
        {
            _drugService = drugService;
        }

        /// <summary>
        /// 查询药品字典：GET /api/drug
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<DrugCatalogEntity>>> GetDrugs()
        {
            var result = await _drugService.GetAllDrugsAsync();
            return Ok(result);
        }

        /// <summary>
        /// 新增药品：POST /api/drug
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateDrug([FromBody] DrugCatalogEntity drug)
        {
            var success = await _drugService.AddDrugAsync(drug);
            if (!success)
            {
                return BadRequest("药品数据不合规，添加失败。");
            }
            return Ok(new { message = "药品录入成功！" });
        }
    }
}