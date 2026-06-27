using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Pharmaceutical.Core;
using Pharmaceutical.Infrastructure;
using Pharmaceutical.Services;
using Pharmaceutical.WebAPI.Auth;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.Configure<PharmacySettings>(
    builder.Configuration.GetSection(PharmacySettings.SectionName));

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection 未配置。");

var mysqlVersion = builder.Configuration["Database:ServerVersion"] ?? "8.0.36";
builder.Services.AddDbContext<PharmaceuticalDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(Version.Parse(mysqlVersion))));

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("RedisConnection");
    options.InstanceName = "Pharmacy_";
});

builder.Services.AddScoped<DrugService>();

builder.Services.AddAuthentication(ApiKeyAuthenticationHandler.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
        ApiKeyAuthenticationHandler.SchemeName, _ => { });
builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/error", () => Results.Problem("服务器内部错误，请稍后重试。"))
    .ExcludeFromDescription();

app.Run();
