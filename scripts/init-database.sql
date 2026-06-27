-- 药品管理系统数据库初始化脚本
-- 适用于 MySQL 8.x

CREATE DATABASE IF NOT EXISTS db2 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE db2;

CREATE TABLE IF NOT EXISTS drugs (
    drug_id        VARCHAR(50)  NOT NULL PRIMARY KEY,
    drug_name      VARCHAR(200) NOT NULL,
    trade_name     VARCHAR(200) NULL,
    specification  VARCHAR(200) NULL,
    dosage_form    VARCHAR(100) NULL,
    approval_num   VARCHAR(100) NULL,
    storage_cond   VARCHAR(200) NULL,
    purchase_price DECIMAL(10, 2) NOT NULL DEFAULT 0,
    retail_price   DECIMAL(10, 2) NOT NULL DEFAULT 0,
    stock_quantity INT NOT NULL DEFAULT 0,
    supplier_id    INT NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

INSERT INTO drugs (drug_id, drug_name, trade_name, specification, dosage_form, approval_num, storage_cond, purchase_price, retail_price, stock_quantity, supplier_id)
VALUES
    ('H20203021', '阿莫西林胶囊', '阿莫仙', '0.25g*24粒/盒', '胶囊剂', '国药准字H12345678', '遮光、密封保存', 12.50, 18.00, 350, 1),
    ('H20203022', '布洛芬缓释胶囊', '芬必得', '0.3g*20粒/盒', '胶囊剂', '国药准字H87654321', '密封保存', 15.00, 22.50, 120, 2),
    ('H20203023', '维生素C片', '维C银翘', '0.1g*100片/瓶', '片剂', '国药准字H11223344', '遮光、干燥处保存', 8.00, 12.00, 80, 1)
ON DUPLICATE KEY UPDATE drug_name = VALUES(drug_name);
