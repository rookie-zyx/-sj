using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Pharmaceutical.Infrastructure;

public class PharmaceuticalDbContextFactory : IDesignTimeDbContextFactory<PharmaceuticalDbContext>
{
    public PharmaceuticalDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PharmaceuticalDbContext>();
        optionsBuilder.UseMySql(
            "server=127.0.0.1;user=root;database=pharmaceutical;port=3306;password=18079587303;",
            new MySqlServerVersion(new Version(8, 0, 36)));

        return new PharmaceuticalDbContext(optionsBuilder.Options);
    }
}
