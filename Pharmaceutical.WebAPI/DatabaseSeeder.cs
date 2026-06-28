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
            
        try
        {
            await context.Database.MigrateAsync();
        }
        catch (MySqlException ex) when (ex.Message.Contains("already exists"))
        {
            // 表已存在，继续执行而不中断
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
    }
}
