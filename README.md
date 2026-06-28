# 药品管理系统

基于 .NET 8 的药品进销存管理系统，采用分层架构：Core → Infrastructure → Services → WebAPI，前端为 Blazor Server。

## 功能模块

| 模块 | 说明 |
|------|------|
| **运营仪表盘** | 库存估值、低库存/临期统计、Top10 动销、智能补货建议 |
| **药品台账** | 录入、编辑、软下架、分页搜索、在售/已下架筛选、CSV 导出 |
| **库存预警** | 低库存监控，可跳转采购单、导出清单 |
| **效期预警** | 30/60/90 天临期批次，支撑 FEFO 策略 |
| **采购单** | 低库存一键生成 → 提交 → Admin 审核 → 收货入库 |
| **入库/出库** | 批次入库、FEFO 出库、流水分页筛选与 CSV 导出 |
| **供应商** | 供应商 CRUD，台账/预警/采购单关联展示 |
| **用户管理** | Admin 创建 Operator / Viewer 账号 |
| **审计日志** | Admin 查看 EF 变更审计（JSON diff） |

## 创新点（三智一合规）

1. **智预警**：低库存 + 多档效期 + 智能补货三维预警
2. **智出库**：FEFO 批次先到期先出
3. **智分析**：运营仪表盘与动销分析
4. **合规追溯**：库存流水 + EF 变更审计日志

## 环境要求

- .NET 8 SDK（本地开发）或 Docker（推荐）
- MySQL 8.0+
- Redis 6+

## Docker 部署（推荐）

```powershell
copy .env.example .env
# 编辑 .env 设置 MYSQL_ROOT_PASSWORD 和 JWT_SECRET（至少 32 字符）
docker compose up -d --build
```

| 服务 | 地址 |
|------|------|
| Blazor 前端 | http://localhost:5100 |
| WebAPI | http://localhost:5246 |
| 健康检查 | http://localhost:5246/health |

默认账号：`admin` / `Admin@123`

停止服务：`docker compose down` · 清除数据卷：`docker compose down -v`

## 本地运行

### 1. 配置

编辑 `Pharmaceutical.WebAPI/appsettings.Development.json` 或使用 User Secrets 设置数据库连接与 JWT 密钥。

### 2. 迁移

```bash
dotnet ef database update --project Pharmaceutical.Infrastructure --startup-project Pharmaceutical.WebAPI
```

### 3. 启动

| 服务 | 端口 | 命令 |
|------|------|------|
| WebAPI | http://localhost:5246 | `dotnet run --project Pharmaceutical.WebAPI` |
| Blazor | http://localhost:5100 | `dotnet run --project 前端页面/Pharmaceutical.Blazor` |

## 角色权限

| 角色 | 权限 |
|------|------|
| Admin | 全部功能 + 用户管理 + 审核采购单 + 下架药品 |
| Operator | 录入/编辑/入库出库/创建采购单 |
| Viewer | 只读 |

## 项目结构

```
Pharmaceutical.Core/           # 实体、DTO、接口
Pharmaceutical.Infrastructure/ # EF Core、Repository、Migrations、审计拦截器
Pharmaceutical.Services/         # 业务逻辑、分析、导出
Pharmaceutical.WebAPI/           # REST API
前端页面/Pharmaceutical.Blazor/  # Blazor Server UI
Pharmaceutical.Tests/            # 单元测试
docs/ARCHITECTURE.md             # 架构与答辩文档
```

## API 文档

开发环境 Swagger：http://localhost:5246/swagger

主要新增接口见 `docs/ARCHITECTURE.md`。

## 测试

```bash
dotnet test Pharmaceutical.Tests
```

CI 已配置 MySQL service container 用于集成测试。
