using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharmaceutical.Core.DTOs;
using Pharmaceutical.Core.Interfaces;
using System.Security.Claims;

namespace Pharmaceutical.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StockController : ControllerBase
{
    private readonly IStockService _stockService;

    public StockController(IStockService stockService)
    {
        _stockService = stockService;
    }

    [HttpGet]
    public async Task<IActionResult> GetTransactions([FromQuery] StockTransactionQueryDto query)
    {
        var result = await _stockService.GetTransactionsAsync(query);
        return Ok(ApiResponse<PagedResult<StockTransactionDto>>.Ok(result));
    }

    [HttpGet("batches/{drugId}")]
    public async Task<IActionResult> GetBatches(string drugId)
    {
        var batches = await _stockService.GetBatchesAsync(drugId);
        return Ok(ApiResponse<List<DrugBatchDto>>.Ok(batches));
    }

    [HttpGet("expiry-alerts")]
    public async Task<IActionResult> GetExpiryAlerts([FromQuery] int withinDays = 90)
    {
        var alerts = await _stockService.GetExpiryAlertsAsync(withinDays);
        return Ok(ApiResponse<List<ExpiryAlertDto>>.Ok(alerts));
    }

    [HttpPost("in")]
    [Authorize(Roles = "Admin,Operator")]
    public async Task<IActionResult> StockIn([FromBody] StockInDto model)
    {
        var operatorName = User.FindFirstValue(ClaimTypes.Name) ?? "unknown";
        var result = await _stockService.StockInAsync(model, operatorName);
        if (!result) return BadRequest(ApiResponse<object>.Fail("入库失败，请检查药品编号与数量"));
        return Ok(ApiResponse<object>.Ok(null, "入库成功"));
    }

    [HttpPost("out")]
    [Authorize(Roles = "Admin,Operator")]
    public async Task<IActionResult> StockOut([FromBody] StockOutDto model)
    {
        var operatorName = User.FindFirstValue(ClaimTypes.Name) ?? "unknown";
        var result = await _stockService.StockOutAsync(model, operatorName);
        if (!result) return BadRequest(ApiResponse<object>.Fail("出库失败，请检查库存是否充足"));
        return Ok(ApiResponse<object>.Ok(null, "出库成功"));
    }
}
