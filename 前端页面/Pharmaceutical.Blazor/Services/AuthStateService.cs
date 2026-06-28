using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.JSInterop;
using Pharmaceutical.Core.DTOs;

namespace Pharmaceutical.Blazor.Services;

public class AuthStateService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ILogger<AuthStateService> _logger;
    private string? _token;
    private string? _username;
    private string? _displayName;
    private List<string> _roles = new();
    private bool _initialized;

    public string? Token => _token;
    public string? Username => _username;
    public string? DisplayName => _displayName;
    public IReadOnlyList<string> Roles => _roles;
    public bool IsAuthenticated => !string.IsNullOrEmpty(_token);

    public bool IsAdmin => _roles.Contains("Admin");
    public bool CanOperate => IsAdmin || _roles.Contains("Operator");

    public AuthStateService(IJSRuntime jsRuntime, ILogger<AuthStateService> logger)
    {
        _jsRuntime = jsRuntime;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        if (_initialized) return;
        _initialized = true;
        try
        {
            var json = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", "pharm_auth");
            if (json != null)
            {
                var data = JsonSerializer.Deserialize<AuthData>(json);
                if (data != null)
                {
                    _token = data.Token;
                    _username = data.Username;
                    _displayName = data.DisplayName;
                    _roles = data.Roles ?? new();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to restore auth state from localStorage");
        }
    }

    public async Task SetAuthAsync(LoginResponseDto response)
    {
        _token = response.Token;
        _username = response.Username;
        _displayName = response.DisplayName;
        _roles = response.Roles;
        await PersistAsync();
    }

    public async Task ClearAsync()
    {
        _token = null;
        _username = null;
        _displayName = null;
        _roles = new();
        try { await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "pharm_auth"); }
        catch { /* ignore during SSR/prerender */ }
    }

    private async Task PersistAsync()
    {
        try
        {
            var data = new AuthData { Token = _token, Username = _username, DisplayName = _displayName, Roles = _roles };
            var json = JsonSerializer.Serialize(data);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "pharm_auth", json);
        }
        catch { /* ignore during SSR/prerender */ }
    }

    private class AuthData
    {
        public string? Token { get; set; }
        public string? Username { get; set; }
        public string? DisplayName { get; set; }
        public List<string>? Roles { get; set; }
    }
}

public class JwtAuthorizationHandler : DelegatingHandler
{
    private readonly AuthStateService _authState;

    public JwtAuthorizationHandler(AuthStateService authState)
    {
        _authState = authState;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(_authState.Token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authState.Token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}

public static class ApiClientHelper
{
    public static async Task<(bool Success, T? Data, string Message)> GetApiData<T>(HttpResponseMessage response)
    {
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<T>>();
        if (apiResponse == null)
            return (false, default, "无法解析服务器响应");

        return (apiResponse.Success, apiResponse.Data, apiResponse.Message ?? string.Empty);
    }
}
