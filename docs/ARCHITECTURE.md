# 药品管理系统 — 架构与答辩文档

## 系统架构

```
Blazor Server (UI) ──JWT──▶ WebAPI ──▶ Services ──▶ Repositories ──▶ MySQL
                              │                      │
                              └── Redis (缓存)        └── 审计拦截器
```

## 创新点（三智一合规）

1. **智预警**：低库存 + 多档效期 + 智能补货三维预警
2. **智出库**：FEFO 批次先到期先出，降低过期损耗
3. **智分析**：运营仪表盘（库存估值、动销 Top10、补货建议）
4. **合规追溯**：库存流水 + EF 变更审计日志

## 核心业务流程

低库存预警 → 一键生成采购单 → Admin 审核 → 收货入库（自动写批次+流水）→ FEFO 出库

## API 清单（新增）

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | /api/dashboard | 运营仪表盘 |
| GET | /api/stock/expiry-alerts | 效期预警 |
| GET | /api/purchaseorder | 采购单列表 |
| POST | /api/purchaseorder/from-low-stock | 从低库存生成 PO |
| POST | /api/purchaseorder/{id}/approve | 审核 |
| POST | /api/purchaseorder/{id}/receive | 收货入库 |
| GET | /api/export/drugs | 导出 CSV |
| GET | /api/audit | 审计日志（Admin） |

## 角色权限

- **Admin**：全部 + 用户管理 + 审核采购单 + 下架药品
- **Operator**：录入/编辑/入库出库/创建采购单
- **Viewer**：只读

## Docker 部署

```powershell
copy .env.example .env
docker compose up -d --build
```

访问 http://localhost:5100 ，默认 admin / Admin@123
