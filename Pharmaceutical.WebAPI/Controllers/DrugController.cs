using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Pharmaceutical.Core.DTOs;
using Pharmaceutical.Core.Interfaces;
using Pharmaceutical.Core.Settings;

namespace Pharmaceutical.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DrugController : ControllerBase
{
    private readonly IDrugService _drugService;
    private readonly AlertSettings _alertSettings;

    public DrugController(IDrugService drugService, IOptions<AlertSettings> alertSettings)
    {
        _drugService = drugService;
        _alertSettings = alertSettings.Value;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetDrugs([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _drugService.GetPagedAsync(search, page, pageSize);
        return Ok(ApiResponse<PagedResult<DrugDto>>.Ok(result));
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDrug(string id)
    {
        var drug = await _drugService.GetByIdAsync(id);
        if (drug == null) return NotFound(ApiResponse<DrugDto>.Fail($"未找到编号为 {id} 的药品"));
        return Ok(ApiResponse<DrugDto>.Ok(drug));
    }

    [HttpGet("low-stock")]
    [AllowAnonymous]
    public async Task<IActionResult> GetLowStock([FromQuery] int? threshold)
    {
        var effectiveThreshold = threshold ?? _alertSettings.LowStockThreshold;
        var drugs = await _drugService.GetLowStockAsync(effectiveThreshold);
        return Ok(ApiResponse<List<DrugDto>>.Ok(drugs));
    }

    [HttpGet("alert-settings")]
    [AllowAnonymous]
    public IActionResult GetAlertSettings()
    {
        return Ok(ApiResponse<AlertSettings>.Ok(_alertSettings));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Operator")]
    public async Task<IActionResult> CreateDrug([FromBody] DrugCreateDto model)
    {
        var result = await _drugService.AddAsync(model);
        if (!result) return BadRequest(ApiResponse<object>.Fail("药品录入失败，请检查数据是否合法或编号是否重复。"));
        return Ok(ApiResponse<object>.Ok(null, "药品录入成功"));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Operator")]
    public async Task<IActionResult> UpdateDrug(string id, [FromBody] DrugUpdateDto model)
    {
        var result = await _drugService.UpdateAsync(id, model);
        if (!result) return BadRequest(ApiResponse<object>.Fail("药品更新失败，请检查数据或药品是否存在。"));
        return Ok(ApiResponse<object>.Ok(null, "药品更新成功"));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteDrug(string id)
    {
        try
        {
            var result = await _drugService.DeleteAsync(id);
            if (!result) return NotFound(ApiResponse<object>.Fail($"未找到编号为 {id} 的药品"));
            return Ok(ApiResponse<object>.Ok(null, "药品下架成功"));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<object>.Fail(ex.Message));
        }
    }
}
