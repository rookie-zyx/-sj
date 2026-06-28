using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharmaceutical.Core.DTOs;
using Pharmaceutical.Core.Interfaces;

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
    [AllowAnonymous]
    public async Task<IActionResult> GetTransactions()
    {
        var transactions = await _stockService.GetAllTransactionsAsync();
        return Ok(ApiResponse<List<StockTransactionDto>>.Ok(transactions));
    }

    [HttpPost("in")]
    [Authorize(Roles = "Admin,Operator")]
    public async Task<IActionResult> StockIn([FromBody] StockInDto model)
    {
        var result = await _stockService.StockInAsync(model);
        if (!result) return BadRequest(ApiResponse<object>.Fail("入库失败，请检查药品编号与数量"));
        return Ok(ApiResponse<object>.Ok(null, "入库成功"));
    }

    [HttpPost("out")]
    [Authorize(Roles = "Admin,Operator")]
    public async Task<IActionResult> StockOut([FromBody] StockOutDto model)
    {
        var result = await _stockService.StockOutAsync(model);
        if (!result) return BadRequest(ApiResponse<object>.Fail("出库失败，请检查库存是否充足"));
        return Ok(ApiResponse<object>.Ok(null, "出库成功"));
    }
}
