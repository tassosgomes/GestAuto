using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GestAuto.Commercial.Infra;

public class CommercialDbContextFactory : IDesignTimeDbContextFactory<CommercialDbContext>
{
    public CommercialDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CommercialDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=gestauto_commercial_dev;Username=postgres;Password=postgres");

        return new CommercialDbContext(optionsBuilder.Options);
    }
}