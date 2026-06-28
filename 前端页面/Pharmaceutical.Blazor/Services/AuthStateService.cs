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
    private readonly TaskCompletionSource _initTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private string? _token;
    private string? _username;
    private string? _displayName;
    private List<string> _roles = new();
    private bool _initialized;

    public event Func<Task>? Unauthorized;

    public Task WhenInitialized => _initTcs.Task;
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
            _logger.LogWarning(ex, "Failed to restore auth state");
        }
        finally
        {
            _initTcs.TrySetResult();
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
        catch { /* ignore */ }
    }

    public async Task NotifyUnauthorizedAsync()
    {
        await ClearAsync();
        if (Unauthorized != null) await Unauthorized.Invoke();
    }

    private async Task PersistAsync()
    {
        try
        {
            var data = new AuthData { Token = _token, Username = _username, DisplayName = _displayName, Roles = _roles };
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "pharm_auth", JsonSerializer.Serialize(data));
        }
        catch { /* ignore */ }
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
        await _authState.WhenInitialized;

        if (!string.IsNullOrEmpty(_authState.Token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authState.Token);

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            await _authState.NotifyUnauthorizedAsync();

        return response;
    }
}

public static class ApiClientHelper
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public static async Task<(bool Success, T? Data, string Message)> GetApiData<T>(HttpResponseMessage response)
    {
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            return (false, default, "登录已过期，请重新登录");

        var content = await response.Content.ReadAsStringAsync();

        if (response.StatusCode == System.Net.HttpStatusCode.BadRequest
            && content.Contains("\"errors\"", StringComparison.OrdinalIgnoreCase))
        {
            var validationMessage = TryParseValidationErrors(content);
            if (!string.IsNullOrEmpty(validationMessage))
                return (false, default, validationMessage);
        }

        try
        {
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<T>>(content, JsonOptions);
            if (apiResponse == null) return (false, default, "无法解析服务器响应");
            return (apiResponse.Success, apiResponse.Data, apiResponse.Message ?? string.Empty);
        }
        catch
        {
            return (false, default, "无法解析服务器响应");
        }
    }

    private static string? TryParseValidationErrors(string content)
    {
        try
        {
            using var doc = JsonDocument.Parse(content);
            if (!doc.RootElement.TryGetProperty("errors", out var errors)) return null;
            var messages = new List<string>();
            foreach (var property in errors.EnumerateObject())
                foreach (var error in property.Value.EnumerateArray())
                    if (error.GetString() is { } m) messages.Add(m);
            return messages.Count > 0 ? string.Join("; ", messages) : null;
        }
        catch { return null; }
    }
}
