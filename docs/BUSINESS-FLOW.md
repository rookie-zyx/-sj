# 业务流程图

## 进销存主流程

```mermaid
flowchart TD
    subgraph inbound [入库侧]
        A[录入供应商] --> B[录入药品台账]
        B --> C[入库并指定批号效期]
        C --> D[写入drug_batches]
        C --> E[写入stock_transactions IN]
    end

    subgraph outbound [出库侧]
        F[出库请求] --> G{FEFO扣减批次}
        G --> H[更新库存数量]
        H --> I[写入stock_transactions OUT]
    end

    subgraph purchase [采购闭环]
        J[低库存预警] --> K[一键生成采购单]
        K --> L[Operator提交]
        L --> M[Admin审核]
        M --> N[确认收货]
        N --> C
    end

    subgraph alert [三维预警]
        P[低库存阈值] --> J
        Q[效期30/60/90天] --> R[效期预警页]
        S[动销分析] --> T[智能补货建议]
    end
```

## 角色与权限

```mermaid
flowchart LR
    Admin[Admin] --> All[全部功能]
    Admin --> Users[用户管理]
    Admin --> Approve[采购审核]
    Admin --> Audit[审计日志]
    Operator[Operator] --> Operate[录入编辑入库出库采购]
    Viewer[Viewer] --> Read[只读查询]
```

## 演示自检路径

1. 登录 http://localhost:5100（admin / Admin@123）
2. 运营仪表盘 — 查看 KPI 与图表
3. 效期预警 — 确认临期批次
4. 低库存预警 — 导出 CSV
5. 采购单 — 生成 → 提交 → 审核 → 收货
6. 入库出库 — 流水筛选与导出
7. 审计日志（Admin）— 查看变更记录
