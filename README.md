# 药品管理系统升级版

基于 .NET 8 的药品库存管理系统，包含 Web API 后端与 Blazor Server 前端。

## 技术栈

| 组件 | 技术 |
|------|------|
| 后端 | ASP.NET Core Web API |
| 前端 | Blazor Server |
| 数据库 | MySQL 8.x |
| 缓存 | Redis |
| ORM | EF Core (Pomelo) |

## 环境要求

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- MySQL 8.x（数据库名 `db2`）
- Redis（默认 `127.0.0.1:6379`）

## 快速开始

### 1. 初始化数据库

在 MySQL 中执行初始化脚本：

```bash
mysql -u root -p < scripts/init-database.sql
```

或手动创建 `db2` 数据库及 `drugs` 表（脚本见 `scripts/init-database.sql`）。

### 2. 配置连接

复制示例配置并填入本地密码：

```bash
# WebAPI
copy Pharmaceutical.WebAPI\appsettings.Development.example.json Pharmaceutical.WebAPI\appsettings.Development.json

# Blazor 前端
copy 前端页面\Pharmaceutical.Blazor\appsettings.Development.example.json 前端页面\Pharmaceutical.Blazor\appsettings.Development.json
```

编辑 `Pharmaceutical.WebAPI\appsettings.Development.json`，将 `YOUR_MYSQL_PASSWORD` 替换为本地 MySQL 密码。

前后端 `ApiSettings:ApiKey` 必须保持一致（开发环境默认为 `dev-pharmacy-api-key`）。

### 3. 启动 Redis

确保 Redis 在 `127.0.0.1:6379` 运行。Redis 不可用时系统会自动降级到 MySQL 查询。

### 4. 启动后端 API

```bash
cd Pharmaceutical.WebAPI
dotnet run
```

- API 地址：`http://localhost:5246`
- Swagger 文档：`http://localhost:5246/swagger`

### 5. 启动 Blazor 前端

```bash
cd 前端页面\Pharmaceutical.Blazor
dotnet run
```

- 前端地址：`http://localhost:5100`

## 项目结构

```
Pharmaceutical.Core/          # 实体与共享配置
Pharmaceutical.Infrastructure/ # EF Core DbContext
Pharmaceutical.Services/       # 业务逻辑（含 Redis 缓存）
Pharmaceutical.WebAPI/         # REST API
前端页面/Pharmaceutical.Blazor/ # Blazor Server UI
scripts/init-database.sql      # 数据库初始化脚本
```

## API 接口

| 方法 | 路径 | 说明 | 认证 |
|------|------|------|------|
| GET | `/api/Drug` | 获取全部药品 | 否 |
| GET | `/api/Drug/low-stock` | 获取低库存药品 | 否 |
| POST | `/api/Drug` | 新增药品 | 需要 `X-Api-Key` |
| DELETE | `/api/Drug/{id}` | 删除药品 | 需要 `X-Api-Key` |

## 配置说明

| 配置项 | 说明 | 默认值 |
|--------|------|--------|
| `ConnectionStrings:DefaultConnection` | MySQL 连接字符串 | 见 appsettings |
| `ConnectionStrings:RedisConnection` | Redis 连接 | `127.0.0.1:6379` |
| `ApiSettings:ApiKey` | API 密钥（保护写操作） | 开发环境见 Development.json |
| `PharmacySettings:LowStockThreshold` | 低库存预警阈值 | `200` |
| `ApiSettings:BaseUrl` | 前端调用 API 地址 | `http://localhost:5246/` |

## 安全说明

- 生产环境请勿将密码和 API Key 提交到 Git
- `appsettings.Development.json` 已在 `.gitignore` 中排除
- 写操作（新增/删除）需携带 `X-Api-Key` 请求头

## 一键构建

```bash
dotnet build Pharmaceutical.sln
```
