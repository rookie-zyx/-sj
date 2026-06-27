using Microsoft.EntityFrameworkCore;
using Pharmaceutical.Infrastructure;
using Pharmaceutical.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// ==================== 【新加的核心代码段】 ====================
// 1. 读取 appsettings.json 中的 MySQL 连接字符串
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. 将 EF Core 数据库上下文注入到容器中（注意补上你本地 appsettings.json 里的密码）
builder.Services.AddDbContext<PharmaceuticalDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
// 注册 Redis 分布式缓存服务
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("RedisConnection");
    options.InstanceName = "Pharmacy_"; // 缓存 Key 的前缀，防止多系统冲突
});
// 3. 将你的业务层 DrugService 注入到容器中，这样 Controller 才能正常调用它
builder.Services.AddScoped<DrugService>();
// ==============================================================

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();