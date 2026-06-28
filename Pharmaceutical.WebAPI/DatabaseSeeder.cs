using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Pharmaceutical.Core;
using Pharmaceutical.Infrastructure;
using MySqlConnector;

namespace Pharmaceutical.WebAPI;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PharmaceuticalDbContext>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        try
        {
            await context.Database.MigrateAsync();
        }
        catch (MySqlException ex) when (ex.Message.Contains("already exists"))
        {
            Console.WriteLine($"迁移提示：{ex.Message}，继续执行...");
        }

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        string[] roles = ["Admin", "Operator", "Viewer"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        var admin = await userManager.FindByNameAsync("admin");
        if (admin == null)
        {
            admin = new AppUser { UserName = "admin", DisplayName = "系统管理员" };
            await userManager.CreateAsync(admin, "Admin@123");
            await userManager.AddToRoleAsync(admin, "Admin");
        }

        var seedDemo = configuration.GetValue<bool>("SEED_DEMO_DATA")
            || string.Equals(Environment.GetEnvironmentVariable("SEED_DEMO_DATA"), "true", StringComparison.OrdinalIgnoreCase);

        if (seedDemo)
        {
            await SeedDemoUsersAsync(userManager);
            await SeedDemoDataAsync(context);
            await SeedDemoPurchaseOrderAsync(context);
        }
    }

    private static async Task SeedDemoUsersAsync(UserManager<AppUser> userManager)
    {
        await EnsureUserAsync(userManager, "operator1", "Operator@123", "演示操作员", "Operator");
        await EnsureUserAsync(userManager, "viewer1", "Viewer@123", "演示只读", "Viewer");
    }

    private static async Task EnsureUserAsync(
        UserManager<AppUser> userManager, string username, string password, string displayName, string role)
    {
        if (await userManager.FindByNameAsync(username) != null) return;
        var user = new AppUser { UserName = username, DisplayName = displayName };
        var result = await userManager.CreateAsync(user, password);
        if (result.Succeeded)
            await userManager.AddToRoleAsync(user, role);
    }

    private static async Task SeedDemoPurchaseOrderAsync(PharmaceuticalDbContext context)
    {
        if (await context.PurchaseOrders.AnyAsync()) return;

        var supplier = await context.Suppliers.OrderBy(s => s.SupplierId).FirstOrDefaultAsync();
        var lowStockDrugs = await context.Drugs
            .Where(d => d.IsActive && d.StockQuantity < 200 && d.SupplierId == supplier!.SupplierId)
            .Take(3)
            .ToListAsync();
        if (supplier == null || lowStockDrugs.Count == 0) return;

        var order = new PurchaseOrderEntity
        {
            SupplierId = supplier.SupplierId,
            Status = PurchaseOrderStatus.Draft,
            CreatedBy = "admin",
            CreatedAt = DateTime.UtcNow,
            Lines = lowStockDrugs.Select(d => new PurchaseOrderLineEntity
            {
                DrugId = d.DrugId,
                Quantity = Math.Max(200 - d.StockQuantity, 10),
                ReceivedQuantity = 0
            }).ToList()
        };
        context.PurchaseOrders.Add(order);
        await context.SaveChangesAsync();
        Console.WriteLine("演示数据：已创建草稿采购单（可在采购单页提交审核）。");
    }

    private static async Task SeedDemoDataAsync(PharmaceuticalDbContext context)
    {
        if (await context.Drugs.AnyAsync())
        {
            Console.WriteLine("演示数据：库中已有药品，跳过种子数据。");
            return;
        }

        Console.WriteLine("演示数据：正在写入供应商、药品、批次与流水...");

        var supplier1 = new SupplierEntity
        {
            Name = "华北医药供应有限公司",
            ContactPerson = "张经理",
            Phone = "010-88886666",
            Address = "北京市朝阳区医药产业园"
        };
        var supplier2 = new SupplierEntity
        {
            Name = "华南康宁药业",
            ContactPerson = "李主管",
            Phone = "020-66668888",
            Address = "广州市白云区物流园"
        };
        context.Suppliers.AddRange(supplier1, supplier2);
        await context.SaveChangesAsync();

        var now = DateTime.UtcNow;
        var drugs = new[]
        {
            new DrugCatalogEntity
            {
                DrugId = "D001", DrugName = "阿莫西林胶囊", TradeName = "阿莫仙",
                Specification = "0.25g*24粒", DosageForm = "胶囊剂", ApprovalNum = "国药准字H12345678",
                StorageCond = "密封阴凉干燥", PurchasePrice = 12.50m, RetailPrice = 18.00m,
                StockQuantity = 45, SupplierId = supplier1.SupplierId, IsActive = true
            },
            new DrugCatalogEntity
            {
                DrugId = "D002", DrugName = "布洛芬缓释胶囊", TradeName = "芬必得",
                Specification = "0.3g*20粒", DosageForm = "胶囊剂", ApprovalNum = "国药准字H23456789",
                StorageCond = "密封保存", PurchasePrice = 15.00m, RetailPrice = 22.00m,
                StockQuantity = 180, SupplierId = supplier1.SupplierId, IsActive = true
            },
            new DrugCatalogEntity
            {
                DrugId = "D003", DrugName = "维生素C片", TradeName = "",
                Specification = "100mg*100片", DosageForm = "片剂", ApprovalNum = "国药准字H34567890",
                StorageCond = "遮光密封", PurchasePrice = 8.00m, RetailPrice = 12.00m,
                StockQuantity = 320, SupplierId = supplier2.SupplierId, IsActive = true
            },
            new DrugCatalogEntity
            {
                DrugId = "D004", DrugName = "感冒灵颗粒", TradeName = "999感冒灵",
                Specification = "10g*9袋", DosageForm = "颗粒剂", ApprovalNum = "国药准字Z45678901",
                StorageCond = "密封", PurchasePrice = 10.50m, RetailPrice = 15.80m,
                StockQuantity = 25, SupplierId = supplier2.SupplierId, IsActive = true
            },
            new DrugCatalogEntity
            {
                DrugId = "D005", DrugName = "盐酸氨溴索口服液", TradeName = "沐舒坦",
                Specification = "100ml", DosageForm = "口服液", ApprovalNum = "国药准字H56789012",
                StorageCond = "遮光密封", PurchasePrice = 22.00m, RetailPrice = 32.00m,
                StockQuantity = 90, SupplierId = supplier1.SupplierId, IsActive = true
            }
        };
        context.Drugs.AddRange(drugs);
        await context.SaveChangesAsync();

        context.DrugBatches.AddRange(
            new DrugBatchEntity
            {
                DrugId = "D001", BatchNumber = "AMX202601", ManufactureDate = now.AddMonths(-6),
                ExpiryDate = now.AddDays(45), Quantity = 30
            },
            new DrugBatchEntity
            {
                DrugId = "D001", BatchNumber = "AMX202602", ManufactureDate = now.AddMonths(-3),
                ExpiryDate = now.AddDays(120), Quantity = 15
            },
            new DrugBatchEntity
            {
                DrugId = "D002", BatchNumber = "IBU202512", ManufactureDate = now.AddMonths(-4),
                ExpiryDate = now.AddDays(200), Quantity = 180
            },
            new DrugBatchEntity
            {
                DrugId = "D004", BatchNumber = "GM202508", ManufactureDate = now.AddMonths(-8),
                ExpiryDate = now.AddDays(25), Quantity = 25
            },
            new DrugBatchEntity
            {
                DrugId = "D005", BatchNumber = "AMB202603", ManufactureDate = now.AddMonths(-2),
                ExpiryDate = now.AddDays(60), Quantity = 90
            });

        var transactions = new List<StockTransactionEntity>();
        for (var i = 0; i < 15; i++)
        {
            transactions.Add(new StockTransactionEntity
            {
                DrugId = "D002",
                TransactionType = StockTransactionTypes.Out,
                Quantity = 3 + i % 5,
                Operator = "admin",
                CreatedAt = now.AddDays(-(i + 1)),
                Remark = "演示出库"
            });
        }
        for (var i = 0; i < 8; i++)
        {
            transactions.Add(new StockTransactionEntity
            {
                DrugId = "D001",
                TransactionType = StockTransactionTypes.Out,
                Quantity = 2,
                Operator = "admin",
                CreatedAt = now.AddDays(-(i + 2)),
                Remark = "演示出库"
            });
        }
        transactions.Add(new StockTransactionEntity
        {
            DrugId = "D003", TransactionType = StockTransactionTypes.In, Quantity = 100,
            Operator = "admin", CreatedAt = now.AddDays(-10), Remark = "演示入库"
        });
        context.StockTransactions.AddRange(transactions);
        await context.SaveChangesAsync();

        Console.WriteLine("演示数据：写入完成（5 种药品、批次、出库流水）。");
    }
}
