using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GestAuto.Stock.Infra;

public class StockDbContextFactory : IDesignTimeDbContextFactory<StockDbContext>
{
    public StockDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<StockDbContext>();
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=gestauto;Username=gestauto;Password=gestauto123;Include Error Detail=true");

        return new StockDbContext(optionsBuilder.Options);
    }
}
