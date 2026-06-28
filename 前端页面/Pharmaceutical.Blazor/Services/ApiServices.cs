using System.Net.Http.Json;
using Pharmaceutical.Core.DTOs;
using Pharmaceutical.Core.Settings;

namespace Pharmaceutical.Blazor.Services;

public class AuthApiService
{
    private readonly HttpClient _httpClient;
    private readonly AuthStateService _authState;

    public AuthApiService(HttpClient httpClient, AuthStateService authState)
    {
        _httpClient = httpClient;
        _authState = authState;
    }

    public async Task<(bool Success, string Message)> LoginAsync(string username, string password)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/login", new LoginDto
        {
            Username = username,
            Password = password
        });

        var (success, data, message) = await ApiClientHelper.GetApiData<LoginResponseDto>(response);
        if (success && data != null)
        {
            await _authState.SetAuthAsync(data);
            return (true, message);
        }

        return (false, message ?? "登录失败");
    }

    public async Task LogoutAsync() => await _authState.ClearAsync();

    public async Task<(bool Success, string Message)> RegisterAsync(RegisterDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/register", dto);
        var (success, _, message) = await ApiClientHelper.GetApiData<object>(response);
        return (success, message ?? (success ? "用户创建成功" : "创建失败"));
    }
}

public class DrugApiService
{
    private readonly HttpClient _httpClient;

    public DrugApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<(PagedResult<DrugDto>? Data, string? Error)> GetPagedAsync(string? search, int page, int pageSize, bool? activeOnly = true)
    {
        var url = $"api/drug?page={page}&pageSize={pageSize}&activeOnly={activeOnly}";
        if (!string.IsNullOrWhiteSpace(search))
            url += $"&search={Uri.EscapeDataString(search)}";

        var response = await _httpClient.GetAsync(url);
        var (success, data, message) = await ApiClientHelper.GetApiData<PagedResult<DrugDto>>(response);
        return success ? (data, null) : (null, message);
    }

    public async Task<List<DrugDto>> GetLowStockDrugsAsync(int? threshold = null)
    {
        var url = threshold.HasValue ? $"api/drug/low-stock?threshold={threshold.Value}" : "api/drug/low-stock";
        var response = await _httpClient.GetAsync(url);
        var (success, data, _) = await ApiClientHelper.GetApiData<List<DrugDto>>(response);
        return success ? data ?? new() : new();
    }

    public async Task<AlertSettings?> GetAlertSettingsAsync()
    {
        var response = await _httpClient.GetAsync("api/drug/alert-settings");
        var (success, data, _) = await ApiClientHelper.GetApiData<AlertSettings>(response);
        return success ? data : null;
    }

    public async Task<(bool Success, string Message)> AddDrugAsync(DrugCreateDto drug)
    {
        var response = await _httpClient.PostAsJsonAsync("api/drug", drug);
        var (success, _, message) = await ApiClientHelper.GetApiData<object>(response);
        return (success, message ?? (success ? "录入成功" : "录入失败"));
    }

    public async Task<(bool Success, string Message)> UpdateDrugAsync(string drugId, DrugUpdateDto drug)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/drug/{Uri.EscapeDataString(drugId)}", drug);
        var (success, _, message) = await ApiClientHelper.GetApiData<object>(response);
        return (success, message ?? (success ? "更新成功" : "更新失败"));
    }

    public async Task<(bool Success, string Message)> DeleteDrugAsync(string drugId)
    {
        var response = await _httpClient.DeleteAsync($"api/drug/{Uri.EscapeDataString(drugId)}");
        var (success, _, message) = await ApiClientHelper.GetApiData<object>(response);
        return (success, message ?? (success ? "下架成功" : "下架失败"));
    }
}

public class SupplierApiService
{
    private readonly HttpClient _httpClient;

    public SupplierApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<SupplierDto>> GetAllAsync()
    {
        var response = await _httpClient.GetAsync("api/supplier");
        var (success, data, _) = await ApiClientHelper.GetApiData<List<SupplierDto>>(response);
        return success ? data ?? new() : new();
    }

    public async Task<(bool Success, string Message)> AddAsync(SupplierCreateDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync("api/supplier", dto);
        var (success, _, message) = await ApiClientHelper.GetApiData<object>(response);
        return (success, message ?? (success ? "创建成功" : "创建失败"));
    }

    public async Task<(bool Success, string Message)> UpdateAsync(int id, SupplierUpdateDto dto)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/supplier/{id}", dto);
        var (success, _, message) = await ApiClientHelper.GetApiData<object>(response);
        return (success, message ?? (success ? "更新成功" : "更新失败"));
    }

    public async Task<(bool Success, string Message)> DeleteAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"api/supplier/{id}");
        var (success, _, message) = await ApiClientHelper.GetApiData<object>(response);
        return (success, message ?? (success ? "删除成功" : "删除失败"));
    }
}

