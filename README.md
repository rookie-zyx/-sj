# 药品管理系统升级版

基于 .NET 8 的药品管理系统，采用分层架构：Core → Infrastructure → Services → WebAPI，前端为 Blazor Server。

## 功能模块

- 药品台账：录入、编辑、下架、分页搜索
- 库存预警：低库存药品监控（可配置阈值）
- 供应商管理：供应商 CRUD
- 入库 / 出库：库存流水与事务一致性
- 用户认证：JWT + 角色权限（Admin / Operator / Viewer）

## 环境要求

- .NET 8 SDK
- MySQL 8.0+
- Redis 6+

## 本地运行

### 1. 配置连接字符串

编辑 `Pharmaceutical.WebAPI/appsettings.Development.json`，或使用 User Secrets：

```bash
cd Pharmaceutical.WebAPI
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "server=127.0.0.1;user=root;database=pharmaceutical;port=3306;password=YOUR_PASSWORD;"
```

### 2. 数据库迁移

```bash
dotnet ef database update --project Pharmaceutical.Infrastructure --startup-project Pharmaceutical.WebAPI
```

应用启动时会自动执行迁移并创建默认管理员账号。

### 3. 启动服务

需同时启动 WebAPI 与 Blazor：

| 服务 | 端口 | 命令 |
|------|------|------|
| WebAPI | http://localhost:5246 | `dotnet run --project Pharmaceutical.WebAPI` |
| Blazor | http://localhost:5100 | `dotnet run --project 前端页面/Pharmaceutical.Blazor` |

VS Code 可使用 **WebAPI + Blazor** 复合调试配置一键启动。

### 4. 默认账号

- 用户名：`admin`
- 密码：`Admin@123`
- 角色：Admin

## Docker 部署

```bash
cp .env.example .env
# 编辑 .env 填入密码
docker compose up -d
```

服务地址：

- WebAPI: http://localhost:5246
- Blazor: http://localhost:5100
- 健康检查: http://localhost:5246/health

## 项目结构

```
Pharmaceutical.Core/          # 实体、DTO、接口
Pharmaceutical.Infrastructure/ # EF Core、Repository、Migrations
Pharmaceutical.Services/      # 业务逻辑
Pharmaceutical.WebAPI/        # REST API
前端页面/Pharmaceutical.Blazor/ # Blazor Server UI
Pharmaceutical.Tests/         # 单元测试与集成测试
```

## API 文档

开发环境下访问 Swagger：http://localhost:5246/swagger

## 测试

```bash
dotnet test Pharmaceutical.Tests
```
