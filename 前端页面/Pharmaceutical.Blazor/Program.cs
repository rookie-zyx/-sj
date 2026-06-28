using Pharmaceutical.Blazor.Components;
using Pharmaceutical.Blazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<AuthStateService>();
builder.Services.AddLogging();
builder.Services.AddTransient<JwtAuthorizationHandler>();

var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"]
    ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");

builder.Services.AddHttpClient<AuthApiService>(client => client.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<DrugApiService>(client => client.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<JwtAuthorizationHandler>();
builder.Services.AddHttpClient<SupplierApiService>(client => client.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<JwtAuthorizationHandler>();
builder.Services.AddHttpClient<StockApiService>(client => client.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<JwtAuthorizationHandler>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