public class StockApiService
{
    private readonly HttpClient _httpClient;

    public StockApiService(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<(PagedResult<StockTransactionDto>? Data, string? Error)> GetTransactionsAsync(StockTransactionQueryDto query)
    {
        var url = $"api/stock?page={query.Page}&pageSize={query.PageSize}";
        if (!string.IsNullOrWhiteSpace(query.DrugId)) url += $"&drugId={Uri.EscapeDataString(query.DrugId)}";
        if (!string.IsNullOrWhiteSpace(query.TransactionType)) url += $"&transactionType={query.TransactionType}";
        var response = await _httpClient.GetAsync(url);
        var (success, data, message) = await ApiClientHelper.GetApiData<PagedResult<StockTransactionDto>>(response);
        return success ? (data, null) : (null, message);
    }

    public async Task<List<ExpiryAlertDto>> GetExpiryAlertsAsync(int withinDays = 90)
    {
        var response = await _httpClient.GetAsync($"api/stock/expiry-alerts?withinDays={withinDays}");
        var (success, data, _) = await ApiClientHelper.GetApiData<List<ExpiryAlertDto>>(response);
        return success ? data ?? new() : new();
    }

    public async Task<(bool Success, string Message)> StockInAsync(StockInDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync("api/stock/in", dto);
        var (success, _, message) = await ApiClientHelper.GetApiData<object>(response);
        return (success, message ?? (success ? "入库成功" : "入库失败"));
    }

    public async Task<(bool Success, string Message)> StockOutAsync(StockOutDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync("api/stock/out", dto);
        var (success, _, message) = await ApiClientHelper.GetApiData<object>(response);
        return (success, message ?? (success ? "出库成功" : "出库失败"));
    }
}

public class DashboardApiService
{
    private readonly HttpClient _httpClient;
    public DashboardApiService(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<(DashboardDto? Data, string? Error)> GetDashboardAsync()
    {
        var response = await _httpClient.GetAsync("api/dashboard");
        var (success, data, message) = await ApiClientHelper.GetApiData<DashboardDto>(response);
        return success ? (data, null) : (null, message);
    }
}

public class PurchaseOrderApiService
{
    private readonly HttpClient _httpClient;
    public PurchaseOrderApiService(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<List<PurchaseOrderDto>> GetAllAsync()
    {
        var response = await _httpClient.GetAsync("api/purchaseorder");
        var (success, data, _) = await ApiClientHelper.GetApiData<List<PurchaseOrderDto>>(response);
        return success ? data ?? new() : new();
    }

    public async Task<(bool Success, string Message)> CreateFromLowStockAsync(int supplierId)
    {
        var response = await _httpClient.PostAsync($"api/purchaseorder/from-low-stock?supplierId={supplierId}", null);
        var (success, _, message) = await ApiClientHelper.GetApiData<object>(response);
        return (success, message ?? "");
    }

    public async Task SubmitAsync(int id) => await _httpClient.PostAsync($"api/purchaseorder/{id}/submit", null);
    public async Task ApproveAsync(int id) => await _httpClient.PostAsync($"api/purchaseorder/{id}/approve", null);
    public async Task ReceiveAsync(int id) => await _httpClient.PostAsync($"api/purchaseorder/{id}/receive", null);
}

public class ExportApiService
{
    private readonly HttpClient _httpClient;

    public ExportApiService(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<byte[]?> DownloadDrugsAsync()
    {
        var response = await _httpClient.GetAsync("api/export/drugs");
        return response.IsSuccessStatusCode ? await response.Content.ReadAsByteArrayAsync() : null;
    }

    public async Task<byte[]?> DownloadTransactionsAsync(StockTransactionQueryDto query)
    {
        var url = $"api/export/transactions?page={query.Page}&pageSize={query.PageSize}";
        if (!string.IsNullOrWhiteSpace(query.DrugId)) url += $"&drugId={Uri.EscapeDataString(query.DrugId)}";
        if (!string.IsNullOrWhiteSpace(query.TransactionType)) url += $"&transactionType={query.TransactionType}";
        var response = await _httpClient.GetAsync(url);
        return response.IsSuccessStatusCode ? await response.Content.ReadAsByteArrayAsync() : null;
    }

    public async Task<byte[]?> DownloadLowStockAsync()
    {
        var response = await _httpClient.GetAsync("api/export/low-stock");
        return response.IsSuccessStatusCode ? await response.Content.ReadAsByteArrayAsync() : null;
    }
}

public class AuditApiService
{
    private readonly HttpClient _httpClient;

    public AuditApiService(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<List<AuditLogDto>> GetRecentAsync(int count = 100)
    {
        var response = await _httpClient.GetAsync($"api/audit?count={count}");
        var (success, data, _) = await ApiClientHelper.GetApiData<List<AuditLogDto>>(response);
        return success ? data ?? new() : new();
    }
}
