using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Pharmaceutical.Core;
namespace Pharmaceutical.Blazor.Services;

public class DrugApiService
{
    private readonly HttpClient _httpClient;
    private readonly PharmacySettings _pharmacySettings;

    public DrugApiService(HttpClient httpClient, IOptions<PharmacySettings> pharmacySettings)
    {
        _httpClient = httpClient;
        _pharmacySettings = pharmacySettings.Value;
    }

    public async Task<List<DrugCatalogEntity>> GetAllDrugsAsync()
    {
        var drugs = await _httpClient.GetFromJsonAsync<List<DrugCatalogEntity>>("api/drug");
        return drugs ?? new List<DrugCatalogEntity>();
    }

    public async Task<List<DrugCatalogEntity>> GetLowStockDrugsAsync(int? threshold = null)
    {
        var effectiveThreshold = threshold ?? _pharmacySettings.LowStockThreshold;
        var drugs = await _httpClient.GetFromJsonAsync<List<DrugCatalogEntity>>(
            $"api/drug/low-stock?threshold={effectiveThreshold}");
        return drugs ?? new List<DrugCatalogEntity>();
    }

    public async Task<(bool Success, string Message)> AddDrugAsync(DrugCatalogEntity drug)
    {
        var response = await _httpClient.PostAsJsonAsync("api/drug", drug);

        if (response.IsSuccessStatusCode)
        {
            return (true, "药品录入成功！");
        }

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            return (false, "API 认证失败，请检查 ApiSettings:ApiKey 配置是否与后端一致。");
        }

        var error = await response.Content.ReadAsStringAsync();
        return (false, string.IsNullOrWhiteSpace(error) ? "药品数据不合规，添加失败。" : error);
    }

    public async Task<(bool Success, string Message)> DeleteDrugAsync(string drugId)
    {
        var response = await _httpClient.DeleteAsync($"api/drug/{Uri.EscapeDataString(drugId)}");

        if (response.IsSuccessStatusCode)
        {
            return (true, "药品已成功下架。");
        }

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return (false, $"未找到编号为 {drugId} 的药品。");
        }

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            return (false, "API 认证失败，请检查 ApiSettings:ApiKey 配置是否与后端一致。");
        }

        var error = await response.Content.ReadAsStringAsync();
        return (false, string.IsNullOrWhiteSpace(error) ? "下架操作失败。" : error);
    }
}
