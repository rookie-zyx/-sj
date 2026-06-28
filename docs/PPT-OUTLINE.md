# 答辩 PPT 建议大纲（约 15～20 页）

## 1. 封面

- 题目：药品管理系统（进销存 + 智能预警）
- 姓名、学号、指导教师、日期

## 2. 研究背景与意义

- 小型药店库存管理痛点：效期损耗、断货、账实不符
- 目标：安全可用的进销存 mini 系统

## 3. 需求分析

- 功能：台账、入库出库、预警、采购、分析
- 非功能：JWT 权限、Docker 部署、审计追溯

## 4. 系统架构

- 引用 [ARCHITECTURE.md](ARCHITECTURE.md) 分层图
- Blazor + WebAPI + MySQL + Redis

## 5. 数据库设计

- 引用 [ER-DIAGRAM.md](ER-DIAGRAM.md)
- 核心表：drugs、drug_batches、stock_transactions、purchase_orders

## 6. 业务流程

- 引用 [BUSINESS-FLOW.md](BUSINESS-FLOW.md)
- 低库存 → 采购单 → 收货 → FEFO 出库

## 7. 创新点（三智一合规）

1. 智预警：低库存 + 效期 + 补货建议
2. 智出库：FEFO 批次策略
3. 智分析：运营仪表盘 + Chart.js
4. 合规追溯：流水 + 审计日志

## 8～14. 功能演示截图

按 [答辩素材.md](答辩素材.md) 截图清单插入：

- 仪表盘、效期预警、采购单、流水、审计日志等

## 15. 安全设计

- JWT 三角色、路由守卫、操作人绑定、库存一致性

## 16. 测试与部署

- 单元测试、Docker Compose、CI

## 17. 总结与展望

- 已完成：进销存闭环 + 三维预警 + 审计
- 展望：扫码入库、移动端（可选）

## 18. 致谢
