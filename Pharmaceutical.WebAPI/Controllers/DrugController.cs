using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Pharmaceutical.Core;
using Pharmaceutical.Services;
using Pharmaceutical.WebAPI.Auth;

namespace Pharmaceutical.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DrugController : ControllerBase
{
    private readonly DrugService _drugService;
    private readonly PharmacySettings _pharmacySettings;

    public DrugController(DrugService drugService, IOptions<PharmacySettings> pharmacySettings)
    {
        _drugService = drugService;
        _pharmacySettings = pharmacySettings.Value;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetDrugs()
    {
        var drugs = await _drugService.GetAllDrugsAsync();
        return Ok(drugs);
    }

    [HttpGet("low-stock")]
    [AllowAnonymous]
    public async Task<IActionResult> GetLowStockDrugs([FromQuery] int? threshold)
    {
        var effectiveThreshold = threshold ?? _pharmacySettings.LowStockThreshold;
        var drugs = await _drugService.GetLowStockDrugsAsync(effectiveThreshold);
        return Ok(drugs);
    }

    [HttpPost]
    [Authorize(AuthenticationSchemes = ApiKeyAuthenticationHandler.SchemeName)]
    public async Task<IActionResult> CreateDrug([FromBody] DrugCatalogEntity model)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var result = await _drugService.AddDrugAsync(model);
        return MapResult(result, successStatus: Ok(new { success = true, message = result.Message }));
    }

    [HttpDelete("{id}")]
    [Authorize(AuthenticationSchemes = ApiKeyAuthenticationHandler.SchemeName)]
    public async Task<IActionResult> DeleteDrug(string id)
    {
        var result = await _drugService.DeleteDrugAsync(id);
        return MapResult(result, successStatus: Ok(new { success = true, message = result.Message }));
    }

    private IActionResult MapResult(DrugOperationResult result, IActionResult successStatus)
    {
        if (result.Success)
        {
            return successStatus;
        }

        return result.Error switch
        {
            DrugOperationError.ValidationFailed => BadRequest(result.Message),
            DrugOperationError.DuplicateKey => Conflict(result.Message),
            DrugOperationError.NotFound => NotFound(result.Message),
            _ => StatusCode(StatusCodes.Status500InternalServerError, result.Message)
        };
    }
}
