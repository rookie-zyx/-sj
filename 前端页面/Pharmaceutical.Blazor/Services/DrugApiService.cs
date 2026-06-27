using System.Net.Http.Json;
using Pharmaceutical.Core;

namespace Pharmaceutical.Blazor.Services;

public class DrugApiService
{
    private readonly HttpClient _httpClient;

    public DrugApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<DrugCatalogEntity>> GetAllDrugsAsync()
    {
        var drugs = await _httpClient.GetFromJsonAsync<List<DrugCatalogEntity>>("api/drug");
        return drugs ?? new List<DrugCatalogEntity>();
    }

    public async Task<(bool Success, string Message)> AddDrugAsync(DrugCatalogEntity drug)
    {
        var response = await _httpClient.PostAsJsonAsync("api/drug", drug);

        if (response.IsSuccessStatusCode)
        {
            return (true, "药品录入成功！");
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

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound
            || response.StatusCode == System.Net.HttpStatusCode.MethodNotAllowed)
        {
            return (false, "后端暂未提供下架接口，请联系管理员补充 DELETE api/drug/{id}。");
        }

        var error = await response.Content.ReadAsStringAsync();
        return (false, string.IsNullOrWhiteSpace(error) ? "下架操作失败。" : error);
    }

    public async Task<List<DrugCatalogEntity>> GetLowStockDrugsAsync(int threshold = 200)
    {
        var drugs = await GetAllDrugsAsync();
        return drugs.Where(d => d.StockQuantity < threshold).ToList();
    }
}
