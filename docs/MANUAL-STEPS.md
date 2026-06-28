# 手动操作详细步骤

项目路径：`D:\study\药品管理系统\-sj`  
推荐运行方式：**Docker**（本机无 .NET 8 SDK 时不要用 `dotnet run`）。

---

## 阶段 A：环境与首次启动（必做）

### A1 进入目录

```powershell
cd "D:\study\药品管理系统\-sj"
```

### A2 配置 `.env`

```powershell
copy .env.example .env
```

编辑 `.env`：

| 变量 | 说明 |
|------|------|
| `MYSQL_ROOT_PASSWORD` | MySQL 强密码 |
| `JWT_SECRET` | 至少 32 字符随机串 |
| `SEED_DEMO_DATA` | 首次建议 `true`（自动演示数据） |

`.env` 不会提交 Git。

### A3 检查端口

默认占用：5100（前端）、5246（API）、3306（MySQL）、6379（Redis）。冲突时停本机服务或改 `docker-compose.yml` 端口映射。

### A4 启动

```powershell
docker compose up -d --build
docker compose ps
docker compose logs webapi --tail 30
```

### A5 访问

| 地址 | 用途 |
|------|------|
| http://localhost:5100 | **Blazor 前端（用这个）** |
| http://localhost:5246/health | API 健康检查 |

### A6 登录

| 账号 | 密码 | 角色 |
|------|------|------|
| `admin` | `Admin@123` | Admin |
| `operator1` | `Operator@123` | Operator（种子自动创建） |
| `viewer1` | `Viewer@123` | Viewer（种子自动创建） |

---

## 阶段 B：演示数据与采购闭环

### 自动种子内容

`SEED_DEMO_DATA=true` 且空库时自动写入：

- 2 家供应商、5 种药品、批次、出库流水
- 演示账号 operator1 / viewer1
- 若有低库存且无采购单：1 张**草稿采购单**

### 手动走通采购（录屏用）

1. 库存预警 → 查看低库存
2. 采购单 → 生成或打开草稿 → **提交** → Admin **审核** → **收货入库**
3. 入库出库 → 查看流水
4. 效期预警 → 30/60/90 天
5. 运营仪表盘 → KPI 与图表

### 重新种子（清空所有数据）

```powershell
docker compose down -v
docker compose up -d --build
```

---

## 阶段 C：演示前自检

- [ ] 未登录访问 `/drugs` 跳转登录
- [ ] 编辑药品不能改库存
- [ ] CSV 导出可用
- [ ] Admin 可见审计日志（编辑药品后有记录）
- [ ] Operator / Viewer 权限差异正确

---

## 阶段 D：答辩材料

| 文档 | 用途 |
|------|------|
| [ARCHITECTURE.md](ARCHITECTURE.md) | 架构与创新点 |
| [ER-DIAGRAM.md](ER-DIAGRAM.md) | ER 图 |
| [BUSINESS-FLOW.md](BUSINESS-FLOW.md) | 业务流程 |
| [答辩素材.md](答辩素材.md) | 录屏脚本 |
| [PPT-OUTLINE.md](PPT-OUTLINE.md) | PPT 大纲 |

截图建议 7 张：首页、仪表盘、效期、低库存、采购单、流水、审计日志。

---

## 常见问题

**Q：`dotnet run` 失败** → 用 Docker。  
**Q：5246 打不开页面** → 用 5100。  
**Q：MySQL 密码错误** → `docker compose down -v` 后重建。  
**Q：仪表盘为 0** → 检查种子或手动录入。  
**Q：采购单为空** → 采购单页点「生成」或设 `SEED_DEMO_DATA=true` 重启 webapi。

---

## 代码备份

```powershell
git status
git push origin master
```

推送后 GitHub Actions 自动 build + test。
