# ER 图（实体关系）

```mermaid
erDiagram
    suppliers ||--o{ drugs : supplies
    suppliers ||--o{ purchase_orders : receives
    drugs ||--o{ drug_batches : has
    drugs ||--o{ stock_transactions : logs
    drugs ||--o{ purchase_order_lines : orders
    purchase_orders ||--o{ purchase_order_lines : contains

    suppliers {
        int supplier_id PK
        string name
        string contact_person
        string phone
        string address
    }

    drugs {
        string drug_id PK
        string drug_name
        decimal purchase_price
        decimal retail_price
        int stock_quantity
        int supplier_id FK
        bool is_active
    }

    drug_batches {
        int batch_id PK
        string drug_id FK
        string batch_number
        datetime expiry_date
        int quantity
    }

    stock_transactions {
        int transaction_id PK
        string drug_id FK
        string transaction_type
        int quantity
        string operator_name
        datetime created_at
    }

    purchase_orders {
        int order_id PK
        int supplier_id FK
        string status
        string created_by
        datetime created_at
    }

    purchase_order_lines {
        int line_id PK
        int order_id FK
        string drug_id FK
        int quantity
        int received_quantity
    }

    audit_logs {
        long audit_id PK
        string entity_type
        string entity_id
        string action
        string user_name
        text old_values
        text new_values
        datetime created_at
    }
```

## 说明

- **库存一致性**：`drugs.stock_quantity` 仅通过 `stock_transactions` 与批次扣减联动变更，编辑药品不可直接改库存。
- **FEFO**：出库时按 `drug_batches.expiry_date` 先到期先扣减。
- **软下架**：`drugs.is_active = false` 表示下架，非物理删除。
